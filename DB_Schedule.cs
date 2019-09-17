﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerCore;
using TilerElements;

namespace TilerFront
{
    public class DB_Schedule:Schedule
    {
        protected UserAccount myAccount;
        protected bool _CreateDump;
        protected DB_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, UserAccount userAccount, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true) : base(allEventDictionary, starOfDay, locations, referenceNow, userAccount.getTilerUser(), rangeOfLookup)
        {
            myAccount = userAccount;
            _CreateDump = createDump;
        }


        protected DB_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, TilerUser user, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true) : base(allEventDictionary, starOfDay, locations, referenceNow, user, rangeOfLookup)
        {
            _CreateDump = createDump;
        }
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true) : base()
        {
            myAccount = AccountEntry;
            this.retrievalOption = retrievalOption;
            this.RangeOfLookup = rangeOfLookup;
            _CreateDump = createDump;
            Initialize(referenceNow, startOfDay).Wait();
            
        }
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true)
        {
            myAccount = AccountEntry;
            this.retrievalOption = retrievalOption;
            this.RangeOfLookup = rangeOfLookup;
            _CreateDump = createDump;
            Initialize(referenceNow).Wait();
        }
        async virtual protected Task Initialize(DateTimeOffset referenceNow)
        {
            DateTimeOffset StartOfDay = myAccount.ScheduleData.getDayReferenceTime();
            _Now = new ReferenceNow(referenceNow, StartOfDay, myAccount.getTilerUser().TimeZoneDifference);
            this.RangeOfLookup = this.RangeOfLookup ?? new TimeLine(_Now.constNow.AddDays(Schedule.TimeLookUpDayStart), _Now.constNow.AddDays(Schedule.TimeLookUpDayEnd));
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo(RangeOfLookup, _Now, retrievalOption, _CreateDump).ConfigureAwait(false);
            myAccount.Now = _Now;
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();

                    //EventIDGenerator.Initialize((uint)(this.LastScheduleIDNumber));
                }
                Locations = profileData.Item3;
            }
            TilerUser = myAccount.getTilerUser();
        }

        async virtual protected Task Initialize(DateTimeOffset referenceNow, DateTimeOffset StartOfDay)
        {
            if (!myAccount.Status)
            {
                throw new Exception("Using non verified tiler Account, try logging into account first.");
            }

            _Now = new ReferenceNow(referenceNow, StartOfDay, myAccount.getTilerUser().TimeZoneDifference);
            TimeLine RangeOfLookup = new TimeLine(_Now.constNow.AddYears(-10), _Now.constNow.AddYears(10));
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo(RangeOfLookup, _Now, this.retrievalOption).ConfigureAwait(false);
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();
                }
                Locations = profileData.Item3;
            }
        }

        public virtual void RemoveAllCalendarEventFromLogAndCalendar()//MyTemp Function for deleting all calendar events
        {
            myAccount.DeleteAllCalendarEvents();
            removeAllFromOutlook();
        }


        /// <summary>
        /// Function rearranges the sub events by Id to be associated with the order of time
        /// </summary>
        /// <returns></returns>
        async Task CleanUpForUI(IEnumerable<CalendarEvent> calEvents)
        {
            calEvents.AsParallel()
                .ForAll(async (eachCalendarEvent) =>
                {
                    if(!eachCalendarEvent.IsRepeat)
                    {
                        shiftSUbEventsByTimeAndId(eachCalendarEvent.ActiveSubEvents);
                    } else
                    {
                        await CleanUpForUI(eachCalendarEvent.Repeat.RecurringCalendarEvents()).ConfigureAwait(false);
                    }
                }
            );
            return;
        }

        void shiftSUbEventsByTimeAndId(IEnumerable<SubCalendarEvent> subEvents)
        {
            List<DateTimeOffset> AllStartTimes = subEvents.AsParallel().Where(obj => !obj.isLocked && !obj.LockToId).Select(obj => obj.Start).ToList();
            AllStartTimes.Sort();
            List<SubCalendarEvent> OrderedSubEvent = subEvents.Where(obj => !obj.isLocked && !obj.LockToId).OrderBy(obj => obj.SubEvent_ID.getSubCalendarEventID()).ToList();


            Parallel.For(0, OrderedSubEvent.Count, i =>
            {
                SubCalendarEvent SubEvent = OrderedSubEvent[i];
                DateTimeOffset TIme = AllStartTimes[i];
                SubEvent.shiftEvent(TIme - SubEvent.Start);
            });
        }

        async virtual public Task WriteFullScheduleToLog(CalendarEvent newCalendarEvent = null)
        {
            await CleanUpForUI(AllEventDictionary.Values).ConfigureAwait(false);
            myAccount.UpdateReferenceDayTime(ReferenceDayTIime);



            foreach (List<CalendarEvent> eachTuple in ThirdPartyCalendars.Values)
            {
                foreach (CalendarEvent THirpartyCalendarEvents in eachTuple)
                {
                    AllEventDictionary.Remove(THirpartyCalendarEvents.Calendar_EventID.getCalendarEventComponent());
                }
            }

            await myAccount.Commit(AllEventDictionary.Values, newCalendarEvent, EventID.LatestID.ToString(), this.Now).ConfigureAwait(false);
        }

        virtual public bool isScheduleLoadSuccessful
        {
            get
            {
                return myAccount.Status;
            }
        }

        virtual public void removeAllFromOutlook()
        {

            TilerFront.OutLookConnector myOutlook = new OutLookConnector();
            myOutlook.removeAllEventsFromOutLook(AllEventDictionary.Values);
        }
        virtual public void WriteFullScheduleToOutlook()
        {
            TilerFront.OutLookConnector myOutlook = new OutLookConnector();
            foreach (CalendarEvent MyCalEvent in AllEventDictionary.Values)
            {
                myOutlook.WriteToOutlook(MyCalEvent);
            }
        }

        async virtual public Task persistToDB(bool persistNewChanges = true)
        {
            //RemoveAllCalendarEventFromLogAndCalendar();
            if(persistNewChanges)
            {
                removeAllFromOutlook();
                await WriteFullScheduleToLog().ConfigureAwait(false);
                CompleteSchedule = getTimeLine();
            } else
            {
                await myAccount.DiscardChanges().ConfigureAwait(false);
            }
            
        }

        async virtual public Task<CustomErrors> AddToScheduleAndCommit(CalendarEvent NewEvent, bool optimizeSchedule = true)
        {
            AddToSchedule(NewEvent, optimizeSchedule);

            await WriteFullScheduleToLog(NewEvent).ConfigureAwait(false);

            CompleteSchedule = getTimeLine();


            
            return NewEvent.Error;
        }

        public virtual async Task UpdateScheduleDueToExternalChanges()
        {
            TimeLine newSubeEvent = new TimeLine(Now.constNow, Now.constNow.AddMinutes(5));
            TimeSpan fiveMinSpan = new TimeSpan(1);
            EventName tempEventName = new EventName(null, null, "TempEvent");
            TilerUser user = this.TilerUser;
            CalendarEvent TempEvent = new CalendarEvent(
                //EventID.GenerateCalendarEvent(), 
                tempEventName, newSubeEvent.Start, newSubeEvent.End, fiveMinSpan, new TimeSpan(), new TimeSpan(), 1, new Repetition(), new Location(), new EventDisplay(), new MiscData(), null, new NowProfile(), true, false, user, new TilerUserGroup(), user.TimeZone, null);
            tempEventName.Creator_EventDB = TempEvent.getCreator;
            tempEventName.AssociatedEvent = TempEvent;
            AddToSchedule(TempEvent);
            AllEventDictionary.Remove(TempEvent.Calendar_EventID.getCalendarEventComponent());
            AllEventDictionary.Remove(TempEvent.Calendar_EventID.ToString());
            await WriteFullScheduleToLog().ConfigureAwait(false);
            return;
        }

        async virtual public Task updateDataSetWithThirdPartyDataAndTriggerNewAddition(Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>> ThirdPartyData)
        {

            if (ThirdPartyData != null)
            {
                updateDataSetWithThirdPartyData(ThirdPartyData);
                await triggerNewlyAddedThirdparty().ConfigureAwait(false);
                foreach (CalendarEvent ThirdPartyCalData in ThirdPartyData.Item2)
                {
                    AllEventDictionary.Remove(ThirdPartyCalData.Calendar_EventID.getCalendarEventComponent());
                }
            }
            retrievedThirdParty = true;
            await WriteFullScheduleToLog().ConfigureAwait(false);
        }


        virtual protected async Task triggerNewlyAddedThirdparty()
        {
            if (retrievedThirdParty)
            {
                TimeLine newSubeEvent = new TimeLine(Now.constNow, Now.constNow.AddMinutes(5));
                TimeSpan fiveMinSpan = new TimeSpan(1);
                EventName tempEventName = new EventName(null, null, "TempEvent");
                TilerUser user = TilerUser;
                CalendarEvent TempEvent = new CalendarEvent(
                    tempEventName, newSubeEvent.Start, newSubeEvent.End, fiveMinSpan, new TimeSpan(), new TimeSpan(), 1, new Repetition(), new Location(), new EventDisplay(), new MiscData(), null, new NowProfile(), true, false, user, new TilerUserGroup(), user.TimeZone, null);
                tempEventName.Creator_EventDB = TempEvent.getCreator;
                tempEventName.AssociatedEvent = TempEvent;
                AddToSchedule(TempEvent);
                AllEventDictionary.Remove(TempEvent.Calendar_EventID.getCalendarEventComponent());
                AllEventDictionary.Remove(TempEvent.Calendar_EventID.ToString());
                return;
            }

            throw new Exception("Hey you are yet to retrieve the latest third party schedule");
        }

        /// <summary>
        /// Function creates a schedule dump that is equivalent to schedule from RDBMS
        /// </summary>
        /// <returns></returns>
        virtual async public Task<ScheduleDump> CreateScheduleDump(ReferenceNow referenceNow = null, string notes="")
        {
            referenceNow = referenceNow ?? this.Now;
            return await myAccount.ScheduleLogControl.CreateScheduleDump(this.getAllCalendarEvents(), myAccount.getTilerUser(), referenceNow, notes).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a schedule dump and then persists it to the DB
        /// </summary>
        /// <returns></returns>
        virtual async public Task<ScheduleDump> CreateAndPersistScheduleDump(ScheduleDump scheduleDump = null, ReferenceNow now = null)
        {
            ReferenceNow refNow = now ?? this.Now;
            scheduleDump = scheduleDump ?? await this.CreateScheduleDump(refNow).ConfigureAwait(false);
            scheduleDump.ReferenceNow = this.Now.constNow;
            scheduleDump.StartOfDay = this.Now.StartOfDay;

            myAccount.ScheduleLogControl.Database.ScheduleDumps.Add(scheduleDump);
            await persistToDB().ConfigureAwait(false);
            return scheduleDump;
        }

    }
}
