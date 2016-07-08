using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBTilerElement;
using Newtonsoft.Json.Linq;

namespace TilerFront.Models
{
    public class UserSchedule
    {
        public IList<repeatedEventData> RepeatCalendarEvent { get; set; }
        public IList<CalEvent> NonRepeatCalendarEvent { get; set; }
        public JObject PauseData { get; set; } = new JObject();
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

        public void populatePauseData(PausedEvent PausedEvent)
        {
            long RangeStart = (long)(TilerElementExtension.CurrentTime - TilerElementExtension.JSStartTime).TotalMilliseconds;
            long RangeEnd = RangeStart + (long)TimeSpan.FromDays(1).TotalMilliseconds;
            Func<SubCalEvent, bool> predicate = (subEvent => 
            {
                if ((RangeStart <= subEvent.SubCalEndDate) && (RangeEnd >= subEvent.SubCalStartDate))
                {
                    return true;
                }
                return false;
            });
            List<SubCalEvent> allSubEvents = this.RepeatCalendarEvent.SelectMany(obj => obj.RepeatCalendarEvents.SelectMany(CalEvent => CalEvent.AllSubCalEvents)).Concat(NonRepeatCalendarEvent.SelectMany(obj => obj.AllSubCalEvents)) .ToList();
            List<SubCalEvent> subEvents = new List<SubCalEvent>();
            subEvents = this.RepeatCalendarEvent.SelectMany(obj => obj.RepeatCalendarEvents.SelectMany(CalEvent => CalEvent.AllSubCalEvents.Where(predicate))).ToList();
            subEvents = subEvents.Concat(NonRepeatCalendarEvent.SelectMany(obj => obj.AllSubCalEvents.Where(predicate))).OrderBy(subEvent => subEvent.SubCalStartDate).ToList();
            SubCalEvent pausedSubEVent = allSubEvents.FirstOrDefault(obj => obj.isPaused);


            //creates paused event from the list of events derived
            if((pausedSubEVent != null) && (PausedEvent == null))
            {
                PausedEvent = new PausedEvent()
                {
                    EventId = pausedSubEVent.ID,
                    isPauseDeleted = pausedSubEVent.isEnabled
                };
            }
            subEvents = subEvents.Take(10).ToList();
            dynamic RetValue = new JObject();
            RetValue.pausedEvent = PausedEvent == null? null : JObject.FromObject(PausedEvent);
            RetValue.subEvents = JArray.FromObject(subEvents);
            this.PauseData = RetValue as JObject;
        }


    }
}