using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class UserSchedule
    {
        /*
        string Delimited = string.Join(",", arg1.Repeat.RecurringCalendarEvents.Select(obj => CalendarEventToString(obj, RangeOfSchedule, TimeZoneSpan)));
        string AllCalEventString = "["+Delimited+"]";
        TotalString = "{\"ID\":" + IDString + ",\"RepeatCalendarName\":" + RepeatCalendarName + ",\"RepeatStartDate\":" + RepeatStartDate + ",\"RepeatEndDate\":" + RepeatEndDate + ",\"RepeatTotalDuration\":" + RepeatTotalDuration + ",\"RepeatRigid\":" + RepeatRigid + ",\"RepeatAddressDescription\": " + RepeatAddressDescription + ",\"RepeatAddress\":" + RepeatAddress + ",\"RepeatCalendarEvents\":" + AllCalEventString + ",\"Latitude\":" + Lat + ",\"Longitude\":" + Long + "}";
        */

        public IList<repeatedEventData> RepeatCalendarEvent { get; set; }
        public IList<CalEvent> NonRepeatCalendarEvent { get; set; }

        public class repeatedEventData
        {
            public string ID { get; set; }
            public string RepeatCalendarName { get; set; }
            public DateTimeOffset RepeatStartDate { get; set; }
            public DateTimeOffset RepeatEndDate { get; set; }
            public TimeSpan RepeatTotalDuration { get; set; }
            public bool RepeatRigid { get; set; }
            public string RepeatAddressDescription { get; set; }
            public string RepeatAddress { get; set; }
            public IList<CalEvent> RepeatCalendarEvents { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }


    }
}