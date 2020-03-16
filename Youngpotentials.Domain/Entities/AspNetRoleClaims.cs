﻿using System;
using System.Collections.Generic;

namespace Youngpotentials.Domain.Entities
{
    public partial class AspNetRoleClaims
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        public AspNetRoles Role { get; set; }
    }
}
