//#define loadFromXml

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Web;
using TilerFront.Models;
using TilerElements;
using DBTilerElement;
//using TilerGoogleCalendarLib;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Web.Http.Cors;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using TilerCore;
using Location = TilerElements.Location;
using System.Diagnostics;
using ScheduleAnalysis;

namespace TilerFront.Controllers
{
    [Authorize]
    /// <summary>
    /// Represents a users schedule. Provides access to schedule creation, modification and deletion
    /// </summary>
    public class ScheduleController : TilerApiController
    {

        // GET api/schedule
        /// <summary>
        /// Retrieve Events within a time frame. Required elements are UserID and UserName. Provided starttime and Endtime for the range of the schedule allows for retrieval of schedule within a timerange
        /// </summary>
        /// <param name="myAuthorizedUser"></param>
        /// <returns></returns>
        //[ValidateAntiForgeryTokenAttribute]
        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> GetSchedule([FromUri] getScheduleModel myAuthorizedUser)
        {
            DateTimeOffset StartTime = Utility.JSStartTime.AddMilliseconds(myAuthorizedUser.StartRange);
            DateTimeOffset EndTime = Utility.JSStartTime.AddMilliseconds(myAuthorizedUser.EndRange);
            TimeLine timeLine = new TimeLine(StartTime, EndTime);
            PostBackData returnPostBack = await getDataFromRestEnd(myAuthorizedUser, timeLine);
            return Ok(returnPostBack.getPostBack);
        }


        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/getScheduleAlexa")]
        public async Task<IHttpActionResult> GetScheduleAlexa(getScheduleModel myAuthorizedUser)
        {
            DateTimeOffset StartTime = Utility.JSStartTime.AddMilliseconds(myAuthorizedUser.StartRange);
            DateTimeOffset EndTime = Utility.JSStartTime.AddMilliseconds(myAuthorizedUser.EndRange);
            TimeLine timeLine = new TimeLine(StartTime, EndTime);
            PostBackData returnPostBack = await getDataFromRestEnd(myAuthorizedUser, timeLine);
            return Ok(returnPostBack.getPostBack);
        }


        async Task<PostBackData> getDataFromRestEnd(getScheduleModel myAuthorizedUser, TimeLine TimelineForData = null)
        {
            UserAccount myUserAccount = await myAuthorizedUser.getUserAccount(db);
            HttpContext myCOntext = HttpContext.Current;
            await myUserAccount.Login();
            myUserAccount.getTilerUser().updateTimeZoneTimeSpan(myAuthorizedUser.getTimeSpan);
            PostBackData returnPostBack;
            TilerUser tilerUser = myUserAccount.getTilerUser();
            if (myUserAccount.Status)
            {
                DateTimeOffset nullStart = myAuthorizedUser.getRefNow().AddDays(Utility.defaultBeginDay);
                DateTimeOffset nullEnd = myAuthorizedUser.getRefNow().AddDays(Utility.defaultEndDay);
                TimelineForData = TimelineForData ?? new TimeLine(nullStart, nullEnd);


                LogControl LogAccess = myUserAccount.ScheduleLogControl;
                List<IndexedThirdPartyAuthentication> AllIndexedThirdParty = await getAllThirdPartyAuthentication(myUserAccount.UserID, db).ConfigureAwait(false);

                List<GoogleTilerEventControl> AllGoogleTilerEvents = AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl(obj, db)).ToList();
                foreach (IndexedThirdPartyAuthentication obj in AllIndexedThirdParty)
                {
                    var GoogleTilerEventControlobj = new GoogleTilerEventControl(obj, db);
                }

                Task<ConcurrentBag<CalendarEvent>> GoogleCalEventsTask = GoogleTilerEventControl.getAllCalEvents(AllGoogleTilerEvents, TimelineForData);
                ReferenceNow now = new ReferenceNow(myAuthorizedUser.getRefNow(), tilerUser.EndfOfDay, tilerUser.TimeZoneDifference);

                IEnumerable<SubCalendarEvent> subEvents = await LogAccess.getAllEnabledSubCalendarEvent(TimelineForData, now, retrievalOptions: DataRetrievalSet.UiSet).ConfigureAwait(false);
                IEnumerable<CalendarEvent> GoogleCalEvents = await GoogleCalEventsTask.ConfigureAwait(false);
                subEvents = subEvents.Concat(GoogleCalEvents.SelectMany(subEvent => subEvent.AllSubEvents)).Where(subEvent => subEvent.StartToEnd.doesTimeLineInterfere(TimelineForData));

#if loadFromXml
                if (!string.IsNullOrEmpty(xmlFileId) && !string.IsNullOrWhiteSpace(xmlFileId))
                {
                    var tempSched = TilerTests.TestUtility.getSchedule(xmlFileId, connectionName: "DefaultConnection", filePath: LogControl.getLogLocation());
                    var MySchedule = (DB_Schedule)tempSched.Item1;
                    subEvents = MySchedule.getAllCalendarEvents().SelectMany(obj => obj.ActiveSubEvents);
                }
#endif
                DayTimeLine sleepTimeline = now.getDayTimeLineByTime(now.constNow.AddDays(2));
                TimeLine sleepTImeline = TimeOfDayPreferrence.splitIntoDaySections(sleepTimeline)[TimeOfDayPreferrence.DaySection.Sleep];

                HashSet<CalendarEvent> calEVents = new HashSet<CalendarEvent>();
                var subCalEvents = subEvents.Select(subEvent =>

                    {
                        calEVents.Add(subEvent.ParentCalendarEvent);
                        return subEvent.ToSubCalEvent(subEvent.ParentCalendarEvent);
                    }
                    ).Concat(calEVents.SelectMany(eachCalEVent => eachCalEVent.PausedTimeLines.Where(pausedTimeline => eachCalEVent.getSubEvent(pausedTimeline.getSubEventId())!=null).Select(eachPausedTimeLine => eachPausedTimeLine.ToSubCalEvent(eachCalEVent)))).ToList() ;
                UserSchedule currUserSchedule = new UserSchedule {
                    //NonRepeatCalendarEvent = NonRepeatingEvents.Select(obj => obj.ToCalEvent(TimelineForData)).ToArray(),
                    //RepeatCalendarEvent = RepeatingEvents,
                    SubCalendarEvents = subCalEvents,
                    SleepTimeline = sleepTImeline.ToJson()
                };
                PausedEvent currentPausedEvent = getCurrentPausedEvent(db);
                currUserSchedule.populatePauseData(currentPausedEvent, myAuthorizedUser.getRefNow());
                InitScheduleProfile retValue = new InitScheduleProfile { Schedule = currUserSchedule, Name = myUserAccount.Usersname };
                returnPostBack = new PostBackData(retValue, 0);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return returnPostBack;
        }


        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/FixDBInstance")]
        public async Task<IHttpActionResult> FixRepetition([FromUri] getScheduleModel myAuthorizedUser)
        {
            string cloudId = "6bc6992f-3222-4fd8-9e2b-b94eba2fb717";
            //string localId = "4751e09f-b592-4e45-9fba-3425ff95b1da";
            string userId = cloudId;
            string userName = "jerbio";
            myAuthorizedUser = new getScheduleModel()
            {
                UserName = userName,
                UserID = userId
            };
            UserAccount myUserAccount = await myAuthorizedUser.getUserAccount(db);
            HttpContext myCOntext = HttpContext.Current;
            await myUserAccount.Login();
            myUserAccount.getTilerUser().updateTimeZoneTimeSpan(myAuthorizedUser.getTimeSpan);

            ReferenceNow now = new ReferenceNow(DateTimeOffset.UtcNow.removeSecondsAndMilliseconds(), myUserAccount.getTilerUser().EndfOfDay, new TimeSpan());
            TimeLine timeLine = new TimeLine(Utility.BeginningOfTime, Utility.BeginningOfTime.AddYears(9000));
            LogControl logControl = myUserAccount.ScheduleLogControl;
            var calEvents = await logControl.getAllEnabledCalendarEvent(timeLine, now, retrievalOptions: DataRetrievalSet.All).ConfigureAwait(false);

            foreach (var cal in calEvents.Values)
            {
                string nowProfileId = cal.NowProfileId;
                if(string.IsNullOrEmpty(nowProfileId))
                {
                    var recurringcals = cal.RecurringCalendarEvents;
                    if(recurringcals!=null)
                    {
                        var notNullProfile = recurringcals.Where(o => !string.IsNullOrEmpty(o.NowProfileId)).FirstOrDefault();
                        if(notNullProfile!=null)
                        {
                            nowProfileId = notNullProfile.NowProfileId;
                        }
                    }
                }

                if (string.IsNullOrEmpty(nowProfileId))
                {
                    var nowProfile = new NowProfile();
                    db.NowProfiles.Add(nowProfile);
                    cal.NowProfileId = nowProfile.Id;
                    nowProfileId = nowProfile.Id;
                }

                if (cal.IsRecurring)
                {
                    foreach(var iterCal in cal.RecurringCalendarEvents)
                    {
                        iterCal.NowProfileId = null;
                        iterCal.ProfileOfNow_EventDB = null;
                        iterCal.NowProfileId = nowProfileId;
                        foreach (var subevent in iterCal.AllSubEvents)
                        {
                            subevent.NowProfileId = null;
                            subevent.ProfileOfNow_EventDB = null;
                            subevent.NowProfileId = nowProfileId;
                        }
                    }
                }
                else
                {
                    foreach (var subevent in cal.AllSubEvents)
                    {
                        subevent.NowProfileId = null;
                        subevent.ProfileOfNow_EventDB = null;
                        subevent.NowProfileId = nowProfileId;
                    }
                }

            }
            bool saveChanges = false;
            if (saveChanges)
            {
                TilerUser tilerUser = myUserAccount.getTilerUser();
                await myUserAccount.Commit(new List<CalendarEvent>(), null, tilerUser.LatestId, now, tilerUser.TravelCache).ConfigureAwait(false);
            }




            PostBackData returnPostBack;
            returnPostBack = new PostBackData("", 0);

            return Ok(returnPostBack.getPostBack);

        }



