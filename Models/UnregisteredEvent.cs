using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class UnregisteredEvent:AuthorizedUser
    {
        public string BColor { get; set; }
        public string RColor { get; set; }
        public string GColor { get; set; }
        public string Opacity { get; set; }
        public string ColorSelection { get; set; }
        public string Count { get; set; }
        public string DurationDays { get; set; }
        public string DurationHours { get; set; }
        public string DurationMins { get; set; }
        public string EndDay { get; set; }
        public string EndHour { get; set; }
        public string EndMins { get; set; }
        public string EndMonth { get; set; }
        public string EndYear { get; set; }
        public string LocationAddress { get; set; }
        public string LocationTag { get; set; }
        public string Name { get; set; }
        public string RepeatData { get; set; }
        public string RepeatEndDay { get; set; }
        public string RepeatEndMonth { get; set; }
        public string RepeatEndYear { get; set; }
        public string RepeatStartDay { get; set; }
        public string RepeatStartMonth { get; set; }
        public string RepeatStartYear { get; set; }
        public string RepeatType { get; set; }
        public string RepeatWeeklyData { get; set; }
        public string Rigid { get; set; }
        public string StartDay { get; set; }
        public string StartHour { get; set; }
        public string StartMins { get; set; }
        public string StartMonth { get; set; }
        public string StartYear { get; set; }
        public string RepeatFrequency { get; set; }
    }
}