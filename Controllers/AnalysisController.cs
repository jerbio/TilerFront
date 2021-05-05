using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TilerFront.Models;
using ScheduleAnalysis;
using TilerElements;
using System.Collections.Concurrent;

namespace TilerFront.Controllers
{
    [Authorize]
    public class AnalysisController : TilerApiController
    {
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Analysis/Suggestion")]
        public async Task<IHttpActionResult> Suggestion([FromBody]AuthorizedUser authorizedUser)
        {
            UserAccount retrievedUser = await authorizedUser.getUserAccount(db).ConfigureAwait(false);
            await retrievedUser.Login().ConfigureAwait(false);
            TilerUser tilerUser = retrievedUser.getTilerUser();
            tilerUser.updateTimeZoneTimeSpan(authorizedUser.getTimeSpan);
            if (retrievedUser.Status)
            {
                DateTimeOffset nowTime = authorizedUser.getRefNow();
                if(retrievedUser.ScheduleLogControl.Now == null)
                {
                    var retrievalOption = DataRetrievalSet.analysisManipulation;
                    DB_Schedule schedule = new DB_Schedule(retrievedUser, nowTime, retrievalOptions: retrievalOption);
                    retrievedUser.ScheduleLogControl.Now = schedule.Now;
                }
                
                await SuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
            }

            var retValue = new PostBackData("", 0);
            return Ok(retValue.getPostBack);
        }


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Analysis/Analyze")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]AuthorizedUser myUser)
        {
            return Ok();
            UserAccount retrievedUser = await myUser.getUserAccount(db); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            retrievedUser.getTilerUser().updateTimeZoneTimeSpan(myUser.getTimeSpan);
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                await SuggestionAnalysis(retrievedUser.ScheduleLogControl).ConfigureAwait(false);
                retValue = new PostBackData(0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            return Ok(retValue.getPostBack);
        }

        public async Task SuggestionAnalysis(LogControl logControl)
        {
            if(logControl.Now== null)
            {
                logControl.Now = new ReferenceNow(DateTimeOffset.UtcNow, logControl.getTilerRetrievedUser().EndfOfDay, logControl.getTilerRetrievedUser().TimeZoneDifference);
            } else
            {
                logControl.Now = new ReferenceNow(logControl.Now.constNow, logControl.getTilerRetrievedUser().EndfOfDay, logControl.getTilerRetrievedUser().TimeZoneDifference);
            }
            DateTimeOffset nowTime = logControl.Now.constNow;
            TimeLine timeline = new TimeLine(logControl.Now.constNow.AddDays(-45), logControl.Now.constNow.AddDays(45));
            List<IndexedThirdPartyAuthentication> AllIndexedThirdParty = await ScheduleController.getAllThirdPartyAuthentication(logControl.LoggedUserID, db).ConfigureAwait(false);
            List<GoogleTilerEventControl> AllGoogleTilerEvents = AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl(obj, db)).ToList();
            var tupleOfSUbEVentsAndAnalysis = logControl.getSubCalendarEventForAnalysis(timeline, logControl.getTilerRetrievedUser());
            List<SubCalendarEvent> subEvents = tupleOfSUbEVentsAndAnalysis.Item1.ToList();
            Analysis analysis = tupleOfSUbEVentsAndAnalysis.Item2;
            Task<ConcurrentBag<CalendarEvent>> GoogleCalEventsTask = GoogleTilerEventControl.getAllCalEvents(AllGoogleTilerEvents, timeline);
            IEnumerable<CalendarEvent> GoogleCalEvents = await GoogleCalEventsTask.ConfigureAwait(false);
            subEvents.AddRange(GoogleCalEvents.SelectMany(o => o.AllSubEvents));
            ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(subEvents, logControl.Now, logControl.getTilerRetrievedUser(), analysis);
            var overoccupiedTimelines = scheduleSuggestion.getOverLoadedWeeklyTimelines(nowTime);
            var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);
            List<CalendarEvent> calEvents = new HashSet<CalendarEvent>(subEvents.Select(o => o.ParentCalendarEvent)).ToList();
            await logControl.Commit(calEvents, null, "0", logControl.Now, null);
        }

        public static async Task updateSuggestionAnalysis(LogControl logcontrol)
        {
            return;
            //await new AnalysisController().SuggestionAnalysis(logcontrol).ConfigureAwait(false);
        }
    }
}
