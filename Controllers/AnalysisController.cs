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

namespace TilerFront.Controllers
{
    public class AnalysisController : TilerApiController
    {
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Analysis/Suggestion")]
        public async Task<IHttpActionResult> Suggestion([FromBody]AuthorizedUser authorizedUser)
        {
            UserAccount retrievedUser = await authorizedUser.getUserAccount(db).ConfigureAwait(false);
            await retrievedUser.Login().ConfigureAwait(false);
            
            if(retrievedUser.Status)
            {
                DateTimeOffset nowTime = authorizedUser.getRefNow();
                DB_Schedule schedule = new DB_Schedule(retrievedUser, nowTime, includeUpdateHistory: true);
                //List<SubCalendarEvent> subEVents = (await retrievedUser.ScheduleLogControl.getAllSubCalendarEvents(null, now, true).ConfigureAwait(false)).ToList();
                List<SubCalendarEvent> subEVents = schedule.getAllActiveSubEvents().ToList();
                ScheduleSuggestionsAnalysis scheduleSuggestion = new ScheduleSuggestionsAnalysis(subEVents, schedule.Now, retrievedUser.getTilerUser());
                var overoccupiedTimelines = scheduleSuggestion.temp_fix(nowTime);
                var suggestion = scheduleSuggestion.suggestScheduleChange(overoccupiedTimelines);
            }

            
            var retValue = new PostBackData("", 0);

            return Ok(retValue.getPostBack);
        }
    }
}
