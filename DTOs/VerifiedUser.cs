using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerFront.DTOs
{
    public abstract class VerifiedUser
    {
        public string UserName { get; set; }
        public string UserID { get; set; }

        public string ClientId { get; set; }
        public string Secret { get; set; }
    }
}
