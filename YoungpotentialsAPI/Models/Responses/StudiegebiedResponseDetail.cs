﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YoungpotentialsAPI.Models.Responses
{
    public class StudiegebiedResponseDetail : StudiegebiedResponse
    {
        public IEnumerable<OpleidingResponseDetail> Opleiding { get; set; }
    }
}