using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using DBTilerElement;
using TilerFront.Models;

namespace TilerFront
{
    public class PeekResult
    {
        public PeekDay[] PeekDays;
        public int ConflictingCount;
        public PeekResult(IEnumerable<IEnumerable<SubCalendarEvent>>AllInterferringEvents,DayTimeLine[] DayReferences, IEnumerable<SubCalendarEvent>ConflictingEvents)
        {
            PeekDays = AllInterferringEvents.Select((obj,i) =>{
               
                TimeLine myDay = DayReferences[i];
                TimeLine myTimeLine = new TimeLine(myDay.Start, myDay.End);
                myTimeLine.AddBusySlots(obj.Select(obj1 => obj1.ActiveSlot));
                TimeSpan SleepSpan = myTimeLine.getAllFreeSlots().Max(obj1 => obj1.TimelineSpan);
                TimeSpan TotalActiveSpan = SubCalendarEvent.TotalActiveDuration(obj);
                double ActiveTimeDuration =TotalActiveSpan.TotalMilliseconds;
                double durationRatio = ActiveTimeDuration/DayReferences[i].TimelineSpan.TotalMilliseconds;
                var notBlobSubevent = obj.Where(o => !o.isBlobEvent).ToList();
                var allSubEvents = notBlobSubevent.Concat(obj.Where(o => o.isBlobEvent).SelectMany(o => (o as BlobSubCalendarEvent).getSubCalendarEventsInBlob()));
                PeekDay MyPeekDay = new PeekDay { DayIndex = (int)myDay.Start.DayOfWeek, AllSubEvents = allSubEvents.Select(obj1 => obj1.ToSubCalEvent()).ToArray(), TotalDuration = TotalActiveSpan.TotalHours, DurationRatio = durationRatio, SleepTime = SleepSpan.TotalHours };
                return MyPeekDay;
            }).ToArray();
            ConflictingCount = ConflictingEvents.Count();
        }
        
        
    }

    public class PeekDay
    {
        public SubCalEvent[] AllSubEvents { get; set; }
        public double TotalDuration { get; set; }
        public double DurationRatio { get; set; }
        public double SleepTime { get; set; }
        public int DayIndex { get; set; }
    }
}