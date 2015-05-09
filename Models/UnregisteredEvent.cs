using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    /// <summary>
    /// Represents an Event that has not being added to a users schedule. 
    /// </summary>
    public class UnregisteredEvent:AuthorizedUser
    {
        /// <summary>
        /// Blue Value for RGBO color format  for calendar event
        /// </summary>
        public string BColor { get; set; }
        /// <summary>
        /// Red Value for RGBO color format  for calendar event
        /// </summary>
        public string RColor { get; set; }
        /// <summary>
        /// Green Value for RGBO color format for calendar event
        /// </summary>
        public string GColor { get; set; }
        /// <summary>
        /// Sets the opacity RGBO color format for calendar event
        /// </summary>
        public string Opacity { get; set; }
        /// <summary>
        /// One of the preset color selections for tiler. The preset options are from [0-8].
        /// </summary>
        public string ColorSelection { get; set; }
        /// <summary>
        /// The number of splits for a specific calendar event. Default is 1.
        /// </summary>
        public string Count { get; set; }
        /// <summary>
        /// Sets the number of days for duration component of the given calendar event. Default is 0
        /// </summary>
        public string DurationDays { get; set; }
        /// <summary>
        /// Sets the number of hours component for the duration of event. for the given calendar event. Default is 0
        /// </summary>
        public string DurationHours { get; set; }
        /// <summary>
        /// Sets the number of Minutes component for the duration of event. for the given calendar event. Default is 0
        /// </summary>
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
        /// <summary>
        /// is time restriction set on this event
        /// </summary>
        public string isRestricted { get; set; }
        /// <summary>
        /// Start time for restriction
        /// </summary>
        public string RestrictionStart {get;set;}
        /// <summary>
        /// End time for restriction
        /// </summary>
        public string RestrictionEnd {get;set;}
        /// <summary>
        /// is the restrcition to be for only work week.
        /// </summary>
        public string isWorkWeek {get;set;} 
        
    }
}