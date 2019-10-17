using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class ShuffleModel : AuthorizedUser
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
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