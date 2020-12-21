using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class UpdateTriggerModel : AuthorizedUser
    {
        public bool IsInitialized { get; set; } = false;
        public AuthorizedUser User
        {
            get
            {
                return new AuthorizedUser { UserName = UserName, UserID = UserID, MobileFlag = MobileFlag, TimeZoneOffset = TimeZoneOffset , UserLongitude = this.UserLongitude, UserLatitude = this.UserLatitude, UserLocationVerified = this.UserLocationVerified };
            }
        }
    }
}