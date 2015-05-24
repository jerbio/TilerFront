using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class getEventModel:AuthorizedUser
    {
        /// <summary>
        /// ID of the Event in tiler format. It should be in the format a_b_c
        /// </summary>
        public string EventID { get; set; }
        /// <summary>
        /// ID of the third party service. The ID is provided from the 3rd party
        /// </summary>
        public string ThirdPartyEventID { get; set; }
        /// <summary>
        /// User ID to the third party service for which this event was created from
        /// </summary>
        public string ThirdPartyUserID { get; set; }
        /// <summary>
        /// The type or provider of this scheduling service(e.g: google, outlook).
        /// </summary>
        public string ThirdPartyType { get; set; }
    }
}