        // GET api/schedule
        /// <summary>
        /// Retrieve Events within a time frame. Required elements are UserID and UserName. Provided starttime and Endtime for the range of the schedule allows for retrieval of schedule within a timerange
        /// </summary>
        /// <param name="myAuthorizedUser"></param>
        /// <returns></returns>
        /// 

        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/DeletedSchedule")]
        public async Task<IHttpActionResult> GetDeletedSchedule([FromUri] getScheduleModel myAuthorizedUser)
        {
            UserAccount myUserAccount = await myAuthorizedUser.getUserAccount(db);
            HttpContext myCOntext = HttpContext.Current;
            await myUserAccount.Login();
            myUserAccount.getTilerUser().updateTimeZoneTimeSpan(myAuthorizedUser.getTimeSpan);
            PostBackData returnPostBack;
            if (myUserAccount.Status)
            {
                TilerUser tilerUser = myUserAccount.getTilerUser();
                ReferenceNow now = new ReferenceNow(myAuthorizedUser.getRefNow(), tilerUser.EndfOfDay, tilerUser.TimeZoneDifference);
                long StartTimeMs = myAuthorizedUser.StartRange;// new DateTimeOffset(myAuthorizedUser.StartRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTimeSpan).ToUnixTimeMilliseconds();
                long EndTimeMs = myAuthorizedUser.EndRange;// new DateTimeOffset(myAuthorizedUser.EndRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTimeSpan).ToUnixTimeMilliseconds();
                TimeLine TimelineForData = new TimeLine(Utility.BeginningOfTime.AddDays(1), now.constNow.AddDays(Utility.defaultEndDay));


                LogControl LogAccess = myUserAccount.ScheduleLogControl;
                List<CalendarEvent> ScheduleData = new List<CalendarEvent>();
                
                
                List<SubCalendarEvent> subEvents = await LogAccess.getAllSubCalendarEvents(TimelineForData, now, DataRetrievalSet.All)
                    .Include(subEvent => subEvent.Name)
                    .Where(subEvent => 
                    (StartTimeMs <= subEvent.DeletionTime_DB && subEvent.DeletionTime_DB <= EndTimeMs)
                    || (StartTimeMs <= subEvent.CompletionTime_EventDB && subEvent.CompletionTime_EventDB <= EndTimeMs)
                    || (StartTimeMs <= subEvent.ParentCalendarEvent.DeletionTime_DB && subEvent.ParentCalendarEvent.DeletionTime_DB <= EndTimeMs)
                    || (StartTimeMs <= subEvent.ParentCalendarEvent.CompletionTime_EventDB && subEvent.ParentCalendarEvent.CompletionTime_EventDB <= EndTimeMs)
                    ).ToListAsync().ConfigureAwait(false);


                DayTimeLine sleepTimeline = now.getDayTimeLineByTime(now.constNow.AddDays(2));
                TimeLine sleepTImeline = TimeOfDayPreferrence.splitIntoDaySections(sleepTimeline)[TimeOfDayPreferrence.DaySection.Sleep];

                UserSchedule currUserSchedule = new UserSchedule
                {
                    //NonRepeatCalendarEvent = NonRepeatingEvents.Select(obj => obj.ToCalEvent(TimelineForData)).ToArray(),
                    //RepeatCalendarEvent = RepeatingEvents,
                    SubCalendarEvents = subEvents.Select(subEvent =>
                        subEvent.ToSubCalEvent(subEvent.ParentCalendarEvent)
                    ).ToList(),
                    SleepTimeline = sleepTImeline.ToJson()
                };
                PausedEvent currentPausedEvent = getCurrentPausedEvent(db);
                currUserSchedule.populatePauseData(currentPausedEvent, myAuthorizedUser.getRefNow());
                InitScheduleProfile retValue = new InitScheduleProfile { Schedule = currUserSchedule, Name = myUserAccount.Usersname };
                returnPostBack = new PostBackData(retValue, 0);

                //Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>, Analysis> ProfileData = await LogAccess.getProfileInfo(TimelineForData, null, true);

                //ScheduleData = ScheduleData.Concat(ProfileData.Item1.Values).ToList();

                //IEnumerable<CalendarEvent> NonRepeatingEvents = ScheduleData.Where(obj => !obj.IsFromRecurringAndNotChildRepeatCalEvent);




                ////IEnumerable<CalendarEvent> RepeatingEvents = ScheduleData.Where(obj => obj.RepetitionStatus).SelectMany(obj => obj.Repeat.RecurringCalendarEvents);
                //IList<UserSchedule.repeatedEventData> RepeatingEvents = ScheduleData.AsParallel().Where(obj => obj.IsFromRecurringAndNotChildRepeatCalEvent).
                //    Select(obj => new UserSchedule.repeatedEventData
                //    {
                //        ID = obj.Calendar_EventID.ToString(),
                //        Latitude = obj.Location.Latitude,
                //        Longitude = obj.Location.Longitude,
                //        RepeatAddress = obj.Location.Address,
                //        RepeatAddressDescription = obj.Location.Description,
                //        RepeatCalendarName = obj.getName.NameValue,
                //        RepeatCalendarEvents = obj.Repeat.RecurringCalendarEvents().AsParallel().
                //            Select(obj1 => obj1.ToDeletedCalEvent(TimelineForData)).ToList(),
                //        RepeatEndDate = obj.End,
                //        RepeatStartDate = obj.Start,
                //        RepeatTotalDuration = obj.getActiveDuration
                //    }).ToList();


                //UserSchedule currUserSchedule = new UserSchedule { NonRepeatCalendarEvent = NonRepeatingEvents.Select(obj => obj.ToDeletedCalEvent(TimelineForData)).ToArray(), RepeatCalendarEvent = RepeatingEvents };
                //InitScheduleProfile retValue = new InitScheduleProfile { Schedule = currUserSchedule, Name = myUserAccount.Usersname };
                //returnPostBack = new PostBackData(retValue, 0);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }

        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Pause")]
        public async Task<IHttpActionResult> PauseSchedule([FromBody]getEventModel myAuthorizedUser)
        {
            UserAccount myUser = await myAuthorizedUser.getUserAccount(db);
            await myUser.Login();
            myUser.getTilerUser().updateTimeZoneTimeSpan(myAuthorizedUser.getTimeSpan);
            if (myUser.Status)
            {
                List<PausedEvent> pausedEvents = getCurrentPausedEventAndPausedEventWithId(db, myAuthorizedUser.EventID, myUser.UserID);
                PausedEvent currentPausedEvent = pausedEvents.FirstOrDefault(obj => !obj.isPauseDeleted);

                string currentPausedEventId = "";
                if (currentPausedEvent == null)
                {
                    currentPausedEvent = new PausedEvent();
                }
                else
                {
                    currentPausedEventId = currentPausedEvent.EventId;
                    currentPausedEvent.isPauseDeleted = true;
                    db.Entry(currentPausedEvent).State = EntityState.Modified;
                }


                DateTimeOffset myNow = myAuthorizedUser.getRefNow();
                DB_Schedule MySchedule = new DB_Schedule(myUser, myNow);
                MySchedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                SubCalendarEvent SubEvent = MySchedule.getSubCalendarEvent(myAuthorizedUser.EventID);
                if ((!SubEvent.isRigid) && (SubEvent.getId != currentPausedEvent.EventId))
                {
                    DB_UserActivity activity = new DB_UserActivity(myAuthorizedUser.getRefNow(), UserActivity.ActivityType.Pause);

                    JObject json = JObject.FromObject(myAuthorizedUser);
                    activity.updateMiscelaneousInfo(json.ToString());

                    myUser.ScheduleLogControl.updateUserActivty(activity);
                    await MySchedule.PauseEvent(myAuthorizedUser.EventID);
                    await MySchedule.WriteFullScheduleToLog().ConfigureAwait(false);
                    PausedEvent paused;
                    PausedEvent InstanceOfPausedEventAlreadyInDb = pausedEvents.FirstOrDefault(obj => obj.EventId == myAuthorizedUser.EventID);

                    if (InstanceOfPausedEventAlreadyInDb == null)
                    {
                        paused = new PausedEvent() { };
                        paused.EventId = myAuthorizedUser.EventID;
                        paused.isPauseDeleted = false;
                        paused.User = db.Users.Find(myAuthorizedUser.UserID);
                        paused.PauseTime = myNow;
                        db.PausedEvents.Add(paused);
                    }
                    else
                    {
                        InstanceOfPausedEventAlreadyInDb.PauseTime = myNow;
                        InstanceOfPausedEventAlreadyInDb.isPauseDeleted = false;
                        db.Entry(InstanceOfPausedEventAlreadyInDb).State = EntityState.Modified;
                    }
                    await myUser.ScheduleLogControl.SaveDBChanges().ConfigureAwait(false);
                }
                PostBackData myPostData = new PostBackData("\"Success\"", 0);
                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(myUser.getTilerUser());
                return Ok(myPostData.getPostBack);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler user"
            });

        }

        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Resume")]
        public async Task<IHttpActionResult> ResumeSchedule([FromBody]getEventModel myAuthorizedUser)
        {
            UserAccount myUser = await myAuthorizedUser.getUserAccount(db);
            await myUser.Login();
            myUser.getTilerUser().updateTimeZoneTimeSpan(myAuthorizedUser.getTimeSpan);
            if (myUser.Status)
            {
                PausedEvent PausedEvent = getCurrentPausedEvent(db, myUser.UserID);
                SubCalendarEvent pausedSubEvent = null;
                CalendarEvent pausedCalEvent = null;
                if (PausedEvent != null)
                {
                    DateTimeOffset myNow = myAuthorizedUser.getRefNow();
                    
                    DB_Schedule MySchedule = new DB_Schedule(myUser, myNow);
                    MySchedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                    DB_UserActivity activity = new DB_UserActivity(myAuthorizedUser.getRefNow(), UserActivity.ActivityType.Resume);

                    JObject json = JObject.FromObject(myAuthorizedUser);
                    activity.updateMiscelaneousInfo(json.ToString());
                    myUser.ScheduleLogControl.updateUserActivty(activity);
                    await MySchedule.ContinueEvent(PausedEvent.EventId);
                    await MySchedule.WriteFullScheduleToLog().ConfigureAwait(false);
                    pausedSubEvent = MySchedule.getSubCalendarEvent(PausedEvent.EventId);
                    pausedCalEvent = MySchedule.getCalendarEvent(PausedEvent.EventId);
                    PausedEvent.isPauseDeleted = true;
                    db.Entry(PausedEvent).State = EntityState.Modified;
                    await myUser.ScheduleLogControl.SaveDBChanges().ConfigureAwait(false);

                    TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                    scheduleChangeSocket.triggerRefreshData(myUser.getTilerUser());
                }

                PostBackData myPostData;
                if (pausedSubEvent != null)
                {
                    myPostData = new PostBackData(pausedSubEvent.ToSubCalEvent(pausedCalEvent), 0);
                }
                else
                {
                    myPostData = new PostBackData("\"No paused Event\"", 50005000);
                }

                return Ok(myPostData.getPostBack);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler user"
            });
        }
        /// <summary>
        /// This contains the functionality for retrieveing the paused event from the db.
        /// THis is supposed to be part of logcontrol.cs. This should be done after the move to an rdbms like storage
        /// TODO: move to logcontrol.cs after switching to an rdbms db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        PausedEvent getCurrentPausedEvent(ApplicationDbContext db, string userId = null)
        {
            if(string.IsNullOrEmpty(userId))
            {
                userId = User.Identity.GetUserId();
            }
            PausedEvent RetValue = db.PausedEvents.SingleOrDefault(obj => obj.UserId == userId && obj.isPauseDeleted == false);
            return RetValue;
        }
        /// <summary>
        /// Function gets you the paused event and its the paused events with the ID EventId
        /// THis is supposed to be part of logcontrol.cs. This should be done after the move to an rdbms like storage
        /// TODO: move to logcontrol.cs after switching to an rdbms db
        /// </summary>
        /// <param name="db"></param>
        /// <param name="EventId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<PausedEvent> getCurrentPausedEventAndPausedEventWithId(ApplicationDbContext db, string EventId, string userId = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Identity.GetUserId();
            }
            List<PausedEvent> retValue = db.PausedEvents.Where(obj => ((obj.UserId == userId) && ((obj.isPauseDeleted == false) || (obj.EventId == EventId)))).ToList();
            return retValue;
        }


        /// <summary>
        /// Retrieves the third party authentication credentials needed to retrieve third party calendar. Attaches an index for multiple calendar retrieval. Based on a specific userID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        static async public Task<List<IndexedThirdPartyAuthentication>> getAllThirdPartyAuthentication(string ID, ApplicationDbContext db)
        {
            //ApplicationDbContext db = new ApplicationDbContext();
            List<ThirdPartyCalendarAuthenticationModel> AllAuthentications = ( db.ThirdPartyAuthentication.Where(obj => ID == obj.TilerID).ToList());
            List<IndexedThirdPartyAuthentication> RetValue = AllAuthentications.Select((obj, i) => new IndexedThirdPartyAuthentication(obj,(uint) i)).ToList();
            return RetValue;
        }


        /// <summary>
        /// Retrieves the third party authentication credentials needed to retrieve third party calendar. Gets a specific calendar authentication. You need full credentials to get specific oauth credentials
        /// </summary>
        /// <param name="TilerUserID"></param>
        /// <param name="ThirdpaartyUserID"></param>
        /// <param name="ThirdPartyType"></param>
        /// <returns></returns>
        async static public Task<ThirdPartyCalendarAuthenticationModel> getThirdPartyAuthentication(string TilerUserID, string ThirdpaartyUserID, string ThirdPartyType, ApplicationDbContext db)
        {
            Object[] Param = { TilerUserID, ThirdpaartyUserID, ThirdPartyType };
            ThirdPartyCalendarAuthenticationModel RetValue = await db.ThirdPartyAuthentication.FindAsync(Param);
            return RetValue;
        }

        /// <summary>
        /// Handles the trigger when new google notification comes through.
        /// </summary>
        /// <param name="GoogleNotificationID"></param>
        /// <returns></returns>
        static async public Task googleNotificationTrigger(string GoogleNotificationID, ApplicationDbContext db)
        {
            ThirdPartyCalendarAuthenticationModel ThirdPartAuthData= db.ThirdPartyAuthentication.Where(obj => obj.ID == GoogleNotificationID).Single();
            object[] LookUpParams = {ThirdPartAuthData.TilerID};
            TilerUser myUser = db.Users.Find(LookUpParams);
            await notificationTrigger(myUser, db).ConfigureAwait(false);
        }


        static async Task notificationTrigger(TilerUser TilerUser, ApplicationDbContext db)
        {
            UserAccountDirect RetrievedUSer = new UserAccountDirect(TilerUser.Id, db);
            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;//.AddDays(-1);
            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = updatemyScheduleWithGoogleThirdpartyCalendar(RetrievedUSer.UserID, db);
            DB_Schedule TilerSchedule = new DB_Schedule(RetrievedUSer, CurrentTime);
            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
            TilerSchedule.updateDataSetWithThirdPartyData(thirdPartyData);
            await TilerSchedule.UpdateScheduleDueToExternalChanges().ConfigureAwait(false);
        }


        // GET api/schedule/5
        [NonAction]
        public async Task<IHttpActionResult> GetScheduleById([FromBody]AuthorizedUser myAuthorizedUser)
        {
            return Ok("return");
        }


        /// <summary>
        /// Function retrives the third party credential. Credential are used to retrieve calendar event. The retrieved events are used to populate mySchedule
        /// </summary>
        /// <param name="TilerUserID"></param>
        /// <returns></returns>
        static internal async Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> updatemyScheduleWithGoogleThirdpartyCalendar(string TilerUserID, ApplicationDbContext db, TimeLine calcultionTimeLine = null, bool getGoogleLocation = true)
        {
            List<IndexedThirdPartyAuthentication> AllIndexedThirdParty = await getAllThirdPartyAuthentication(TilerUserID, db).ConfigureAwait(false);
            List<GoogleTilerEventControl> AllGoogleTilerEvents = AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl (obj, db)).ToList();

            Tuple<List<GoogleTilerEventControl>, GoogleThirdPartyControl> GoogleEvents = await GoogleTilerEventControl.getThirdPartyControlForIndex(AllGoogleTilerEvents, calcultionTimeLine, getGoogleLocation).ConfigureAwait(false);
            Task DeleteInvalidAuthentication = ManageController.delelteGoogleAuthentication(GoogleEvents.Item1.Select(obj => obj.getDBAuthenticationData()));

            var retValue = new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent> { GoogleEvents.Item2.getThirdpartyCalendarEvent() });

            //mySchedule.updateDataSetWithThirdPartyData();
            //mySchedule.updateDataSetWithThirdPartyData(new Tuple<ThirdPartyControl.CalendarTool.Google, GoogleEvents.Item2.);
            await DeleteInvalidAuthentication.ConfigureAwait(false);
            return retValue;
        }


        /// <summary>
        /// Modify user schedule. It clears out time frame. Procrastinates all Subcalendar events, essentially freeing up sometime.
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/ProcrastinateAll")]
        public async Task<IHttpActionResult> ProcrastinateAll([FromBody]ProcrastinateModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            await retrievedUser.Login();
            TilerUser tilerUser = retrievedUser.getTilerUser();
            tilerUser.updateTimeZoneTimeSpan(UserData.getTimeSpan);
            if (retrievedUser.Status)
            {
                TimeDuration ProcrastinateDuration = UserData.ProcrastinateDuration;
                TimeSpan fullTimeSpan;
                if (!string.IsNullOrEmpty(UserData.FormattedAsISO8601))
                {
                    fullTimeSpan = System.Xml.XmlConvert.ToTimeSpan(UserData.FormattedAsISO8601);
                }
                else
                {
                    fullTimeSpan = ProcrastinateDuration.TotalTimeSpan;
                }
                TimeLine procrastinateionTimeline = UserData.getProcrastinateTimeLine();

                DateTimeOffset nowTime = myAuthorizedUser.getRefNow();

                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);

                HashSet<string> calendarIds = new HashSet<string>() { tilerUser.ClearAllId };
                DB_Schedule schedule = new DB_Schedule(retrievedUser, nowTime, calendarIds: calendarIds);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage;

                if (procrastinateionTimeline != null)
                {
                    ScheduleUpdateMessage = schedule.ProcrastinateAll(procrastinateionTimeline);
                } else
                {
                    ScheduleUpdateMessage = schedule.ProcrastinateAll(fullTimeSpan);
                }
                
                
                DB_UserActivity activity = new DB_UserActivity(myAuthorizedUser.getRefNow(), UserActivity.ActivityType.ProcrastinateAll);
                JObject json = JObject.FromObject(UserData);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                await schedule.persistToDB();
                PostBackData myPostData;
                BusyTimeLine nextBusySchedule = schedule.NextActivity;
                if (nextBusySchedule != null)
                {
                    SubCalendarEvent subEvent = schedule.getSubCalendarEvent(nextBusySchedule.Id);
                    CalendarEvent calEvent = schedule.getCalendarEvent(nextBusySchedule.Id);
                    myPostData = new PostBackData(subEvent.ToSubCalEvent(calEvent), 0);
                }
                else
                {
                    myPostData = new PostBackData("\"There aren't events for the next three months is coming up in the next three months\"", 0);
                }
                await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
                return Ok(myPostData.getPostBack);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler"
            });
        }

        /// <summary>
        /// Procrastinate a given event. 
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Procrastinate")]
        public async Task<IHttpActionResult> ProcrastinateSubCalendarEvent([FromBody]ProcrastinateEventModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            TimeDuration ProcrastinateDuration = UserData.ProcrastinateDuration;
            TimeSpan fullTimeSpan = myAuthorizedUser.getTimeSpan;
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            PostBackData retValue;
            await retrievedUser.Login();
            if (retrievedUser.Status)
            {
                retrievedUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);

                DateTimeOffset myNow = myAuthorizedUser.getRefNow();// myAuthorizedUser.getRefNow();
                HashSet<string> calendarIds = new HashSet<string>() { UserData.EventID };

                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow, calendarIds: calendarIds);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.ProcrastinateSingle);
                JObject json = JObject.FromObject(UserData);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                var ScheduleUpdateMessage = schedule.ProcrastinateJustAnEvent(UserData.EventID, ProcrastinateDuration.TotalTimeSpan);
                await schedule.persistToDB();
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Have Tiler get you something to do. 
        /// </summary>
        /// <param name="shuffleData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Shuffle")]
        public async Task<IHttpActionResult> Shuffle([FromBody]UpdateTriggerModel shuffleData)
        {
            AuthorizedUser authorizedUser = shuffleData.User;
            UserAccount retrievedUser = await shuffleData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(shuffleData.getTimeSpan);
            if (retrievedUser.Status)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                DateTimeOffset myNow = myNow = authorizedUser.getRefNow();
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow);
                schedule.CurrentLocation = authorizedUser.getCurrentLocation();
                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.Shuffle);
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                TilerElements.Location location = authorizedUser.getCurrentLocation();
                await schedule.FindMeSomethingToDo(location).ConfigureAwait(false);
                await schedule.WriteFullScheduleToLog().ConfigureAwait(false);

                List<SubCalendarEvent> allSubEvents = schedule.getAllCalendarEvents().Where(calEvent => calEvent.isActive).SelectMany(calEvent => calEvent.ActiveSubEvents).ToList();
                TimeLine timeLine = new TimeLine();
                timeLine.AddBusySlots(allSubEvents.Select(subEvent => subEvent.ActiveSlot));

                BusyTimeLine nextBusySchedule = schedule.NextActivity;
                PostBackData myPostData;
                if (nextBusySchedule != null)
                {
                    SubCalendarEvent subEvent = schedule.getSubCalendarEvent(nextBusySchedule.Id);
                    CalendarEvent calEvent = schedule.getCalendarEvent(nextBusySchedule.Id);
                    myPostData = new PostBackData(subEvent.ToSubCalEvent(calEvent), 0);
                }
                else
                {
                    myPostData = new PostBackData("\"There aren't events for the next three months is coming up in the next three months\"", 0);
                }
                
                await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
                watch.Stop();
                TimeSpan shuffleScheduleSpan = watch.Elapsed;
                Debug.WriteLine("----shuffle span " + shuffleScheduleSpan.ToString());
                return Ok(myPostData.getPostBack);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler"
            });
        }


        /// <summary>
        /// Have Tiler revise the day
        /// </summary>
        /// <param name="reviseData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Revise")]
        public async Task<IHttpActionResult> reviseSchedule([FromBody] UpdateTriggerModel reviseData)
        {
            AuthorizedUser authorizedUser = reviseData.User;
            UserAccount retrievedUser = await reviseData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(reviseData.getTimeSpan);
            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = myNow = authorizedUser.getRefNow();
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow);
                schedule.CurrentLocation = authorizedUser.getCurrentLocation();
                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.Revise);
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                TilerElements.Location location = authorizedUser.getCurrentLocation();
                await schedule.reviseSchedule(location).ConfigureAwait(false);
                await schedule.WriteFullScheduleToLog().ConfigureAwait(false);

                PostBackData retValue = new PostBackData("\"Success\"", 0);

                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
                return Ok(retValue.getPostBack);
            }

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler"
            });
        }


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/DumpData")]
        public async Task<IHttpActionResult> DumpData([FromBody]DumpModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);
            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = myNow = myAuthorizedUser.getRefNow();
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow, createDump: false);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                ScheduleDump scheduleDump = await schedule.CreateScheduleDump(notes: UserData.Notes).ConfigureAwait(false);
                scheduleDump.Notes = UserData.Notes;
                await schedule.CreateAndPersistScheduleDump(scheduleDump).ConfigureAwait(false);

                ScheduleDump scheduleDumpCopy = new ScheduleDump()
                {
                    Id = scheduleDump.Id,
                    Notes = scheduleDump.Notes,
                    UserId = scheduleDump.UserId,
                    ScheduleXmlString= "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>1024</LastIDCounter><referenceDay>8:00 AM</referenceDay><EventSchedules></EventSchedules></ScheduleLog>"
                };
                scheduleDumpCopy.Id = scheduleDump.Id;
                await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
                PostBackData postBack = new PostBackData(scheduleDumpCopy, 0);
                return Ok(postBack.getPostBack);
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler"
            });
        }

        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/DumpData")]
        public async Task<HttpResponseMessage> getDumpData([FromUri]DumpModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccount myUser = await UserData.getUserAccount(db);
            await myUser.Login();
            myUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);
            if (myUser.Status)
            {
                ScheduleDump retValue = await myUser.ScheduleLogControl.GetScheduleDump(UserData.Id).ConfigureAwait(false);
                TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
                scheduleChangeSocket.triggerRefreshData(myUser.getTilerUser());
                PostBackData postBack = new PostBackData(retValue, 0);

                System.IO.MemoryStream xmlStream = new System.IO.MemoryStream();
                retValue.XmlDoc.Save(xmlStream);
                xmlStream.Flush();
                xmlStream.Position = 0;


                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(xmlStream.ToArray())
                };
                result.Content.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = UserData.Id+".xml"
                    };
                result.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return result;
            }
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Unauthorized access to tiler"
            });
        }



        /// <summary>
        /// Marks an Event as complete.  Note this also triggers a readjust to the schedule
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvent([FromBody]getEventModel UserData)
        {
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);
            PostBackData retValue = new PostBackData("", 1);
            DB_UserActivity activity = new DB_UserActivity(UserData.getRefNow(), UserActivity.ActivityType.CompleteSingle);
            JObject json = JObject.FromObject(UserData);
            activity.updateMiscelaneousInfo(json.ToString());
            if (retrievedUser.Status)
            {
                string CalendarType = UserData.ThirdPartyType.ToLower();

                switch (CalendarType)
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, UserData.ThirdPartyUserID, "Google", db);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                            await googleControl.deleteSubEvent(UserData).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            DateTimeOffset myNow = UserData.getRefNow();
                            HashSet<string> calendarIds = new HashSet<string>() { UserData.EventID };
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow, calendarIds: calendarIds );
                            schedule.CurrentLocation = UserData.getCurrentLocation();
                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                            activity.eventIds.Add(UserData.EventID);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            schedule.markSubEventAsCompleteCalendarEventAndReadjust(UserData.EventID);
                            await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    default:
                        break;
                }
            }

            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Marks a series of Event as complete. It takes a collection of IDs and then marks them as complete
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvents([FromBody]getEventModel requestData)
        {
            UserAccount retrievedUser = await requestData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(requestData.getTimeSpan);
            DateTimeOffset myNow = requestData.getRefNow();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                string notValidUserId = "not_valid_user@mytiler.com";
                List<string> AllEventIDs = requestData.EventID.Split(',').ToList();
                List<string> thirdPartyEmailIds = requestData.ThirdPartyUserID.Split(',').ToList();
                string tilerCalendartype = ThirdPartyControl.CalendarTool.tiler.ToString();
                List<string> thirdPartyTypes = requestData.ThirdPartyType.Split(',').ToList();
                HashSet<string> calendarIds = new HashSet<string>(AllEventIDs);

                if (calendarIds.Count > 0)
                {
                    HashSet<string> tilerIds = new HashSet<string>();


                    if (AllEventIDs.Count == thirdPartyEmailIds.Count && thirdPartyEmailIds.Count == thirdPartyTypes.Count)
                    {
                        Dictionary<string, List<string>> thirdPartyUserToeventId = new Dictionary<string, List<string>>();

                        for (int i = 0; i < AllEventIDs.Count; i++)
                        {
                            string eventId = AllEventIDs[i];
                            string userId = thirdPartyEmailIds[i];
                            string thirdPartyType = thirdPartyTypes[i];
                            if (EventID.isLikeTilerId(eventId) || tilerCalendartype == thirdPartyType)
                            {
                                tilerIds.Add(eventId);
                            }
                            else
                            {
                                if (notValidUserId != userId)
                                {
                                    List<string> thirdPartyIds = new List<string>();
                                    if (!thirdPartyUserToeventId.ContainsKey(userId))
                                    {
                                        thirdPartyUserToeventId.Add(userId, thirdPartyIds);
                                    }
                                    else
                                    {
                                        thirdPartyIds = thirdPartyUserToeventId[userId];
                                    }
                                    thirdPartyIds.Add(eventId);
                                }
                            }
                        }

                        if (thirdPartyUserToeventId.Count > 0)
                        {
                            List<Task> waitForAllDeletions = new List<Task>();
                            foreach (var eachUserIdToList in thirdPartyUserToeventId)
                            {
                                Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, eachUserIdToList.Key, "Google", db);
                                GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                                var waitTask = googleControl.deleteSubEvents(eachUserIdToList.Value);
                                waitForAllDeletions.Add(waitTask);
                            }

                            foreach (var waitDeletion in waitForAllDeletions)
                            {
                                await waitDeletion.ConfigureAwait(false);
                            }
                        }

                        if (tilerIds.Count > 0)
                        {
                            var tilerIdSet = new HashSet<string>(tilerIds);
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow, calendarIds: tilerIdSet);
                            schedule.CurrentLocation = requestData.getCurrentLocation();
                            DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.CompleteMultiple, AllEventIDs);
                            JObject json = JObject.FromObject(requestData);
                            activity.updateMiscelaneousInfo(json.ToString());
                            activity.eventIds.AddRange(AllEventIDs);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                            await schedule.markSubEventsAsComplete(tilerIdSet).ConfigureAwait(false);
                            await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        retValue = new PostBackData("\"Success\"", 0);
                    }
                    else
                    {
                        retValue = new PostBackData(CustomErrors.Errors.UserEmailNotMatchingSubEvent);
                    }
                }
                else
                {
                    retValue = new PostBackData("\"Success\"", 0);
                }
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Sets the provided event as now. The ID has to be a Sub event ID
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Now")]
        public async Task<IHttpActionResult> Now([FromBody]getEventModel myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);// new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = myUser.getRefNow();
                HashSet<string> calendarIds = new HashSet<string>() { myUser.EventID };

                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                var retrievalOption = DataRetrievalSet.scheduleManipulation;
                retrievalOption.Add(DataRetrivalOption.TimeLineHistory);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow, calendarIds: calendarIds, retrievalOptions: retrievalOption);
                schedule.CurrentLocation = myUser.getCurrentLocation();
                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.SetAsNowSingle);
                JObject json = JObject.FromObject(myUser);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                var retValue0 = schedule.SetSubeventAsNow(myUser.EventID, true);
                await schedule.persistToDB();
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }



        /// <summary>
        /// Undoes the last schedule changing effect triggered on tiler.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Undo")]
        public async Task<IHttpActionResult> Undo([FromBody]AuthorizedUser myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);// new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.Undo);
                //retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                //await retrievedUser.ScheduleLogControl.Undo().ConfigureAwait(false);
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }

        

        /// <summary>
        /// Deletes a Sub event from Tiler. The deletion does not trigger a schedule readjustment.
        /// </summary>
        /// <param name="eventModel"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> DeleteEvent([FromBody]getEventModel eventModel)
        {
            UserAccount retrievedUser = await eventModel.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(eventModel.getTimeSpan);
            PostBackData retValue= new PostBackData("", 1);
            if (retrievedUser.Status)
            {
                string CalendarType = eventModel.ThirdPartyType.ToLower();

                switch(CalendarType )
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, eventModel.ThirdPartyUserID, "Google", db);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                            await googleControl.deleteSubEvent(eventModel).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);   
                        }
                        break;
                    case "tiler":
                        {
                            HashSet<string> calendarIds = new HashSet<string>() { eventModel.EventID };
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            DB_Schedule schedule = new DB_Schedule(retrievedUser, eventModel.getRefNow(), calendarIds: calendarIds);
                            schedule.CurrentLocation = eventModel.getCurrentLocation();
                            DB_UserActivity activity = new DB_UserActivity(eventModel.getRefNow(), UserActivity.ActivityType.DeleteSingle);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            JObject json = JObject.FromObject(eventModel);
                            activity.updateMiscelaneousInfo(json.ToString());
                            activity.eventIds.Add(eventModel.EventID);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                            await schedule.deleteSubCalendarEvent(eventModel.EventID).ConfigureAwait(false);
                            await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);   
                        }
                        break;
                    default:
                        break;
                }


                
            }
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Option needed to satisfy a delete request for deletion of a subevent.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> HandleOptionsEvent([FromBody]getEventModel myUser)
        {
            return Ok();
        }

        /// <summary>
        /// Deletes multiple subevents.
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events")]
        public async Task<IHttpActionResult> DeleteEvents([FromBody]getEventModel requestData)
        {
            UserAccount retrievedUser = await requestData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(requestData.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                string notValidUserId = "not_valid_user@mytiler.com";
                List<string> AllEventIDs = requestData.EventID.Split(',').ToList();
                List<string> thirdPartyEmailIds = requestData.ThirdPartyUserID.Split(',').ToList();
                string tilerCalendartype = ThirdPartyControl.CalendarTool.tiler.ToString();
                List<string> thirdPartyTypes = requestData.ThirdPartyType.Split(',').ToList();
                HashSet<string> calendarIds = new HashSet<string>(AllEventIDs);

                if(calendarIds.Count >0)
                {
                    HashSet<string> tilerIds = new HashSet<string>();
                    
                    
                    if(AllEventIDs.Count == thirdPartyEmailIds.Count && thirdPartyEmailIds.Count == thirdPartyTypes.Count)
                    {
                        Dictionary<string, List<string>> thirdPartyUserToeventId = new Dictionary<string, List<string>>();

                        for (int i = 0; i < AllEventIDs.Count; i++)
                        {
                            string eventId = AllEventIDs[i];
                            string userId = thirdPartyEmailIds[i];
                            string thirdPartyType = thirdPartyTypes[i];
                            if (EventID.isLikeTilerId(eventId)|| tilerCalendartype == thirdPartyType)
                            {
                                tilerIds.Add(eventId);
                            }
                            else
                            {
                                if(notValidUserId!=userId)
                                {
                                    List<string> thirdPartyIds = new List<string>();
                                    if (!thirdPartyUserToeventId.ContainsKey(userId))
                                    {
                                        thirdPartyUserToeventId.Add(userId, thirdPartyIds);
                                    }
                                    else
                                    {
                                        thirdPartyIds = thirdPartyUserToeventId[userId];
                                    }
                                    thirdPartyIds.Add(eventId);
                                }
                            }
                        }

                        if (thirdPartyUserToeventId.Count > 0)
                        {
                            List<Task> waitForAllDeletions = new List<Task>();
                            foreach (var eachUserIdToList in thirdPartyUserToeventId)
                            {
                                Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, eachUserIdToList.Key, "Google", db);
                                GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                                var waitTask = googleControl.deleteSubEvents(eachUserIdToList.Value);
                                waitForAllDeletions.Add(waitTask);
                            }

                            foreach (var waitDeletion in waitForAllDeletions)
                            {
                                await waitDeletion.ConfigureAwait(false);
                            }
                        }

                        if (tilerIds.Count > 0)
                        {
                            var tilerIdSet = new HashSet<string>(tilerIds);
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            DB_Schedule schedule = new DB_Schedule(retrievedUser, requestData.getRefNow(), calendarIds: tilerIdSet);
                            schedule.CurrentLocation = requestData.getCurrentLocation();
                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                            DB_UserActivity activity = new DB_UserActivity(requestData.getRefNow(), UserActivity.ActivityType.DeleteMultiple, AllEventIDs);
                            JObject json = JObject.FromObject(requestData);
                            activity.updateMiscelaneousInfo(json.ToString());
                            activity.eventIds.AddRange(AllEventIDs);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            await schedule.deleteSubCalendarEvents(tilerIdSet);
                            await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                        }
                        retValue = new PostBackData("\"Success\"", 0);
                    } else
                    {
                        retValue = new PostBackData(CustomErrors.Errors.UserEmailNotMatchingSubEvent);
                    }
                }
                else
                {
                    retValue = new PostBackData("\"Success\"", 0);
                }
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Option needed to satisfy a delete request for multiple subevents.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events")]
        public async Task<IHttpActionResult> HandleOptionsEvents([FromBody]getEventModel myUser)
        {
            return Ok();
        }


        /// <summary>
        /// Adds an Event to Tiler. Returns data formatted as tiler endpoint together. Returns the earliest sub calendarevent generated. 
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> NewCalEvent([FromBody]UnregisteredEvent newEvent)//, [FromUri]AuthorizedUser userEntry)
        {
            string BColor = newEvent.BColor;
            string RColor = newEvent.RColor;
            string GColor = newEvent.GColor;
            string Opacity = newEvent.Opacity;
            string ColorSelection = newEvent.ColorSelection;
            int Count = Convert.ToInt32(newEvent.Count??1.ToString()) ;
            string DurationDays = newEvent.DurationDays;
            string DurationHours = newEvent.DurationHours;
            string DurationMins = newEvent.DurationMins;
            string EndDay = newEvent.EndDay;
            string EndHour = newEvent.EndHour;
            string EndMins = newEvent.EndMins;
            string EndMonth = newEvent.EndMonth;
            string EndYear = newEvent.EndYear;

            string LocationId = newEvent.LocationId;
            string LocationAddress = newEvent.LocationAddress;
            string LocationTag = newEvent.LocationTag;
            EventName Name = new EventName(null, null, newEvent.Name);

            string RepeatData = newEvent.RepeatData;
            string RepeatEndDay = newEvent.RepeatEndDay;
            string RepeatEndMonth = newEvent.RepeatEndMonth;
            string RepeatEndYear = newEvent.RepeatEndYear;
            string RepeatStartDay = newEvent.RepeatStartDay;
            string RepeatStartMonth = newEvent.RepeatStartMonth;
            string RepeatStartYear = newEvent.RepeatStartYear;
            string RepeatType = newEvent.RepeatType;
            string RepeatWeeklyData = newEvent.RepeatWeeklyData;
            string Rigid = newEvent.Rigid;
            string StartDay = newEvent.StartDay;
            string StartHour = newEvent.StartHour;
            string StartMins = newEvent.StartMins;
            string StartMonth = newEvent.StartMonth;
            string StartYear = newEvent.StartYear;
            string RepeatFrequency = newEvent.RepeatFrequency;
            string TimeZone = newEvent.TimeZone;
            string TimeZoneOrigin = newEvent.TimeZoneOrigin;
            string restrictionPreference = newEvent.isRestricted;
            string lookupString_arg = newEvent.LookupString;
            string locationIsVerified_arg = newEvent.LocationIsVerified;

            bool restrictionFlag = Convert.ToBoolean(restrictionPreference);
            bool locationIsVerified = Convert.ToBoolean(locationIsVerified_arg);
            string lookupString = lookupString_arg;


            string StartTime = StartHour + ":" + StartMins;
            string EndTime = EndHour + ":" + EndMins;
            UserAccount retrievedUser = await newEvent.getUserAccount(db);

            DateTimeOffset StartDateEntry = new DateTimeOffset(Convert.ToInt32(StartYear), Convert.ToInt32(StartMonth), Convert.ToInt32(StartDay), 0, 0, 0, new TimeSpan());
            DateTimeOffset EndDateEntry = new DateTimeOffset(Convert.ToInt32(EndYear), Convert.ToInt32(EndMonth), Convert.ToInt32(EndDay), 0, 0, 0, new TimeSpan());

            TimeSpan fullTimeSpan = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
            TimeSpan EventDuration = TimeSpan.FromSeconds(fullTimeSpan.TotalSeconds * Convert.ToInt32(Count));

            bool RigidScheduleFlag = Convert.ToBoolean(Rigid);
            TilerElements.Location EventLocation = new TilerElements.Location(LocationAddress, LocationTag);
            EventLocation.LookupString = lookupString;
            if (locationIsVerified)
            {
                EventLocation.verify();
            }
            
            Location retrievedLocation =await db.Locations.SingleOrDefaultAsync(location => location.UserId == retrievedUser.UserID && location.SearchdDescription == EventLocation.SearchdDescription).ConfigureAwait(false);
            if(retrievedLocation != null)
            {
                bool resetLocationValidation = (string.IsNullOrEmpty(LocationId) || string.IsNullOrWhiteSpace(LocationId)) || retrievedLocation.Id != LocationId;
                retrievedLocation.update(EventLocation, resetLocationValidation);
                EventLocation = retrievedLocation;
            }
            Repetition MyRepetition = new Repetition();
            DateTimeOffset RepeatStart = new DateTimeOffset();
            DateTimeOffset RepeatEnd = new DateTimeOffset();
            bool RepetitionFlag = false;
            TilerColor userColor = new TilerColor(Convert.ToInt32(RColor), Convert.ToInt32(GColor), Convert.ToInt32(BColor), Convert.ToInt32(Opacity), Convert.ToInt32(ColorSelection));

            if (RigidScheduleFlag)//this needs to be called after the initialization of restrictionFlag
            {
                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);
                FullStartTime = FullStartTime.Add(-newEvent.getTimeSpan);
                FullEndTime = FullEndTime.Add(-newEvent.getTimeSpan);
                EventDuration = (FullEndTime - FullStartTime);
                restrictionFlag = false;
            }

            
            if (!string.IsNullOrEmpty(RepeatType))
            {
                

                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                FullStartTime = FullStartTime.Add(-newEvent.getTimeSpan);
                FullEndTime = FullEndTime.Add(-newEvent.getTimeSpan);

                RepeatEnd = new DateTimeOffset(Convert.ToInt32(RepeatEndYear), Convert.ToInt32(RepeatEndMonth), Convert.ToInt32(RepeatEndDay), 23, 59, 0, new TimeSpan());
                RepeatEnd = RepeatEnd.Add(-newEvent.getTimeSpan);
                if (!RigidScheduleFlag)
                {
                    //DateTimeOffset newEndTime = FullEndTime;

                    string Frequency = RepeatFrequency.Trim().ToUpper();
                    switch(Frequency)
                    {
                        case "DAILY":
                            //FullEndTime = FullStartTime.AddDays(1);
                            break;
                        case "WEEKLY":
                            //FullEndTime = FullStartTime.AddDays(7);
                            break;
                        case "MONTHLY":
                            //FullEndTime = FullStartTime.AddMonths(1);
                            break;
                        case "YEARLY":
                            //FullEndTime = FullStartTime.AddYears(1);
                            break;
                        default:
                            break;
                    }

                    //RepeatEnd = newEndTime;
                }


                

                RepeatStart = StartDateEntry.Add(-newEvent.getTimeSpan);
                DayOfWeek[] selectedDaysOftheweek={};
                RepeatWeeklyData = string.IsNullOrEmpty( RepeatWeeklyData )?"":RepeatWeeklyData.Trim();
                if (!string.IsNullOrEmpty(RepeatWeeklyData))
                {
                    selectedDaysOftheweek = RepeatWeeklyData.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => Convert.ToInt32(obj)).Select(num => (DayOfWeek)num).ToArray();
                }
  
                if(RepeatStart>=RepeatEnd)
                {
                    var postResult = new PostBackData(CustomErrors.Errors.Creation_Config_RepeatEnd_Earlier_Than_RepeatStart);
                    return Ok(postResult.getPostBack);
                }

                RepetitionFlag = true;
                MyRepetition = new Repetition(new TimeLine(RepeatStart, RepeatEnd), Utility.ParseEnum<Repetition.Frequency> (RepeatFrequency.ToUpper()), new TimeLine(FullStartTime, FullEndTime), selectedDaysOftheweek);
                EndDateEntry = MyRepetition.Range.End > EndDateEntry ? MyRepetition.Range.End : EndDateEntry;
            }

            PostBackData retValue;
            await retrievedUser.Login();
            TilerUser tilerUser = retrievedUser.getTilerUser();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(newEvent.getTimeSpan);
            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                CalendarEvent newCalendarEvent;
                RestrictionProfile myRestrictionProfile = newEvent.getRestrictionProfile(myNow);
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow);
                schedule.CurrentLocation = newEvent.getCurrentLocation();
                if (myRestrictionProfile != null)
                {
                    string TimeString = StartDateEntry.Date.ToShortDateString() + " " + StartTime;
                    DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    StartDateTime = StartDateTime.Add(-newEvent.getTimeSpan);
                    TimeString = EndDateEntry.Date.ToShortDateString() + " " + EndTime;
                    DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    EndDateTime = EndDateTime.Add(-newEvent.getTimeSpan);

                    newCalendarEvent = new CalendarEventRestricted(tilerUser, new TilerUserGroup(), Name, StartDateTime, EndDateTime, myRestrictionProfile, EventDuration, MyRepetition, false, true, Count, RigidScheduleFlag, new NowProfile(), EventLocation, new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0),null, schedule.Now, new Procrastination(Utility.JSStartTime, new TimeSpan()), null, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), TimeZone);
                }
                else
                {
                    DateTimeOffset StartData = DateTimeOffset.Parse(StartTime+" "+StartDateEntry.Date.ToShortDateString()).UtcDateTime;
                    StartData = StartData.Add(-newEvent.getTimeSpan);
                    DateTimeOffset EndData = DateTimeOffset.Parse(EndTime + " " + EndDateEntry.Date.ToShortDateString()).UtcDateTime;
                    EndData = EndData.Add(-newEvent.getTimeSpan);
                    if (StartData >= EndData)
                    {
                        var postResult = new PostBackData(CustomErrors.Errors.Creation_Config_End_Earlier_Than_Start);
                        return Ok(postResult.getPostBack);
                    }

                    if (RigidScheduleFlag) {
                        newCalendarEvent = new RigidCalendarEvent(
                            Name, StartData, EndData, EventDuration,new TimeSpan(), new TimeSpan(), MyRepetition, EventLocation,  new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), true,false, tilerUser, new TilerUserGroup(), TimeZone, null, new NowProfile(), null);
                    }
                    else
                    {
                        newCalendarEvent = new CalendarEvent(
                            Name, StartData, EndData, EventDuration, new TimeSpan(), new TimeSpan(), Count, MyRepetition, EventLocation, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), new Procrastination(new DateTimeOffset(), new TimeSpan()), new NowProfile(), true, false, tilerUser, new TilerUserGroup(), TimeZone, null, null);
                    }
                }
                Name.Creator_EventDB = newCalendarEvent.getCreator;
                Name.AssociatedEvent = newCalendarEvent;
                Task DoInitializeClassification=newCalendarEvent.InitializeClassification();
                MyRepetition.ParentEvent = newCalendarEvent;

                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                await DoInitializeClassification;
                if (newCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                {
                    if(newCalendarEvent.getIsEventRestricted)
                    {
                        newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent as CalendarEventRestricted);
                    }
                    else
                    {
                        newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent);
                    }
                    
                }
                string BeforemyName = newCalendarEvent.ToString(); //BColor + " -- " + Count + " -- " + DurationDays + " -- " + DurationHours + " -- " + DurationMins + " -- " + EndDay + " -- " + EndHour + " -- " + EndMins + " -- " + EndMonth + " -- " + EndYear + " -- " + GColor + " -- " + LocationAddress + " -- " + LocationTag + " -- " + Name + " -- " + RColor + " -- " + RepeatData + " -- " + RepeatEndDay + " -- " + RepeatEndMonth + " -- " + RepeatEndYear + " -- " + RepeatStartDay + " -- " + RepeatStartMonth + " -- " + RepeatStartYear + " -- " + RepeatType + " -- " + RepeatWeeklyData + " -- " + Rigid + " -- " + StartDay + " -- " + StartHour + " -- " + StartMins + " -- " + StartMonth + " -- " + StartYear;
                string AftermyName = newCalendarEvent.ToString();
                CustomErrors userError = newCalendarEvent.Error;
                retrievedUser.ScheduleLogControl.updateNewLocation(EventLocation);
                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.NewEventCreation);
                JObject json = JObject.FromObject(newEvent);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                try
                {
                    userError = await schedule.AddToScheduleAndCommitAsync(newCalendarEvent).ConfigureAwait(false);
                } 
                catch(CustomErrors computeError)
                {
                    userError = computeError;
                }
                
                
                
                int errorCode = userError?.Code ?? 0;
                retValue = new PostBackData(newCalendarEvent.ActiveSubEvents.First().ToSubCalEvent(newCalendarEvent), errorCode);
                
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }



        /// <summary>
        /// Triggers a schedule update when a notification is received pertaining third party event modification
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Notification")]
        public async Task<IHttpActionResult> Notification([FromBody]AuthorizedUser newEvent)
        {
            PostBackData retValue = new PostBackData("", 1);
            try
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                UserAccount retrievedUser = await newEvent.getUserAccount(db).ConfigureAwait(false);
                await retrievedUser.Login();
                if (retrievedUser.Status)
                {
                    Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                    DB_Schedule schedule = new DB_Schedule(retrievedUser, myNow);
                    schedule.CurrentLocation = newEvent.getCurrentLocation();
                    var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                    schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                    await schedule.UpdateScheduleDueToExternalChanges().ConfigureAwait(false);
                    retValue = new PostBackData("\"Success\"", 0);
                }
                else
                {
                    retValue = new PostBackData("", 1);
                }
            }
            catch
            {
                ;
            }
            return Ok(retValue.getPostBack);
            
        }


        /// <summary>
        /// Peeks into user schedule. Generates an update to a schedule for any changes.
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Peek")]
        public async Task<IHttpActionResult> PeekCalEvent([FromBody]UnregisteredEvent newEvent)
        {
            string BColor = newEvent.BColor;
            string RColor = newEvent.RColor;
            string GColor = newEvent.GColor;
            string Opacity = newEvent.Opacity;
            string ColorSelection = newEvent.ColorSelection;
            int Count = Convert.ToInt32(newEvent.Count ?? 1.ToString());
            string DurationDays = newEvent.DurationDays;
            string DurationHours = newEvent.DurationHours;
            string DurationMins = newEvent.DurationMins;
            string EndDay = newEvent.EndDay.isNot_NullEmptyOrWhiteSpace() && newEvent.EndDay != "NaN" ? newEvent.EndDay : (1).ToString();
            string EndHour = newEvent.EndHour.isNot_NullEmptyOrWhiteSpace() && newEvent.EndHour != "NaN" ? newEvent.EndHour : (0).ToString();
            string EndMins = newEvent.EndMins.isNot_NullEmptyOrWhiteSpace() && newEvent.EndMins != "NaN" ? newEvent.EndMins : (0).ToString();
            string EndMonth = newEvent.EndMonth.isNot_NullEmptyOrWhiteSpace() && newEvent.EndMonth != "NaN" ? newEvent.EndMonth : (1).ToString();
            string EndYear = newEvent.EndYear.isNot_NullEmptyOrWhiteSpace() && newEvent.EndYear != "NaN" ? newEvent.EndYear : (DateTimeOffset.UtcNow.Year + 20).ToString();

            string LocationAddress = string.IsNullOrEmpty( newEvent.LocationAddress)?"": newEvent.LocationAddress;
            string LocationTag = LocationAddress = string.IsNullOrEmpty(newEvent.LocationTag) ? "" : newEvent.LocationTag;
            EventName Name = new EventName(null, null, newEvent.Name);

            string RepeatData = newEvent.RepeatData;
            string RepeatEndDay = newEvent.RepeatEndDay;
            string RepeatEndMonth = newEvent.RepeatEndMonth;
            string RepeatEndYear = newEvent.RepeatEndYear;
            string RepeatStartDay = newEvent.RepeatStartDay;
            string RepeatStartMonth = newEvent.RepeatStartMonth;
            string RepeatStartYear = newEvent.RepeatStartYear;
            string RepeatType = newEvent.RepeatType;
            string RepeatWeeklyData = newEvent.RepeatWeeklyData;
            string Rigid = newEvent.Rigid;
            string StartDay = newEvent.StartDay;
            string StartHour = newEvent.StartHour;
            string StartMins = newEvent.StartMins;
            string StartMonth = newEvent.StartMonth;
            string StartYear = newEvent.StartYear;
            string RepeatFrequency = newEvent.RepeatFrequency;
            string TimeZone = newEvent.TimeZone;
            string lookupString_arg = newEvent.LookupString;
            string locationIsVerified_arg = newEvent.LocationIsVerified;
            bool locationIsVerified = Convert.ToBoolean(locationIsVerified_arg);
            string lookupString = lookupString_arg;

            string restrictionPreference = newEvent.isRestricted;

            bool restrictionFlag = Convert.ToBoolean(restrictionPreference);

            string StartTime = StartHour + ":" + StartMins;
            string EndTime = EndHour + ":" + EndMins;
            DateTimeOffset StartDateEntry = new DateTimeOffset(Convert.ToInt32(StartYear), Convert.ToInt32(StartMonth), Convert.ToInt32(StartDay), 0, 0, 0, new TimeSpan());
            DateTimeOffset EndDateEntry = new DateTimeOffset(Convert.ToInt32(EndYear), Convert.ToInt32(EndMonth), Convert.ToInt32(EndDay), 0, 0, 0, new TimeSpan());
            
            TimeSpan fullTimeSpan = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
            TimeSpan EventDuration = TimeSpan.FromSeconds(fullTimeSpan.TotalSeconds * Convert.ToInt32(Count));

            bool RigidScheduleFlag = Convert.ToBoolean(Rigid);
            TilerElements.Location EventLocation = new TilerElements.Location(LocationAddress, LocationTag);
            EventLocation.LookupString = lookupString;
            if (locationIsVerified)
            {
                EventLocation.verify();
            }
                

            Repetition MyRepetition = new Repetition();
            DateTimeOffset RepeatStart = new DateTimeOffset();
            DateTimeOffset RepeatEnd = new DateTimeOffset();
            bool RepetitionFlag = false;
            TilerColor userColor = new TilerColor(Convert.ToInt32(RColor), Convert.ToInt32(GColor), Convert.ToInt32(BColor), Convert.ToInt32(Opacity), Convert.ToInt32(ColorSelection));

            if (RigidScheduleFlag)
            {
                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);
                FullStartTime = FullStartTime.Add(-newEvent.getTimeSpan);
                FullEndTime = FullEndTime.Add(-newEvent.getTimeSpan);
                EventDuration = (FullEndTime - FullStartTime);
            }

            if (!string.IsNullOrEmpty(RepeatType))
            {

                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                FullStartTime = FullStartTime.Add(-newEvent.getTimeSpan);
                FullEndTime = FullEndTime.Add(-newEvent.getTimeSpan);

                RepeatEnd = new DateTimeOffset(Convert.ToInt32(RepeatEndYear), Convert.ToInt32(RepeatEndMonth), Convert.ToInt32(RepeatEndDay), 23, 59, 0, new TimeSpan());
                RepeatEnd = RepeatEnd.Add(-newEvent.getTimeSpan);
                if (!RigidScheduleFlag)
                {
                    DateTimeOffset newEndTime = FullEndTime;

                    string Frequency = RepeatFrequency.Trim().ToUpper();
                    RepeatEnd = newEndTime;
                }




                RepeatStart = StartDateEntry;
                DayOfWeek[] selectedDaysOftheweek = { };
                RepeatWeeklyData = string.IsNullOrEmpty(RepeatWeeklyData) ? "" : RepeatWeeklyData.Trim();
                if (!string.IsNullOrEmpty(RepeatWeeklyData))
                {
                    selectedDaysOftheweek = RepeatWeeklyData.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => Convert.ToInt32(obj)).Select(num => (DayOfWeek)num).ToArray();
                }


                //RepeatEnd = (DateTimeOffset.UtcNow).AddDays(7);
                RepetitionFlag = true;
                MyRepetition = new Repetition(new TimeLine(RepeatStart, RepeatEnd), Utility.ParseEnum<Repetition.Frequency>( RepeatFrequency.ToUpper()), new TimeLine(FullStartTime, FullEndTime), selectedDaysOftheweek);
                EndDateEntry = RepeatEnd;
            }

            UserAccount retrievedUser = await newEvent.getUserAccount(db);
            PostBackData retValue;
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(newEvent.getTimeSpan);
            TilerUser tilerUser = retrievedUser.getTilerUser();

            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                Schedule schedule = new DB_Schedule(retrievedUser, myNow, createDump: false);
                schedule.CurrentLocation = newEvent.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);

                CalendarEvent newCalendarEvent;
                RestrictionProfile myRestrictionProfile = newEvent.getRestrictionProfile(myNow);
                if (myRestrictionProfile != null)
                {
                    string TimeString = StartDateEntry.Date.ToShortDateString() + " " + StartTime;
                    DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    StartDateTime = StartDateTime.Add(-newEvent.getTimeSpan);
                    TimeString = EndDateEntry.Date.ToShortDateString() + " " + EndTime;
                    DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    EndDateTime = EndDateTime.Add(-newEvent.getTimeSpan);
                    newCalendarEvent = new CalendarEventRestricted(tilerUser, new TilerUserGroup(), Name, StartDateTime, EndDateTime, myRestrictionProfile, EventDuration, MyRepetition, false, true, Count, RigidScheduleFlag, new NowProfile(), new TilerElements.Location(), new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), null, schedule.Now, new Procrastination(Utility.BeginningOfTime, new TimeSpan()), null, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData());
                }
                else
                {
                    DateTimeOffset StartData = DateTimeOffset.Parse(StartTime + " " + StartDateEntry.Date.ToShortDateString()).UtcDateTime;
                    StartData = StartData.Add(-newEvent.getTimeSpan);
                    StartData = newEvent.getRefNow();
                    DateTimeOffset EndData = DateTimeOffset.Parse(EndTime + " " + EndDateEntry.Date.ToShortDateString()).UtcDateTime;
                    EndData = EndData.Add(-newEvent.getTimeSpan);
                    if (RigidScheduleFlag)
                    {
                        newCalendarEvent = new RigidCalendarEvent(
                            Name, StartData, EndData, EventDuration, new TimeSpan(), new TimeSpan(), MyRepetition, EventLocation, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), true, false, tilerUser, new TilerUserGroup(), TimeZone, null, new NowProfile(), null);
                    }
                    else
                    {
                        newCalendarEvent = new CalendarEvent(
                            Name, StartData, EndData, EventDuration, new TimeSpan(), new TimeSpan(), Count, MyRepetition, EventLocation, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), null, new NowProfile(), true, false, tilerUser, new TilerUserGroup(), TimeZone, null, null);
                    }
                }
                Name.Creator_EventDB = newCalendarEvent.getCreator;
                Name.AssociatedEvent = newCalendarEvent;
                if (newCalendarEvent.IsFromRecurringAndNotChildRepeatCalEvent)
                {
                    if (newCalendarEvent.getIsEventRestricted)
                    {
                        newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent as CalendarEventRestricted);
                    }
                    else
                    {
                        newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent);
                    }
                }

                string BeforemyName = newCalendarEvent.ToString(); 
                string AftermyName = newCalendarEvent.ToString();
