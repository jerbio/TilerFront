using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TilerElements;
using TilerFront.Models;
using DBTilerElement;
using Newtonsoft.Json.Linq;
using TilerCore;

namespace TilerFront.Controllers
{

    public class CalendarEventController : TilerApiController
    {
        // GET api/CalendarEvent/5
        /// <summary>
        /// Retrieve a calendar event by the ID, and registered user account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> GetCalEvent(string id,[FromUri]AuthorizedUser myUser )
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);
            await retrievedUser.Login();
            TilerElements.CalendarEvent retrievedCalendarEvent = await retrievedUser.ScheduleLogControl.getCalendarEventWithID(id);
            PostBackData retValue = new PostBackData(retrievedCalendarEvent.ToCalEvent(), 0);


            return Ok(retValue.getPostBack);
        }
        
        // GET api/CalendarEvent/Name
        
        /// <summary>
        /// Look up a calendar event by name
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Name")]
        public async Task<IHttpActionResult> CalEventName([FromUri]NameSearchModel myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);
            await retrievedUser.Login();
            string phrase = myUser.Data;

            PostBackData retValue = new PostBackData("", 4);
            if (retrievedUser.Status)
            {
                long myNow = (long)(DateTimeOffset.UtcNow - TilerElementExtension.JSStartTime).TotalMilliseconds;
                IEnumerable<CalendarEvent> retrievedCalendarEvents = (await retrievedUser.ScheduleLogControl.getEnabledCalendarEventWithName(phrase));
                    //.Where(obj => obj.isActive);
                var allCalEvent = retrievedCalendarEvents
                    .ToList();
                retValue = new PostBackData(
                    allCalEvent
                    .OrderByDescending(obj => obj.TimeCreated)
                    .ThenByDescending(obj => obj.End)
                    .Select(obj => obj.ToCalEvent(includeSubevents: false))
                    .ToList(), 0);
            }
            
                
            
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Deletes a calendar event.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent")]
        public async Task<IHttpActionResult> DeleteCalEvent([FromBody]getEventModel myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                HashSet<string> calendarIds = new HashSet<string>() { myUser.EventID };
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myUser.getRefNow(), calendarIds: calendarIds);
                schedule.CurrentLocation = myUser.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.DeleteCalendarEvent, new List<String>() { myUser.EventID });
                JObject json = JObject.FromObject(myUser);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                CustomErrors messageReturned = await schedule.deleteCalendarEventAndReadjust(myUser.EventID).ConfigureAwait(false);
                await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                int errorCode = messageReturned?.Code ?? 0;
                retValue = new PostBackData(messageReturned, errorCode);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            return Ok(retValue.getPostBack);
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent")]
        public async Task<IHttpActionResult> HandleOptionsEvent([FromBody]getEventModel myUser)
        {
            return Ok();
        }

        /// <summary>
        /// MArks the whole calendar event as complete.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Complete")]
        public async Task<IHttpActionResult> CompleteCalEvent([FromBody]getEventModel myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                HashSet<string> calendarIds = new HashSet<string>() { myUser.EventID };

                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myUser.getRefNow(), calendarIds: calendarIds);
                schedule.CurrentLocation = myUser.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.CompleteCalendarEvent, new List<String>(){myUser.EventID});
                JObject json = JObject.FromObject(myUser);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                CustomErrors messageReturned = await schedule.markAsCompleteCalendarEventAndReadjust(myUser.EventID).ConfigureAwait(false);
                await schedule.WriteFullScheduleToLog().ConfigureAwait(false);
                int errorCode = messageReturned?.Code ?? 0;
                retValue = new PostBackData(messageReturned, errorCode);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Sets the earleiest valid sub event to current time. Essentially, makes the event have a start time of now.
        /// </summary>
        /// <param name="nowEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Now")]
        public async Task<IHttpActionResult> Now( [FromBody]NowEventModel nowEvent)
        {
            UserAccount retrievedUser = await nowEvent.getUserAccount(db); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(nowEvent.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                HashSet<string> calendarIds = new HashSet<string>() { nowEvent.ID };
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, nowEvent.getRefNow(), includeUpdateHistory: true, calendarIds: calendarIds);
                schedule.CurrentLocation = nowEvent.getCurrentLocation();
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                var ScheduleUpdateMessage = schedule.SetCalendarEventAsNow(nowEvent.ID);
                DB_UserActivity activity = new DB_UserActivity(nowEvent.getRefNow(), UserActivity.ActivityType.SetAsNowCalendarEvent, new List<String>() { nowEvent.ID });
                JObject json = JObject.FromObject(nowEvent);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                await schedule.persistToDB().ConfigureAwait(false);
                retValue = new PostBackData(ScheduleUpdateMessage.Item1);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Updates the calendar event properties in the information  sent.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Update")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]EditCalEventModel myUser)    
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue = new PostBackData("", 1);



            if (retrievedUser.Status)
            {
                string CalendarType = myUser.ThirdPartyType.ToLower();

                switch (CalendarType)
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await ScheduleController.getThirdPartyAuthentication(retrievedUser.UserID, myUser.ThirdPartyUserID, "Google", db);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                            await googleControl.updateSubEvent(myUser).ConfigureAwait(false);
                            Dictionary<string, CalendarEvent>AllCalendarEvents =  (await googleControl.getCalendarEvents(null, true).ConfigureAwait(false)).ToDictionary(obj=>obj.getId, obj=>obj);

                            GoogleThirdPartyControl googleEvents = new GoogleThirdPartyControl(AllCalendarEvents, AllIndexedThirdParty.getTilerUser());

                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow(), true);
                            NewSchedule.CurrentLocation = myUser.getCurrentLocation();
                            await NewSchedule.updateDataSetWithThirdPartyDataAndTriggerNewAddition(new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent> { googleEvents.getThirdpartyCalendarEvent() })).ConfigureAwait(false);

                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            HashSet<string> calendarIds = new HashSet<string>() { myUser.EventID };
                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow(), true, calendarIds: calendarIds);
                            NewSchedule.CurrentLocation = myUser.getCurrentLocation();
                            DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(myUser.Start);
                            newStart = newStart.Add(myUser.getTimeSpan);
                            DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(myUser.End);
                            newEnd = newEnd.Add(myUser.getTimeSpan);
                            int SplitCount = (int)myUser.Split;
                            TimeSpan SpanPerSplit = TimeSpan.FromMilliseconds(myUser.Duration);
                            CalendarEvent calEvent = NewSchedule.getCalendarEvent(myUser.EventID);
                            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID, new EventName(retrievedUser.getTilerUser(), calEvent, myUser.EventName), newStart, newEnd, SplitCount, myUser.EscapedNotes);
                            DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.InternalUpdateCalendarEvent, new List<String>() { myUser.EventID });
                            JObject json = JObject.FromObject(myUser);
                            activity.updateMiscelaneousInfo(json.ToString());

                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                            await NewSchedule.persistToDB().ConfigureAwait(false);
                            retValue = new PostBackData(ScheduleUpdateMessage.Item1);
                        }
                        break;
                    default:
                        break;
                }
            }



            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
            await AnalysisController.updateSuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            return Ok(retValue.getPostBack);
        }
    }
}