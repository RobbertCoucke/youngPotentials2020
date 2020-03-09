﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Youngpotentials.Domain.Entities;

namespace Youngpotentials.Domain.Models.Requests
{
    public class CreateOfferRequest
    {

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime? ExpirationDate { get; set; }
        [Required]
        public int CompanyId { get; set; }
        [Required]
        public int TypeId { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public bool? Country { get; set; }

        public IList<Studiegebied> Tags {get; set;}
    }
}
