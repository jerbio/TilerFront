using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_SubCalendarEvent : SubCalendarEvent
    {
        public DB_SubCalendarEvent(string MySubEventID, BusyTimeLine MyBusylot, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, DateTimeOffset OriginalStartData, string myParentID, bool Rigid, bool Enabled, EventDisplay UiParam, MiscData Notes, bool completeFlag, Location_Elements EventLocation = null, TimeLine RangeOfSubCalEvent = null, ConflictProfile conflicts = null)
        {
            if (conflicts == null)
            {
                conflicts = new ConflictProfile();
            }
            ConflictingEvents = conflicts;
            OriginalStart = OriginalStartData;
            CalendarEventRange = RangeOfSubCalEvent;
            //string eventName, TimeSpan EventDuration, DateTimeOffset EventStart, DateTimeOffset EventDeadline, TimeSpan EventPrepTime, TimeSpan PreDeadline, bool EventRigidFlag, bool EventRepetition, int EventSplit
            StartDateTime = EventStart;
            EndDateTime = EventDeadline;
            EventDuration = MyBusylot.End - MyBusylot.Start;
            BusyFrame = MyBusylot;
            PrepTime = EventPrepTime;
            UniqueID = new EventID(MySubEventID);
            this.LocationInfo = EventLocation;
            RepetitionSequence = 0;
            UiParams=UiParam;
            DataBlob= Notes;
            Complete = completeFlag;

            this.Enabled = Enabled;
            //EventSequence = new EventTimeLine(UniqueID.ToString(), StartDateTime, EndDateTime);
            RigidSchedule = Rigid;
        }
    }
}