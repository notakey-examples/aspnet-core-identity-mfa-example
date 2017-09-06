using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentitySample.Models
{
    public class NotakeyAuthState
    {
        public bool isExpired { get; set; }

        public bool isValid { get; set; }

		public bool isProcessed { get; set; }

        public bool isApproved { get; set; }
    }
}
