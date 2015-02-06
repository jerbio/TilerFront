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

        public TimeSpan TotalTimeSpan
        {
            get 
            {
                TimeSpan fullTimeSpan = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
                return fullTimeSpan;
            }
        }
    }

}