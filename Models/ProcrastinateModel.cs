using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront.Models
{
    public class ProcrastinateModel : AuthorizedUser
    {
        public long DurationDays { get; set; }
        public long DurationHours { get; set; }
        public long DurationMins { get; set; }
        public long DurationInMs { get; set; } = -1;
        public long StartInMs { get; set; } = -1;
        public long EndInMs { get; set; } = -1;
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

        public TimeLine getProcrastinateTimeLine()
        {
            if (this.StartInMs > 0)
            {
                if( this.EndInMs > 0)
                {
                    DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(this.StartInMs);
                    DateTimeOffset end = DateTimeOffset.FromUnixTimeMilliseconds(this.EndInMs);
                    TimeLine retValue = new TimeLine(start, end);
                    return retValue;
                } else
                {
                    TimeDuration duration = this.ProcrastinateDuration;
                    DateTimeOffset start = DateTimeOffset.FromUnixTimeMilliseconds(this.StartInMs);
                    DateTimeOffset end = start.Add(duration.TotalTimeSpan);
                    TimeLine retValue = new TimeLine(start, end);
                    if (retValue.TimelineSpan.Ticks != 0)
                        return retValue;
                }
            }

            return null;
        }
    }
}