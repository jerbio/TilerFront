using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class ProcrastinateModel : AuthorizedUser
    {
        public long DurationDays { get; set; }
        public long DurationHours { get; set; }
        public long DurationMins { get; set; }
        public long DurationInMs { get; set; } = -1;
        public string FormattedAsISO8601 { get; set; }

        public AuthorizedUser User 
        {
            get 
            {
                return new AuthorizedUser { UserName = UserName, UserID = UserID, MobileFlag = MobileFlag, TimeZoneOffset = TimeZoneOffset , UserLongitude = this.UserLongitude, UserLatitude = this.UserLatitude, UserLocationVerified = this.UserLocationVerified };
            }
        }

        public TimeDuration ProcrastinateDuration 
        {
            get 
            {
                TimeDuration retValue;
                if(DurationInMs!=-1)
                {
                    retValue = new TimeDuration
                    {
                        DurationInMs = DurationInMs
                    };
                } else
                {
                    retValue = new TimeDuration { DurationDays = DurationDays, DurationHours = DurationHours, DurationMins = DurationMins };
                }

                return retValue;
            }
        }
    }
}