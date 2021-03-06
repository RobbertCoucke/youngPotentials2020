﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Youngpotentials.Domain.Models.Requests
{
    public class UserRegistrationRequest
    {

        //USER

        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public string Telephone { get; set; }


        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Address { get; set; }


        //COMPANY
        public string Description { get; set; }
        public string Url { get; set; }
        public string CompanyName { get; set; }
        public int SectorId { get; set; }

        //STUDENT

        public string Name { get; set; }
        public string FirstName { get; set; }
        public string CvUrl { get; set; }

        [Required]
        public bool IsStudent { get; set; }




    }
}
