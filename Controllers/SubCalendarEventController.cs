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
using DBTilerElement;
using TilerFront.Models;
using TilerCore;
using Newtonsoft.Json.Linq;

namespace TilerFront.Controllers
{
    public class SubCalendarEventController : TilerApiController
    {
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/SubCalendarEvent/Update")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]EditSubCalEventModel myUser)
        {
            UserAccount retrievedUser = await myUser.getUserAccount(db);
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
                            DateTimeOffset myNow = myUser.getRefNow();

                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myNow);
                            NewSchedule.CurrentLocation = myUser.getCurrentLocation();
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await ScheduleController.getThirdPartyAuthentication(retrievedUser.UserID, myUser.ThirdPartyUserID, "Google", db);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                            await googleControl.updateSubEvent(myUser).ConfigureAwait(false);
                            Dictionary<string, CalendarEvent> AllCalendarEvents = (await googleControl.getCalendarEvents(null, true).ConfigureAwait(false)).ToDictionary(obj => obj.getId, obj => obj);
                            GoogleThirdPartyControl googleEvents = new GoogleThirdPartyControl(AllCalendarEvents, AllIndexedThirdParty.getTilerUser());
                            DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.ThirdPartyUpdate);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);

                            await NewSchedule.updateDataSetWithThirdPartyDataAndTriggerNewAddition(new Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent> { googleEvents.getThirdpartyCalendarEvent() })).ConfigureAwait(false);

                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            DateTimeOffset myNow = myUser.getRefNow();
                            myNow = myUser.getRefNow();
                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myNow);
                            NewSchedule.CurrentLocation = myUser.getCurrentLocation();
                            await ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(NewSchedule, retrievedUser.UserID, db).ConfigureAwait(false);

                            long StartLong = Convert.ToInt64(myUser.Start);
                            long EndLong = Convert.ToInt64(myUser.End);
                            long LongBegin = Convert.ToInt64(myUser.CalStart);
                            long LongDeadline = Convert.ToInt64(myUser.CalEnd);
                            DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(StartLong);
                            newStart = newStart.Add(myUser.getTimeSpan);
                            DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(EndLong);
                            newEnd = newEnd.Add(myUser.getTimeSpan);
                            DateTimeOffset Begin = TilerElementExtension.JSStartTime.AddMilliseconds(LongBegin);
                            Begin = Begin.Add(myUser.getTimeSpan);
                            if(LongBegin == 0)
                            {
                                Begin = Utility.BeginningOfTime;
                            }

                            DateTimeOffset Deadline = TilerElementExtension.JSStartTime.AddMilliseconds(LongDeadline);
                            Deadline = Deadline.Add(myUser.getTimeSpan);
                            if (LongDeadline == 0)
                            {
                                Deadline = Utility.BeginningOfTime;
                            }
                            int SplitCount = (int)myUser.Split;
                            if(SplitCount >= 1)
                            {
                                SubCalendarEvent subEventedited = NewSchedule.getSubCalendarEvent(myUser.EventID);
                                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID, new EventName(retrievedUser.getTilerUser(), subEventedited, myUser.EventName), newStart, newEnd, Begin, Deadline, SplitCount, myUser.EscapedNotes);
                                DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.InternalUpdate, new List<String>() { myUser.EventID });
                                retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                                await NewSchedule.persistToDB().ConfigureAwait(false);
                                EventID eventId = new EventID(myUser.EventID);
                                CalendarEvent calendarEvent = await retrievedUser.ScheduleLogControl.getCalendarEventWithID(eventId).ConfigureAwait(false);
                                SubCalendarEvent subEvent = calendarEvent.ActiveSubEvents.FirstOrDefault();
                                SubCalEvent subCalEvent = subEvent.ToSubCalEvent(calendarEvent);
                                JObject retSubEvent = new JObject();
                                retSubEvent.Add("subEvent", JObject.FromObject(subCalEvent));
                                retValue = new PostBackData(retSubEvent, 0);
                            } else
                            {
                                CustomErrors error = new CustomErrors(CustomErrors.Errors.TilerConfig_Zero_SplitCount);
                                retValue = new PostBackData(error);
                            }
                            
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