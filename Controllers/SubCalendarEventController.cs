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
                            DateTimeOffset myNow = myUser.getRefNow();
                            myNow = DateTimeOffset.UtcNow;

                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myNow);

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
                            myNow = DateTimeOffset.UtcNow;
                            DB_Schedule NewSchedule = new DB_Schedule(retrievedUser, myNow);

                            await ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(NewSchedule, retrievedUser.UserID, db).ConfigureAwait(false);

                            long StartLong = Convert.ToInt64(myUser.Start);
                            long EndLong = Convert.ToInt64(myUser.End);
                            long LongBegin = Convert.ToInt64(myUser.CalStart);
                            long LongDeadline = Convert.ToInt64(myUser.CalEnd);
                            DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(StartLong);
                            newStart = newStart.Add(myUser.getTImeSpan);
                            DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(EndLong);
                            newEnd = newEnd.Add(myUser.getTImeSpan);
                            DateTimeOffset Begin = TilerElementExtension.JSStartTime.AddMilliseconds(LongBegin);
                            Begin = Begin.Add(myUser.getTImeSpan);
                            DateTimeOffset Deadline = TilerElementExtension.JSStartTime.AddMilliseconds(LongDeadline);
                            Deadline = Deadline.Add(myUser.getTImeSpan);
                            int SplitCount = (int)myUser.Split;
                            //TimeSpan SpanPerSplit = TimeSpan.FromMilliseconds(myUser.Duration);
                            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID,  new EventName(myUser.EventName), newStart, newEnd, Begin, Deadline, SplitCount);//, SpanPerSplit);
                            DB_UserActivity activity = new DB_UserActivity(myNow, UserActivity.ActivityType.InternalUpdate, new List<String>() { myUser.EventID });
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            await NewSchedule.UpdateWithDifferentSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                            SubCalendarEvent subEvent = NewSchedule.getSubCalendarEvent(myUser.EventID);
                            CalendarEvent calendarEvent = NewSchedule.getCalendarEvent(myUser.EventID);
                            SubCalEvent subCalEvent =  subEvent.ToSubCalEvent(calendarEvent);
                            JObject retSubEvent = new JObject();
                            retSubEvent.Add("subEvent", JObject.FromObject(subCalEvent));
                            retValue = new PostBackData(retSubEvent, 0);
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