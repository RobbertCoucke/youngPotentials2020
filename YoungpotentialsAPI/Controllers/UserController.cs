﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Youngpotentials.Domain.Models.Responses;
using Youngpotentials.Domain.Models.Requests;
using Youngpotentials.Service;
using AutoMapper;
using YoungpotentialsAPI.Helpers;
using Youngpotentials.Domain.Entities;
using Microsoft.AspNetCore.Cors;
using YoungpotentialsAPI.Models.Requests;

namespace YoungpotentialsAPI.Controllers
{

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private IUserService _userService;
        private IStudentService _studentService;
        private IRoleService _roleService;
        private ICompanyService _companyService;
        private readonly AppSettings _appSettings;
        private IMapper _mapper;
        private EmailService _mailService = new EmailService();

        public UserController(IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings, IStudentService studentService, ICompanyService companyService, IRoleService roleService)
        {
            _userService = userService;
            _companyService = companyService;
            _studentService = studentService;
            _appSettings = appSettings.Value;
            _mapper = mapper;
            _roleService = roleService;
        }



        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Email, model.Password);

            if (user == null)
            {
                return BadRequest(new { message = "email or password is incorrect" });
            }

            var role = user.Role.Name;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var claims = new Claim(ClaimTypes.Role, "Admin");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    //ipv "Admin" role ophalen van user
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new AuthenticationResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role.Name,
                Token = tokenString
            });

        }

        [Authorize(Roles = "User")]
        [HttpGet("test")]
        public JsonResult Test()
        {
            return Json("het werk woehoe!");
        }


        //[HttpGet("password/{email}")]
        //public async void ResetEmail(string email)
        //{
        //    var user = _userService.GetUserByEmail(email);
        //    if (user != null)
        //    {
        //        var body = "klik op deze link om een nieuw passwoord in te stellen: Click <a href=\"http://myAngularSite/passwordReset?code= " + user.Code + "\>here</a>";
        //        await _mailService.sendEmailAsync(email, "testEmail", "password reset", body);

        //    }
        //}
        [AllowAnonymous]
        [HttpPost("password")]
        public async Task<IActionResult> PasswordResetAsync([FromBody] EmailRequest e)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            //var claims = new Claim(ClaimTypes.Role, "Admin");

            var user = _userService.GetUserByEmail(e.Email); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    //ipv "Admin" role ophalen van user
                    
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            if (user != null)
            {
                var href = $"<a href='http://localhost:4200/wachtwoord-reseten?email={user.Email}&token={tokenString}>";
                var message = "klik op deze link om een nieuw passwoord in te stellen: Click" + href;
                var emailService = new EmailService();
                await emailService.sendEmailAsync("ibrahemhajkasem@gmail.com", "george.desmet1998@gmail.com", "reset password", message);
            }
            else
            {
                return BadRequest("Incorrect Email");
            }
            //var result = _userService.ResetPassword(user, req.password);
            return Ok(tokenString);

        }

        [AllowAnonymous]
        [HttpPost("password/reset")]
        public IActionResult ResetPassword([FromBody] PasswordResetRequest passwordResetRequest)
        {
            var user = _userService.GetUserByEmail(passwordResetRequest.email);
            if(user != null & passwordResetRequest.token != null)
            {
                _userService.ResetPassword(user, passwordResetRequest.newPassword);
                return Ok();
            }
            else
            {
                return BadRequest("Invalid gegevens");
            }

            return Ok();
  
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserRegistrationRequest model)
        {
            var user = _mapper.Map<AspNetUsers>(model);

            try {

                if (model.IsStudent)
                    user.RoleId = _roleService.GetRoleByName("User").Id;
                else
                    user.RoleId = _roleService.GetRoleByName("Company").Id;

                user = _userService.Create(user, model.Password);



                if (model.IsStudent)
                {
                    var student = _mapper.Map<Students>(model);
                    student.UserId = user.Id;
                    _studentService.CreateStudent(student);

                }
                else
                {
                    var company = _mapper.Map<Companies>(model);
                    company.UserId = user.Id;
                    _companyService.CreateCompany(company);

                    //TODO send mail to admin that new company has registered and has yet to be verified
                }

                var role = user.Role.Name;

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var claims = new Claim(ClaimTypes.Role, "Admin");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, role)
                }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),

                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);


                return Ok(new AuthenticationResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role.Name,
                    Token = tokenString
                });


            }
            catch (Exception e)
            {
                return BadRequest(new { message = e.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getAll")]
        public IActionResult GetAll()
        {
            var result = new List<UserResponse>();
            var users = _userService.GetAll();
            foreach (var user in users)
            {
                var model = _mapper.Map<UserResponse>(user);
                result.Add(model);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);
            UserResponse model = null;
            if (user != null)
            {
                var userResponse = _mapper.Map<UserResponse>(user);
                var roleId = user.RoleId;
                //user is student
                if (roleId == 2)
                {

                    var student = _studentService.GetStudentByUserId(user.Id);
                    if (student != null)
                    {

                        model = _mapper.Map<StudentResponse>(student);

                        model.Email = student.User.Email;
                        model.Address = student.User.Address;
                        model.City = student.User.City;
                        model.Telephone = student.User.Telephone;
                        model.ZipCode = student.User.ZipCode;
                        model.UserId = student.User.Id;
                        model.IsStudent = true;
                    }

                }
                else if (roleId == 3)
                {
                    var company = _companyService.GetCompanyByUserId(user.Id);
                    if (company != null)
                    {
                        model = _mapper.Map<CompanyResponse>(company);
                        model.UserId = company.User.Id;
                        model.Email = company.User.Email;
                        model.City = company.User.City;
                        model.Address = company.User.Address;
                        model.Telephone = company.User.Telephone;
                        model.ZipCode = company.User.ZipCode;
                        model.IsStudent = false;
                    }

                }
            }

            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserUpdateRequest model)
        {

            var u = _mapper.Map<AspNetUsers>(model);
            u.Id = id;
            var user = _userService.GetById(id);
            if (user != null)
            {
                try
                {
                    if (user.RoleId == 2)
                    {

                        var student = _studentService.GetStudentByUserId(user.Id);
                        student.User.Address = u.Address;
                        student.User.City = u.City;
                        student.User.Email = u.Email;
                        student.User.Telephone = u.Telephone;
                        student.User.ZipCode = u.ZipCode;
                        student.FirstName = model.FirstName;
                        student.Name = model.Name;
                        student.UserId = u.Id;
                        _studentService.UpdateStudent(student);
                        return Ok();

                    }
                    else if (user.RoleId == 3)
                    {
                        var company = _companyService.GetCompanyByUserId(user.Id);
                        company.User.Address = u.Address;
                        company.User.City = u.City;
                        company.User.Email = u.Email;
                        company.User.Telephone = u.Telephone;
                        company.User.ZipCode = u.ZipCode;
                        company.Description = model.Description;
                        company.Url = model.Url;
                        company.UserId = u.Id;
                        company.CompanyName = model.CompanyName;
                        //_userService.Update(u, "");
                        _companyService.UpdateCompany(company);
                        //var sf = _companyService.GetCompanyByUserId(u.Id);
                        return Ok();
                    }
                }
                catch(Exception e)
                {
                    return BadRequest(new { message = e.Message });
                }

            }
            return Ok();
    }
            

            
            

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);
            return Ok();
        }

    }
}