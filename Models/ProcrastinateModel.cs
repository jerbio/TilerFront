using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class ProcrastinateModel : AuthorizedUser
    {
        public string UserName { get; set; }
        public string UserID { get; set; }
        public bool MobileFlag { get; set; }
        public int TimeZoneOffset { get; set; }

        public long DurationDays { get; set; }
        public long DurationHours { get; set; }
        public long DurationMins { get; set; }

        public AuthorizedUser User 
        {
            get 
            {
                return new AuthorizedUser { UserName = UserName, UserID = UserID, MobileFlag = MobileFlag, TimeZoneOffset = TimeZoneOffset };
            }
        }

        public TimeDuration ProcrastinateDuration 
        {
            get 
            {
                return new TimeDuration { DurationDays = DurationDays, DurationHours = DurationHours, DurationMins = DurationMins };
            }
        }

    }
}