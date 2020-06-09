using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class DumpModel : AuthorizedUser
    {
        public string Notes { get; set; }
        public AuthorizedUser User
        {
            get
            {
                return new AuthorizedUser { UserName = UserName, UserID = UserID, MobileFlag = MobileFlag, TimeZoneOffset = TimeZoneOffset , UserLongitude = this.UserLongitude, UserLatitude = this.UserLatitude, UserLocationVerified = this.UserLocationVerified };
            }
        }

        public string Id { get; set; }
    }
}