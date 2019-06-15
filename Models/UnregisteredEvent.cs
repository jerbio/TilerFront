using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

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
        /// <summary>
        /// Day-date component of End date.
        /// </summary>
        public string EndDay { get; set; }
        /// <summary>
        /// Hour component of End date.
        /// </summary>
        public string EndHour { get; set; }
        /// <summary>
        /// Minute component of End date.
        /// </summary>
        public string EndMins { get; set; }
        /// <summary>
        /// Month component of End date.
        /// </summary>
        public string EndMonth { get; set; }
        /// <summary>
        /// Year component of End date.
        /// </summary>
        public string EndYear { get; set; }
        /// <summary>
        /// Full Address for new event. Fully described in default format. e.g 1234 stret apt 56 Kingston, CO 78901
        /// </summary>
        public string LocationAddress { get; set; }
        /// <summary>
        /// Should be populated when the location is from a cache
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Prefereed Nick name for location. If Nick name already exists, it overwrites previous full address & long lat with new nick name
        /// </summary>
        public string LocationTag { get; set; }
        /// <summary>
        /// Name of Newly added Tile/Event
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ***No current use***
        /// </summary>
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
        public string isEveryDay { get; set; }
        public RestrictionWeekConfig RestrictiveWeek { get; set; }
        /// <summary>
        /// The timezone of the location where the request is received. This defaults to UTC
        /// </summary>
        public string TimeZoneOrigin { get; set; } = "UTC";

        public RestrictionProfile getRestrictionProfile(DateTimeOffset currentTime)
        {
            bool EveryDayFlag = Boolean.Parse(isEveryDay);
            bool WorkWeek = Boolean.Parse(isWorkWeek);
            DateTimeOffset myNow = currentTime;
            DateTimeOffset RestrictStart = new DateTimeOffset(myNow.Year, myNow.Month, myNow.Day, 0,0, 0,new TimeSpan());
            RestrictStart = RestrictStart.Add(-getTImeSpan);
            DateTimeOffset RestrictEnd = RestrictStart.AddSeconds(-1);
            RestrictionProfile retValue;
            DayOfWeek[] selectedDaysOftheweek = { };
            if(EveryDayFlag||WorkWeek)
            {
                if ((DateTimeOffset.TryParse(RestrictionStart, out RestrictStart)) && ((DateTimeOffset.TryParse(RestrictionEnd, out RestrictEnd))))
                {
                    RestrictStart = RestrictStart.Add(getTImeSpan);
                    RestrictEnd = RestrictEnd.Add(getTImeSpan);
                    selectedDaysOftheweek = RestrictionProfile.AllDaysOfWeek.ToArray();
                    if (WorkWeek)
                    {
                        retValue = new RestrictionProfile(7, DayOfWeek.Monday, RestrictStart, RestrictEnd);
                    }
                    else
                    {
                        RestrictionTimeLine RestrictionTimeLine = new TilerElements.RestrictionTimeLine(RestrictStart, RestrictEnd);
                        retValue = new RestrictionProfile(selectedDaysOftheweek, RestrictionTimeLine);
                    }
                    return retValue;
                }
                else
                {
                    retValue = null;
                }
            }


            retValue = RestrictiveWeek.getRestriction(getTImeSpan);

            
            return retValue;
        }
        
    }
}