using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class TimeDuration
    {
        public long DurationDays { get; set; }
        public long DurationHours { get; set; }
        public long DurationMins { get; set; }
        public long DurationInMs { get; set; } = -1;

        public TimeSpan TotalTimeSpan
        {
            get 
            {
                TimeSpan retValue;
                if(DurationInMs == -1)
                {
                    retValue = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
                } else
                {
                    retValue = TimeSpan.FromMilliseconds(DurationInMs);
                }
                    
                return retValue;
            }
        }
    }

}