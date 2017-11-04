﻿using System;
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
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(db);
            await retrievedUser.Login();
            TilerElements.CalendarEvent retrievedCalendarEvent = retrievedUser.ScheduleLogControl.getCalendarEventWithID(id);
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
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(db);
            await retrievedUser.Login();
            string phrase = myUser.Data;

            PostBackData retValue = new PostBackData("", 4);
            if (retrievedUser.Status)
            {
                long myNow = (long)(DateTimeOffset.UtcNow - TilerElementExtension.JSStartTime).TotalMilliseconds; ;
                IEnumerable<CalendarEvent> retrievedCalendarEvents = retrievedUser.ScheduleLogControl.getCalendarEventWithName(phrase).Where(obj => obj.isActive);
                retValue = new PostBackData(retrievedCalendarEvents.Select(obj => obj.ToCalEvent()).OrderBy(obj => Math.Abs(myNow - obj.EndDate)).ToList(), 0);
            }
            
                
            
            return Ok(retValue.getPostBack);
        }


        /*
        // POST api/CalendarEvent
        [ResponseType(typeof(CalEvent))]
        public async Task<IHttpActionResult> PostCalEvent([FromBody]CalEvent calevent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.CalEvents.Add(calevent);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CalEventExists(calevent.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = calevent.ID }, calevent);
        }
        
        // DELETE api/CalendarEvent/5
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> DeleteCalEvent(string id, bool readjust, [FromBody]AuthorizedUser myUser )
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccount();// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue ;
            if(retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, new DateTime(myUser.getRefNow().Ticks));
                CustomErrors messageReturned= NewSchedule.deleteCalendarEventAndReadjust(id);
                retValue = new PostBackData(messageReturned, messageReturned.Code);
            }
            else
            {
                retValue = new PostBackData("",1);
            }

            return Ok(retValue.getPostBack);
        }
        */
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
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(db);// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow());
                DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.DeleteCalendarEvent, new List<String>() { myUser.EventID });
                JObject json = JObject.FromObject(myUser);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                CustomErrors messageReturned = await NewSchedule.deleteCalendarEventAndReadjust(myUser.EventID).ConfigureAwait(false);
                await NewSchedule.WriteFullScheduleToLogAndOutlook().ConfigureAwait(false);
                int errorCode = messageReturned?.Code ?? 0;
                retValue = new PostBackData(messageReturned, errorCode);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            TilerFront.SocketHubs.ScheduleChange scheduleChangeSocket = new TilerFront.SocketHubs.ScheduleChange();
            scheduleChangeSocket.triggerRefreshData(retrievedUser.getTilerUser());
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
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(db);// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow());
                DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.CompleteCalendarEvent, new List<String>(){myUser.EventID});
                JObject json = JObject.FromObject(myUser);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                CustomErrors messageReturned = await NewSchedule.markAsCompleteCalendarEventAndReadjust(myUser.EventID).ConfigureAwait(false);
                await NewSchedule.WriteFullScheduleToLogAndOutlook().ConfigureAwait(false);
                int errorCode = messageReturned?.Code ?? 0;
                retValue = new PostBackData(messageReturned, errorCode);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
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
            UserAccountDirect retrievedUser = await nowEvent.getUserAccountDirect(db); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            DateTime myDate;
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, nowEvent.getRefNow());

                await ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(NewSchedule, nowEvent.UserID, db).ConfigureAwait(false);

                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.SetCalendarEventAsNow(nowEvent.ID);
                DB_UserActivity activity = new DB_UserActivity(nowEvent.getRefNow(), UserActivity.ActivityType.SetAsNowCalendarEvent, new List<String>() { nowEvent.ID });
                JObject json = JObject.FromObject(nowEvent);
                activity.updateMiscelaneousInfo(json.ToString());
                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                await NewSchedule.UpdateWithDifferentSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                retValue = new PostBackData(ScheduleUpdateMessage.Item1);
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
        /// Updates the calendar event properties in the information  sent.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Update")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]EditCalEventModel myUser)    
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(db); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
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
                            Dictionary<string, CalendarEvent>AllCalendarEvents =  (await googleControl.getCalendarEvents().ConfigureAwait(false)).ToDictionary(obj=>obj.getId, obj=>obj);

                            GoogleThirdPartyControl googleEvents = new GoogleThirdPartyControl(AllCalendarEvents, AllIndexedThirdParty.getTilerUser());

                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow());
                            await NewSchedule.updateDataSetWithThirdPartyDataAndTriggerNewAddition(new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent> { googleEvents.getThirdpartyCalendarEvent() })).ConfigureAwait(false);

                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myUser.getRefNow());
                            
                            DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(myUser.Start);
                            newStart = newStart.Add(myUser.getTImeSpan);
                            DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(myUser.End);
                            newEnd = newEnd.Add(myUser.getTImeSpan);
                            int SplitCount = (int)myUser.Split;
                            TimeSpan SpanPerSplit = TimeSpan.FromMilliseconds(myUser.Duration);
                            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID, myUser.EventName, newStart, newEnd, SplitCount);//, SpanPerSplit);
                            DB_UserActivity activity = new DB_UserActivity(myUser.getRefNow(), UserActivity.ActivityType.InternalUpdateCalendarEvent, new List<String>() { myUser.EventID });
                            JObject json = JObject.FromObject(myUser);
                            activity.updateMiscelaneousInfo(json.ToString());

                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                            await NewSchedule.UpdateWithDifferentSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                            retValue = new PostBackData(ScheduleUpdateMessage.Item1);
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
    }
}