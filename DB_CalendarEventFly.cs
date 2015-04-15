using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_CalendarEventFly:CalendarEvent
    {
        public DB_CalendarEventFly(string EventIDData, string Name, DateTimeOffset StartData, DateTimeOffset EndData, int PriorityData, Repetition RepetitionData, Location_Elements LocationData, TimeSpan TImePerSplitData, DateTimeOffset OriginalStartData, TimeSpan EventPrepTimeData, TimeSpan Event_PreDeadlineData, bool EventRigidFlagData, int SplitData, EventDisplay UiData, MiscData NoteData, bool CompletionFlagData, long RepeatIndexData, Procrastination ProcrastinationData, NowProfile NowProfileData, int CompleteCountData, int DeletionCountData,List<string> AllUserIDs)
        {
            EventName = Name;
            StartDateTime = StartData;
            EndDateTime = EndData;
            EventRepetition = RepetitionData;
            LocationInfo = LocationData;
            TimePerSplit = TImePerSplitData;
            OriginalStart = OriginalStartData;
            PrepTime = EventPrepTimeData;
            EventPreDeadline = Event_PreDeadlineData;
            ProfileOfNow = NowProfileData;
            RigidSchedule = EventRigidFlagData;
            Complete = CompletionFlagData;
            CompletedCount = CompleteCountData;
            DeletedCount = DeletionCountData;
            ProfileOfProcrastination = ProcrastinationData;
            SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            Priority = PriorityData;
            isRestricted = false;
            RepetitionSequence = 0;
            Splits = SplitData;
            UniqueID = new EventID(EventIDData);
            UserIDs=AllUserIDs;
            
            if(EventRepetition.Enable)
            {
                EventRepetition.PopulateRepetitionParameters(this);
            }
            else
            {
                
                DateTimeOffset SubEventEndData =  EndData;
                DateTimeOffset SubEventStartData =  SubEventEndData-TimePerSplit;
                int i = 0;
                int SubEventCount = Splits - (CompletedCount + DeletedCount);
                for (int j=0; j < SubEventCount; i++,j++)
                {
                    EventID SubEventID= EventID.GenerateSubCalendarEvent(UniqueID.ToString(),i+1);
                    SubCalendarEvent newSubCalEvent = new DB_SubCalendarEventFly(SubEventID, Name, SubEventStartData, SubEventEndData, PriorityData, LocationInfo.CreateCopy(), OriginalStart, EventPrepTimeData, Event_PreDeadlineData, EventRigidFlagData, UiData.createCopy(), NoteData.createCopy(), Complete, ProcrastinationData, NowProfileData, this.RangeTimeLine, EventRepetition.Enable, false, true, AllUserIDs.ToList(),0);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }

                for (int j = 0; j < CompletedCount; i++, j++)
                {
                    EventID SubEventID = EventID.GenerateSubCalendarEvent(UniqueID.ToString(), i + 1);
                    SubCalendarEvent newSubCalEvent = new DB_SubCalendarEventFly(SubEventID, Name, SubEventStartData, SubEventEndData, PriorityData, LocationInfo.CreateCopy(), OriginalStart, EventPrepTimeData, Event_PreDeadlineData, EventRigidFlagData, UiData.createCopy(), NoteData.createCopy(), Complete, ProcrastinationData, NowProfileData, this.RangeTimeLine, EventRepetition.Enable, true, true, AllUserIDs.ToList(), 0);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }

                for (int j = 0; j < DeletedCount; i++, j++)
                {
                    EventID SubEventID = EventID.GenerateSubCalendarEvent(UniqueID.ToString(), i + 1);
                    SubCalendarEvent newSubCalEvent = new DB_SubCalendarEventFly(SubEventID, Name, SubEventStartData, SubEventEndData, PriorityData, LocationInfo.CreateCopy(), OriginalStart, EventPrepTimeData, Event_PreDeadlineData, EventRigidFlagData, UiData.createCopy(), NoteData.createCopy(), Complete, ProcrastinationData, NowProfileData, this.RangeTimeLine, EventRepetition.Enable, true, false, AllUserIDs.ToList(), 0);
                    SubEvents.Add(newSubCalEvent.SubEvent_ID, newSubCalEvent);
                }
            }
        }

        public void UpdateModifiable(IEnumerable<SubCalendarEvent>Modifiables)
        {
            foreach(SubCalendarEvent eachSubCalendarEvent in Modifiables)
            {
                SubCalendarEvent mySubCalEvent=getSubEvent(eachSubCalendarEvent.SubEvent_ID);
                mySubCalEvent.UpdateThis(eachSubCalendarEvent);
            }
        }
    }
}