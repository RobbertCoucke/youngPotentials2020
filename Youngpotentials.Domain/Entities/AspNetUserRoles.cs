﻿using System;
using System.Collections.Generic;

namespace Youngpotentials.Domain.Entities
{
    public partial class AspNetUserRoles
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }

        public AspNetRoles Role { get; set; }
    }
}
