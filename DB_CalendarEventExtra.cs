using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    internal class DB_CalendarEventExtra:CalendarEvent
    {
        public DB_CalendarEventExtra(CalendarEvent CalendarEventData, Procrastination procrastinationData, NowProfile NowProfileData)
        {
            this.EventDuration = CalendarEventData.Duration;
            this.EventName = CalendarEventData.Name;
            this.StartDateTime = CalendarEventData.Start;
            this.EndDateTime = CalendarEventData.End;
            this.EventPreDeadline = CalendarEventData.PreDeadline;
            this.PrepTime = CalendarEventData.Preparation;
            this.Priority = CalendarEventData.EventPriority;
            //this.RepetitionFlag = CalendarEventData.RepetitionStatus;
            this.EventRepetition = (CalendarEventData).Repeat;// EventRepetition != CalendarEventData.null ? EventRepetition.CreateCopy() : EventRepetition;
            this.Complete = CalendarEventData.isComplete;
            this.RigidSchedule = CalendarEventData.Rigid;//hack
            this.Splits = CalendarEventData.NumberOfSplit;
            this.TimePerSplit = CalendarEventData.EachSplitTimeSpan;
            this.UniqueID = CalendarEventData.Calendar_EventID;//hack
            //this.EventSequence = CalendarEventData.EventSequence;
            this.SubEvents = new Dictionary<EventID, SubCalendarEvent>();
            this.UiParams = CalendarEventData.UIParam;
            this.DataBlob = CalendarEventData.Notes;
            this.Enabled = CalendarEventData.isEnabled;
            this.isRestricted = CalendarEventData.isEventRestricted;
            this.LocationInfo = CalendarEventData.myLocation;//hack you might need to make copy
            this.ProfileOfProcrastination = CalendarEventData.ProcrastinationInfo;
            this.DeadlineElapsed = CalendarEventData.isDeadlineElapsed;
            this.UserDeleted = CalendarEventData.isUserDeleted;
            this.CompletedCount = CalendarEventData.CompletionCount;
            this.DeletedCount = CalendarEventData.DeletionCount;
            this.ProfileOfProcrastination = procrastinationData;
            this.ProfileOfNow = NowProfileData;
            this.OriginalStart = CalendarEventData.OrginalStartInfo;
            //this.SubEvents = ((DB_CalendarEventRestricted)CalendarEventData).getSubEvents();

            if (!this.EventRepetition.Enable)
            {
                foreach (SubCalendarEvent eachSubCalendarEvent in CalendarEventData.AllSubEvents)
                {
                    this.SubEvents.Add(eachSubCalendarEvent.SubEvent_ID, eachSubCalendarEvent);
                }
            }

            //this.SubEvents = CalendarEventData.SubEvents;
            this.otherPartyID = CalendarEventData.ThirdPartyID;// == CalendarEventData.null ? null : otherPartyID.ToString();
            this.UserIDs = CalendarEventData.getAllUserIDs();//.ToList();
            //return MyCalendarEventCopy;
        }
    }
}