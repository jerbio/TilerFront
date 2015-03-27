using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using TilerFront.Models;

namespace TilerFront
{
    public class PeekResult
    {
        PeekDay[] PeekDays;
        int ConflictingCount;
        public PeekResult(IEnumerable<IEnumerable<SubCalendarEvent>>AllInterferringEvents,DayTimeLine[] DayReferences, IEnumerable<SubCalendarEvent>ConflictingEvents)
        {
            PeekDays = AllInterferringEvents.Select((obj,i) =>{
               
                double ActiveTimeDuration =(SubCalendarEvent.TotalActiveDuration(obj).TotalMilliseconds);
                double durationRatio = ActiveTimeDuration/DayReferences[i].TimelineSpan.TotalMilliseconds;
                PeekDay MyPeekDay = new PeekDay { AllSubEvents = obj.Select(obj1 => obj1.ToSubCalEvent()).ToArray(), TotalDuration = ActiveTimeDuration, DurationRatio = durationRatio };
                return MyPeekDay;
            }).ToArray();
            ConflictingCount = ConflictingEvents.Count();
        }
        
        
    }

    class PeekDay
    {
        public SubCalEvent[] AllSubEvents { get; set; }
        public double TotalDuration { get; set; }
        public double DurationRatio { get; set; }
        public double SleepTime { get; set; }
    }
}