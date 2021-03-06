﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Youngpotentials.Domain.Models.Responses;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YoungpotentialsAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UploadController : Controller
    {
        // GET: /<controller>/
        
        public IActionResult Index()
        {
            return View();
        }


        /// <summary>
        /// check if user/offer has a file attached and return the path of the file
        /// </summary>
        [HttpGet("file/{isUser}/{id}")]
        public IActionResult GetFile(bool isUser, int id)
        {


            try
            {
                var folderPath = System.IO.Path.Combine("Resources", (isUser ? "users" : "offers"));
                var filePath = System.IO.Path.Combine(folderPath, id.ToString());
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                if (!System.IO.Directory.Exists(fullPath))
                {
                    return Ok(null);
                }

                var filename = Directory.GetFiles(fullPath).First().Split('\\').Last();



                return Ok(new UploadResponse { path = filename });
            }
            catch(Exception e)
            {
                return BadRequest();
            }

        }

        /// <summary>
        /// //get the attached file of an user/offer
        /// </summary>
        /// <param name="isUser"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("download/{isUser}/{id}")]
        public IActionResult Download(bool isUser, int id)
        {

            var folderPath = System.IO.Path.Combine("Resources", (isUser ? "users" : "offers"));
            var filePath = System.IO.Path.Combine(folderPath, id.ToString());
            //var fileFullPath = Path.Combine(filePath, fileName);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);

            var fileName = Directory.GetFiles(fullPath).First();

            var fullFilePath = System.IO.Path.Combine(fullPath, fileName);


            if (!System.IO.File.Exists(fullFilePath))
            {
                return Ok(null);
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(fullFilePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            var file = File(memory, "application/pdf", fileName);

            return file;

        }


        /// <summary>
        /// delete the attached file of an user/offer
        /// </summary>
        /// <param name="isUser"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{isUser}/{id}")]
        public IActionResult delete(bool isUser, int id)
        {

            var folderPath = System.IO.Path.Combine("Resources", (isUser ? "users" : "offers"));
            var filePath = System.IO.Path.Combine(folderPath, id.ToString());
            //var fileFullPath = Path.Combine(filePath, fileName);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);



            System.IO.DirectoryInfo di = new DirectoryInfo(fullPath);

            foreach (FileInfo f in di.GetFiles())
            {
                f.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            return Ok();

        }


        /// <summary>
        /// attach a file to an user/offer
        /// </summary>
        /// <returns></returns>
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult Upload( )
        {

            try
            {
                var file = Request.Form.Files[0];

                var isUser = Boolean.Parse(Request.Form["isUser"]);
                var id = Int32.Parse(Request.Form["id"]);



                var folderPath = System.IO.Path.Combine("Resources", (isUser ? "users" : "offers" ));
                var filePath = System.IO.Path.Combine(folderPath, id.ToString());
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), filePath);
                Directory.CreateDirectory(pathToSave);

                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(filePath, fileName);

                    //user or vacature can only have 1 file associated so we delete all files and folders in the userId/vacatureId folder.
                    System.IO.DirectoryInfo di = new DirectoryInfo(pathToSave);

                    foreach (FileInfo f in di.GetFiles())
                    {
                        f.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "internal server error");
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

    }
}
