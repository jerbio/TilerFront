using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class getEventModel:AuthorizedUser
    {
        public string EventID { get; set; }
        public string ThirdPartyEventID { get; set; }
        public string ThirdPartyUserID { get; set; }
        public string ThirdPartyType { get; set; }
    }
}