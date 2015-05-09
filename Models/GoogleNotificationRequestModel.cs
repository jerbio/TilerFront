using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class GoogleNotificationRequestModel
    {
        public string id { get; set; }//"id": "01234567-89ab-cdef-0123456789ab", // Your channel ID.
        public string type { get; set; }//"type": "web_hook",
        public string address { get; set; }//"address": "https://mydomain.com/notifications", // Your receiving URL.
        public string token{get;set;}//"token": "target=myApp-myCalendarChannelDest", // (Optional) Your channel token.
        public string expiration { get; set; }//"expiration": 1426325213000 // (Optional) Your requested channel expiration time.
    }
}