#if loadFromXml
                if (!string.IsNullOrEmpty(xmlFileId) && !string.IsNullOrWhiteSpace(xmlFileId)) {
                    var tempSched = TilerTests.TestUtility.getSchedule(xmlFileId, connectionName: "DefaultConnection", filePath: LogControl.getLogLocation());
                    MySchedule = (DB_Schedule)tempSched.Item1;
                }
#endif
                TimeLine timeline = new TimeLine(schedule.Now.constNow.AddDays(-45), schedule.Now.constNow.AddDays(45));
                var tupleOfSUbEVentsAndAnalysis = retrievedUser.ScheduleLogControl.getSubCalendarEventForAnalysis(timeline, retrievedUser.ScheduleLogControl.getTilerRetrievedUser());
                List<SubCalendarEvent> subEvents = tupleOfSUbEVentsAndAnalysis.Item1.ToList();
                Analysis analysis = tupleOfSUbEVentsAndAnalysis.Item2;

                ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(subEvents, retrievedUser.ScheduleLogControl.Now, retrievedUser.ScheduleLogControl.getTilerRetrievedUser(), analysis);
                DateTimeOffset deadline = scheduleSuggestion.evaluateIdealDeadline(newCalendarEvent, schedule.getAllActiveCalendarEvents().ToList());


                Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekingEvents = schedule.peekIntoSchedule(newCalendarEvent);
                PeekResult peekData = new PeekResult(peekingEvents.Item1, peekingEvents.Item2, peekingEvents.Item3);
                peekData.DeadlineSuggestion = deadline.ToUnixTimeMilliseconds();

                CustomErrors userError = newCalendarEvent.Error;
                int errorCode = userError?.Code ?? 0;
                retValue = new PostBackData(peekData, errorCode);

            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Creates a restriction profile to be used when generating a new event.
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <param name="workWeek"></param>
        /// <param name="TimeZoneOffSet"></param>
        /// <param name="DaySelection"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
