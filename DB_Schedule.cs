using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerCore;
using TilerElements;
using System.Data.Entity;
using System.Diagnostics;

namespace TilerFront
{
    public class DB_Schedule:Schedule
    {
        protected UserAccount myAccount;
        protected bool _CreateDump;
        protected DB_Schedule(): base()
        {

        }
        protected DB_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, UserAccount userAccount, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true) : base(allEventDictionary, starOfDay, locations, referenceNow, userAccount.getTilerUser(), rangeOfLookup)
        {
            myAccount = userAccount;
            _CreateDump = createDump;
        }


        protected DB_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, TilerUser user, DataRetrivalOption retrievalOption = DataRetrivalOption.Evaluation, TimeLine rangeOfLookup = null, bool createDump = true, HashSet<string> calendarIds = null) : base(allEventDictionary, starOfDay, locations, referenceNow, user, rangeOfLookup)
        {
            _CreateDump = createDump;
        }
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay, HashSet<DataRetrivalOption> retrievalOption = null, TimeLine rangeOfLookup = null, bool createDump = true, HashSet<string> calendarIds = null) : base()
        {
            myAccount = AccountEntry;
            if (retrievalOption == null)
            {
                retrievalOption = DataRetrievalSet.scheduleManipulationPerformance;
            }
            this.retrievalOptions = retrievalOption;
            this.RangeOfLookup = rangeOfLookup ?? new TimeLine(referenceNow.AddDays(Utility.defaultBeginDay), referenceNow.AddDays(Utility.defaultEndDay));
            _CreateDump = createDump;
            Initialize(referenceNow, startOfDay, calendarIds).Wait();
            
        }
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow, HashSet<DataRetrivalOption> retrievalOptions = null, TimeLine rangeOfLookup = null, bool createDump = true, HashSet<string> calendarIds = null)
        {
            myAccount = AccountEntry;
            if(retrievalOptions == null)
            {
                retrievalOptions = DataRetrievalSet.scheduleManipulationPerformance;
            }
            this.retrievalOptions = retrievalOptions;
            this.RangeOfLookup = rangeOfLookup?? new TimeLine(referenceNow.AddDays(Utility.defaultBeginDay), referenceNow.AddDays(Utility.defaultEndDay));
            _CreateDump = createDump;
            Initialize(referenceNow, calendarIds).Wait();
        }
        async virtual protected Task Initialize(DateTimeOffset referenceNow, HashSet<string> calendarIds = null)
        {
            DateTimeOffset StartOfDay = myAccount.ScheduleData.getDayReferenceTime();
            await Initialize(referenceNow, StartOfDay, calendarIds).ConfigureAwait(false);
        }

        async virtual protected Task Initialize(DateTimeOffset referenceNow, DateTimeOffset StartOfDay, HashSet<string> calendarIds = null)
        {
            if (!myAccount.Status)
            {
                throw new Exception("Using non verified tiler Account, try logging into account first.");
            }

            _Now = new ReferenceNow(referenceNow, StartOfDay, myAccount.getTilerUser().TimeZoneDifference);
            this.RangeOfLookup = this.RangeOfLookup ?? new TimeLine(_Now.constNow.AddDays(Schedule.TimeLookUpDayStart), _Now.constNow.AddDays(Schedule.TimeLookUpDayEnd));
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>, Analysis> profileData = await myAccount.ScheduleData.getProfileInfo(RangeOfLookup, _Now, this.retrievalOptions, calendarIds: calendarIds).ConfigureAwait(false);
            myAccount.Now = _Now;
            TravelCache travelCache = await myAccount.ScheduleData.getTravelCache(myAccount.UserID).ConfigureAwait(false);
            updateTravelCache(travelCache);
            if (profileData != null)
            {
                this.setAnalysis(profileData.Item4);
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                initializeAllEventDictionary(profileData.Item1.Values);
                if (getAllEventDictionary!= null)
                {
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();
                }
                Locations = profileData.Item3;
            }
            TilerUser = myAccount.getTilerUser();
        }


        /// <summary>
        /// Function sets all calendar events in as disabled and then deletes them from the UI
        /// </summary>
        public virtual void RemoveAllCalendarEventFromLogAndCalendar()
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
            foreach (CalendarEvent eachCalendarEvent in calEvents)
            {
                {
                    if (!eachCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                    {
                        shiftSUbEventsByTimeAndId(eachCalendarEvent.ActiveSubEvents);
                    }
                    else
                    {
                        if (eachCalendarEvent.isRepeatLoaded)
                        {
                            await CleanUpForUI(eachCalendarEvent.Repeat.RecurringCalendarEvents()).ConfigureAwait(false);
                        }
                    }
                }
            };

            foreach (DayTimeLine dayTimeLine in Now.getAllDaysForCalc())// Need to clear sub events because the daytimeline holds the subevents in the wrong day after a shift
            {
                dayTimeLine.ClearAllSubEvents();
            }

            populateDayTimeLinesWithSubcalendarEvents();
            return;
        }

        void shiftSUbEventsByTimeAndId(IEnumerable<SubCalendarEvent> subEvents)
        {
            List<DateTimeOffset> AllStartTimes = subEvents.Where(obj => !obj.isLocked && !obj.LockToId).Select(obj => obj.Start).ToList();
            AllStartTimes.Sort();
            List<SubCalendarEvent> OrderedSubEvent = subEvents.Where(obj => !obj.isLocked && !obj.LockToId).OrderBy(obj => obj.SubEvent_ID.getSubCalendarEventID()).ToList();


            Parallel.For(0, OrderedSubEvent.Count, i =>
            {
                SubCalendarEvent SubEvent = OrderedSubEvent[i];
                DateTimeOffset TIme = AllStartTimes[i];
                SubEvent.shiftEvent(TIme - SubEvent.Start, true);
            });
        }

        async virtual public Task WriteFullScheduleToLog(CalendarEvent newCalendarEvent = null)
        {
            await CleanUpForUI(getAllCalendarEvents()).ConfigureAwait(false);
            myAccount.UpdateReferenceDayTime(ReferenceDayTIime);



            foreach (List<CalendarEvent> eachTuple in ThirdPartyCalendars.Values)
            {
                foreach (CalendarEvent THirpartyCalendarEvents in eachTuple)
                {
                    removeFromAllEventDictionary(THirpartyCalendarEvents.Calendar_EventID.getCalendarEventComponent());
                }
            }

            await myAccount.Commit(getAllCalendarEvents(), newCalendarEvent, EventID.LatestID.ToString(), this.Now, this.TravelCache ?? myAccount.getTilerUser().TravelCache).ConfigureAwait(false);
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
            myOutlook.removeAllEventsFromOutLook(getAllCalendarEvents());
        }
        virtual public void WriteFullScheduleToOutlook()
        {
            TilerFront.OutLookConnector myOutlook = new OutLookConnector();
            foreach (CalendarEvent MyCalEvent in getAllActiveCalendarEvents())
            {
                myOutlook.WriteToOutlook(MyCalEvent);
            }
        }

        async virtual public Task persistToDB(bool persistNewChanges = true)
        {
            if (persistNewChanges)
            {
                await WriteFullScheduleToLog().ConfigureAwait(false);
            } else
            {
                await myAccount.DiscardChanges().ConfigureAwait(false);
            }
            
        }

        /// <summary>
        /// Function adds a new event to the schedule but is async chronous
        /// </summary>
        /// <param name="NewEvent"></param>
        /// <param name="optimizeSchedule"></param>
        /// <returns></returns>
        async virtual public Task<CustomErrors> AddToScheduleAndCommitAsync(CalendarEvent NewEvent, bool optimizeSchedule = true)
        {   
            AddToSchedule(NewEvent, optimizeSchedule);

            await WriteFullScheduleToLog(NewEvent).ConfigureAwait(false);
            CompleteSchedule = getTimeLine();
            return NewEvent.Error;
        }

        /// <summary>
        /// Function adds a new event to the schedule 
        /// </summary>
        /// <param name="NewEvent"></param>
        /// <param name="optimizeSchedule"></param>
        virtual public CustomErrors AddToScheduleAndCommit(CalendarEvent NewEvent, bool optimizeSchedule = true)
        {
            AddToSchedule(NewEvent, optimizeSchedule);

            var writeToLogTask = WriteFullScheduleToLog(NewEvent);
            writeToLogTask.Wait();

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
                tempEventName, newSubeEvent.Start, newSubeEvent.End, fiveMinSpan, new TimeSpan(), new TimeSpan(), 1, new Repetition(), new Location(), new EventDisplay(), new MiscData(), null, new NowProfile(), true, false, user, new TilerUserGroup(), user.TimeZone, null, null);
            tempEventName.Creator_EventDB = TempEvent.getCreator;
            tempEventName.AssociatedEvent = TempEvent;
            AddToSchedule(TempEvent);
            removeFromAllEventDictionary(TempEvent.Calendar_EventID.getCalendarEventComponent());
            removeFromAllEventDictionary(TempEvent.Calendar_EventID.ToString());
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
                    removeFromAllEventDictionary(ThirdPartyCalData.Calendar_EventID.getCalendarEventComponent());
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
                    tempEventName, newSubeEvent.Start, newSubeEvent.End, fiveMinSpan, new TimeSpan(), new TimeSpan(), 1, new Repetition(), new Location(), new EventDisplay(), new MiscData(), null, new NowProfile(), true, false, user, new TilerUserGroup(), user.TimeZone, null, null);
                tempEventName.Creator_EventDB = TempEvent.getCreator;
                tempEventName.AssociatedEvent = TempEvent;
                AddToSchedule(TempEvent);
                removeFromAllEventDictionary(TempEvent.Calendar_EventID.getCalendarEventComponent());
                removeFromAllEventDictionary(TempEvent.Calendar_EventID.ToString());
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

        virtual public void DisableScheduleDump()
        {
            myAccount.ScheduleLogControl.disableUpdateBigData();
        }

        virtual public void EnableScheduleDump()
        {
            myAccount.ScheduleLogControl.enableUpdateBigData();
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
            scheduleDump.StartOfDay = this.Now.EndOfDay;

            myAccount.ScheduleLogControl.Database.ScheduleDumps.Add(scheduleDump);
            await persistToDB().ConfigureAwait(false);
            return scheduleDump;
        }


        public override Tuple<CustomErrors, Dictionary<string, CalendarEvent>> BundleChangeUpdate(string EventId, EventName NewName, DateTimeOffset newStart, DateTimeOffset newEnd, int newSplitCount, string notes, bool forceRecalculation = false, SubCalendarEvent triggerSubEvent = null)
        {
            Task<TilerEvent> pendingNameAndMisc = null;
            //dynamic pendingNameAndMisc = null;
            bool useSubEvent = triggerSubEvent != null && triggerSubEvent.NameId.isNot_NullEmptyOrWhiteSpace() && triggerSubEvent.DataBlobId.isNot_NullEmptyOrWhiteSpace();
            if (useSubEvent)
            {
                if(this.myAccount.ScheduleLogControl.Database!=null)
                {
                    pendingNameAndMisc = Task<TilerEvent>.Run(async () =>
                    {
                        SubCalendarEvent subRetValue = await this.myAccount.ScheduleLogControl.Database.SubEvents
                        .Include(eachSubEvent => eachSubEvent.DataBlob_EventDB)
                        .Include(eachSubEvent => eachSubEvent.Name)
                        .FirstAsync(eachSubEvent => eachSubEvent.Id == triggerSubEvent.Id).ConfigureAwait(false);
                        TilerEvent tileRetValue = (TilerEvent)subRetValue;
                        return tileRetValue;

                    });
                }
            } 
            else
            {
                var calEent = getCalendarEvent(EventId);
                if(calEent!=null)
                {
                    if (this.myAccount.ScheduleLogControl.Database != null)
                    {
                        pendingNameAndMisc = Task<TilerEvent>.Run(async () =>
                        {
                            CalendarEvent calRetValue = await this.myAccount.ScheduleLogControl.Database.CalEvents
                            .Include(eachSubEvent => eachSubEvent.DataBlob_EventDB)
                            .Include(eachSubEvent => eachSubEvent.Name)
                            .FirstAsync(eachSubEvent => eachSubEvent.Id == calEent.Id).ConfigureAwait(false);
                            TilerEvent tileRetValue = (TilerEvent)calRetValue;
                            return tileRetValue;

                        });
                    }
                        
                }

            }

            var retValue = base.BundleChangeUpdate(EventId, NewName, newStart, newEnd, newSplitCount, notes, forceRecalculation, triggerSubEvent);


            TilerEvent tilerEvent = null;
            if (!useSubEvent)
            {
                tilerEvent = getCalendarEvent(EventId);
            }
            else
            {
                tilerEvent = triggerSubEvent;
            }

            if (pendingNameAndMisc != null)
            {
                pendingNameAndMisc.Wait();
                var pendingNameAndMiscResult = pendingNameAndMisc.Result;

                EventName oldName = pendingNameAndMiscResult.getName;
                MiscData miscData = pendingNameAndMiscResult.Notes;
                bool isNameChange = NewName.NameValue != oldName?.NameValue;
                
                if (isNameChange)
                {
                    tilerEvent.updateEventName(NewName.NameValue);
                }


                
                var note = miscData;
                if (note != null)
                {
                    note.UserNote = notes;
                }
            }

            return retValue;
        }

    }
}
