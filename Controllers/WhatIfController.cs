using DBTilerElement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using TilerElements;
using ScheduleAnalysis;
using TilerFront.Models;

namespace TilerFront.Controllers
{
    public class WhatIfController : TilerApiController
    {
        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/PushedAll")]
        public async Task<IHttpActionResult> pushed([FromBody]WhatIfModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);
            PostBackData returnPostBack;
            if (retrievedUser.Status)
            {
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myAuthorizedUser.getRefNow());
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                Tuple<Health, Health> evaluation;
                evaluation = await schedule.TimeStone.PushedAll(UserData.Duration, null);

                JObject before = evaluation.Item1.ToJson();
                JObject after = evaluation.Item2.ToJson();
                JObject resultData = new JObject();
                resultData.Add("before", before);
                resultData.Add("after", after);
                returnPostBack = new PostBackData(resultData, 0);
                return Ok(returnPostBack.getPostBack);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }

        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/PushedEvent")]
        public async Task<IHttpActionResult> pushedEvent([FromBody]WhatIfModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccount retrievedUser = await UserData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(UserData.getTimeSpan);
            PostBackData returnPostBack;
            if (retrievedUser.Status)
            {
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myAuthorizedUser.getRefNow());
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();
                Tuple<Health, Health> evaluation;

                SubCalendarEvent subEvent = null;
                try
                {
                    if (String.IsNullOrEmpty(UserData.EventId))
                    {
                        var calEventAndSubEvent = schedule.getNearestEventToNow();
                        subEvent = calEventAndSubEvent.Item2;
                        EventID eventId = new EventID(calEventAndSubEvent.Item2.getId);
                        evaluation = await schedule.TimeStone.PushSingleEvent(UserData.Duration, eventId, null);
                    }
                    else
                    {
                        evaluation = await schedule.TimeStone.PushSingleEvent(UserData.Duration, new EventID(UserData.EventId), null);
                    }

                    JObject before = evaluation.Item1.ToJson();
                    JObject after = evaluation.Item2.ToJson();
                    JObject resultData = new JObject();
                    if (subEvent != null)
                    {
                        SubCalEvent subcalevent = subEvent.ToSubCalEvent();
                        JObject jsonSubcal = JObject.FromObject(subcalevent);
                        resultData.Add("subEvent", jsonSubcal);
                    }

                    resultData.Add("before", before);
                    resultData.Add("after", after);
                    returnPostBack = new PostBackData(resultData, 0);
                    return Ok(returnPostBack.getPostBack);
                }
                catch (CustomErrors error)
                {
                    returnPostBack = new PostBackData(error);
                    return Ok(returnPostBack.getPostBack);
                }
                
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }

        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/SetAsNow")]
        public async Task<IHttpActionResult> SetAsNow([FromBody]WhatIfModel SetAsNowData)
        {
            AuthorizedUser myAuthorizedUser = SetAsNowData.User;
            UserAccount retrievedUser = await SetAsNowData.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(SetAsNowData.getTimeSpan);
            PostBackData returnPostBack;
            if (retrievedUser.Status)
            {
                Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                DB_Schedule schedule = new DB_Schedule(retrievedUser, myAuthorizedUser.getRefNow(), includeUpdateHistory: true);
                var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                schedule.CurrentLocation = myAuthorizedUser.getCurrentLocation();

                var evaluation = await schedule.TimeStone.WhatIfSetAsNow(SetAsNowData.EventId);
                JObject before = evaluation.Item1.ToJson();
                JObject after = evaluation.Item2.ToJson();
                JObject resultData = new JObject();
                resultData.Add("before", before);
                resultData.Add("after", after);
                returnPostBack = new PostBackData(resultData, 0);
                return Ok(returnPostBack.getPostBack);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }


        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/SubeventEdit")]
        public async Task<IHttpActionResult> SubeventEdit([FromBody]EditSubEventWhatIfModel SubEventEdit)
        {
            UserAccount retrievedUser = await SubEventEdit.getUserAccount(db);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(SubEventEdit.getTimeSpan);
            PostBackData returnPostBack;
            if (retrievedUser.Status)
            {
                int SplitCount = (int)SubEventEdit.Split;
                long StartLong = Convert.ToInt64(SubEventEdit.Start);
                long EndLong = Convert.ToInt64(SubEventEdit.End);
                long LongBegin = Convert.ToInt64(SubEventEdit.CalStart);
                long LongDeadline = Convert.ToInt64(SubEventEdit.CalEnd);
                DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(StartLong);
                DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(EndLong);
                DateTimeOffset Begin = TilerElementExtension.JSStartTime.AddMilliseconds(LongBegin);
                if (LongBegin == 0)
                {
                    Begin = Utility.BeginningOfTime;
                }

                DateTimeOffset Deadline = TilerElementExtension.JSStartTime.AddMilliseconds(LongDeadline);
                if (LongDeadline == 0)
                {
                    Deadline = Utility.BeginningOfTime;
                }


                DateTimeOffset now = SubEventEdit.getRefNow();
                DB_Schedule schedule;
                Tuple<Health, Health> evaluation;

                string CalendarType = SubEventEdit.ThirdPartyType.ToLower();
                switch (CalendarType)
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await ScheduleController.getThirdPartyAuthentication(retrievedUser.UserID, SubEventEdit.ThirdPartyUserID, "Google", db);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty, db);
                            await googleControl.updateSubEvent(SubEventEdit).ConfigureAwait(false);
                            Dictionary<string, CalendarEvent> AllCalendarEvents = (await googleControl.getCalendarEvents(null, true).ConfigureAwait(false)).ToDictionary(obj => obj.getId, obj => obj);
                            GoogleThirdPartyControl googleEvents = new GoogleThirdPartyControl(AllCalendarEvents, AllIndexedThirdParty.getTilerUser());
                            DB_UserActivity activity = new DB_UserActivity(now, UserActivity.ActivityType.ThirdPartyUpdate);
                            retrievedUser.ScheduleLogControl.updateUserActivty(activity);
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            schedule = new DB_Schedule(retrievedUser, now);
                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                            evaluation = await schedule.TimeStone.EventUpdate().ConfigureAwait(false);
                        }
                        break;
                    case "tiler":
                        {
                            HashSet<string> calendarIds = new HashSet<string>() { SubEventEdit.EventID };
                            Task<Tuple<ThirdPartyControl.CalendarTool, IEnumerable<CalendarEvent>>> thirdPartyDataTask = ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(retrievedUser.UserID, db);
                            schedule = new DB_Schedule(retrievedUser, now, calendarIds: calendarIds, includeUpdateHistory: true);
                            var thirdPartyData = await thirdPartyDataTask.ConfigureAwait(false);
                            schedule.updateDataSetWithThirdPartyData(thirdPartyData);
                            schedule.CurrentLocation = SubEventEdit.getCurrentLocation();
                            evaluation = await schedule.TimeStone.EventUpdate(
                                newStart,
                                newEnd,
                                Begin,
                                Deadline,
                                SplitCount,
                                SubEventEdit.EventID
                                ).ConfigureAwait(false);
                        }
                        break;
                    default:
                        CustomErrors error = new CustomErrors(CustomErrors.Errors.Preview_Calendar_Type_Not_Supported);
                        returnPostBack = new PostBackData(error);
                        return Ok(returnPostBack.getPostBack);
                }




                

                
                JObject before = evaluation.Item1.ToJson();
                JObject after = evaluation.Item2.ToJson();

                HealthComparison healthComparison = new HealthComparison(evaluation.Item1, evaluation.Item2);
                var delta = healthComparison.delta();
                JObject deltaJson = new JObject();
                deltaJson.Add("conflictDeltaCount", delta.Item1);
                deltaJson.Add("tardyDeltaCount", delta.Item2);

                JObject resultData = new JObject();
                resultData.Add("before", before);
                resultData.Add("after", after);
                resultData.Add("delta", deltaJson);
                returnPostBack = new PostBackData(resultData, 0);
                return Ok(returnPostBack.getPostBack);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }
    }
}