using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class DB_SubCalendarEventFly:SubCalendarEvent
    {
        DB_SubCalendarEventFly()
        { }
        internal DB_SubCalendarEventFly(EventID EventIDData, string Name, DateTimeOffset StartData, DateTimeOffset EndData, int PriorityData, Location_Elements LocationData, DateTimeOffset OriginalStartData, TimeSpan EventPrepTimeData, TimeSpan Event_PreDeadlineData, bool EventRigidFlagData, EventDisplay UiData, MiscData NoteData, bool CompletionFlagData, Procrastination ProcrastinationData, NowProfile NowProfileData, TimeLine CalendarEventRangeData, bool FromRepeatFlagData, bool ElapsedFlagData, bool EnabledFlagData, List<string> UserIDData,long Sequence)
        {
            this.BusyFrame = new BusyTimeLine(EventIDData.ToString(), StartData, EndData);
            this.CalendarEventRange = CalendarEventRangeData;
            this.FromRepeatEvent = FromRepeatFlagData;
            this.EventName = Name;
            this.EventDuration = BusyFrame.BusyTimeSpan;
            this.Complete = CompletionFlagData;
            this.ConflictingEvents = new ConflictProfile();
            this.DataBlob = NoteData;
            this.DeadlineElapsed = ElapsedFlagData;
            this.Enabled = EnabledFlagData;
            this.StartDateTime = StartData;
            this.EndDateTime = EndData;
            this.EventPreDeadline = Event_PreDeadlineData;
            this.RepetitionSequence = Sequence;
            this.LocationInfo = LocationData;
            //this.OldPreferredIndex = mySubCalEvent.OldUniversalIndex;
            //this.otherPartyID = mySubCalEvent.ThirdPartyID;
            //this.preferredDayIndex = mySubCalEvent.UniversalDayIndex;
            this.PrepTime = EventPrepTimeData;
            this.Priority = PriorityData;
            this.ProfileOfNow = NowProfileData;
            this.ProfileOfProcrastination = ProcrastinationData;
            //this.RepetitionFlag = mySubCalEvent.FromRepeat;
            this.RigidSchedule = EventRigidFlagData;

            this.UiParams = UiData;
            this.UniqueID = EventIDData;

            this.UserIDs = UserIDData;
            this.OriginalStart = OriginalStartData;
        }

        internal void InitializeDisabled(SubCalendarEvent SubCalendarEventData)
        {
            if (!SubCalendarEventData.isEnabled)
            {
                TimeSpan SPanShift = SubCalendarEventData.Start - this.Start;
                this.shiftEvent(SPanShift, true);
                this.Enabled = SubCalendarEventData.isEnabled;
                return;
            }
            throw new Exception("Trying to set undelete event as deleted, check DB_SubCalendarEventFly");
        }

        internal void InitializeNowProfile(SubCalendarEvent SubCalendarEventData)
        {
            if (SubCalendarEventData.NowInfo.isInitialized)
            {
                TimeSpan SPanShift = SubCalendarEventData.Start - this.Start;
                this.shiftEvent(SPanShift, true);
                this.Enabled = SubCalendarEventData.isEnabled;
                return;
            }
            throw new Exception("Trying to Now event thats not set to Now");
        }

        internal void InitializeCompleted(SubCalendarEvent SubCalendarEventData)
        {
            if (SubCalendarEventData.isComplete)
            {
                TimeSpan SPanShift = SubCalendarEventData.Start - this.Start;
                this.shiftEvent(SPanShift, true);
                this.Complete = SubCalendarEventData.isComplete;
                return;
            }
            throw new Exception("Trying to set uncomplete event as completed, check DB_SubCalendarEventFly");
        }
    }
}