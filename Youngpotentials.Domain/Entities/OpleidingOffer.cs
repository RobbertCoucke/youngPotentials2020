﻿using System;
using System.Collections.Generic;

namespace Youngpotentials.Domain.Entities
{
    public partial class OpleidingOffer
    {
        public string IdOpleiding { get; set; }
        public int IdOffer { get; set; }

        public virtual Offers IdOfferNavigation { get; set; }
        public virtual Opleiding IdOpleidingNavigation { get; set; }
    }
}