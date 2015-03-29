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

namespace TilerFront.Controllers
{

    public class CalendarEventController : ApiController
    {
        /*
        private TilerContext db = new TilerContext();

        // GET api/CalendarEvent
        [NonAction]
        public IQueryable<CalEvent> GetCalEvents()
        {
            return db.CalEvents;
        }*/

        // GET api/CalendarEvent/5
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> GetCalEvent(string id,[FromUri]AuthorizedUser myUser )
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();
            await retrievedUser.Login();
            TilerElements.CalendarEvent retrievedCalendarEvent = retrievedUser.ScheduleLogControl.getCalendarEventWithID(id);
            PostBackData retValue = new PostBackData(retrievedCalendarEvent.ToCalEvent(), 0);


            return Ok(retValue.getPostBack);
        }

        // GET api/CalendarEvent/Name
        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Name")]
        public async Task<IHttpActionResult> CalEventName([FromUri]NameSearchModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();
            await retrievedUser.Login();
            string phrase = myUser.Data;

            PostBackData retValue = new PostBackData("", 4);
            if (retrievedUser.Status)
            {
                long myNow =(long) ( DateTimeOffset.Now - WebApiConfig.JSStartTime).TotalMilliseconds;;
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

        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent")]
        public async Task<IHttpActionResult> DeleteCalEvent([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                CustomErrors messageReturned = NewSchedule.deleteCalendarEventAndReadjust(myUser.EventID);
                retValue = new PostBackData(messageReturned, messageReturned.Code);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent")]
        public async Task<IHttpActionResult> HandleOptionsEvent([FromBody]getEventModel myUser)
        {
            return Ok();
        }


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Complete")]
        public async Task<IHttpActionResult> CompleteCalEvent([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();// new UserAccount(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                CustomErrors messageReturned = NewSchedule.markAsCompleteCalendarEventAndReadjust(myUser.EventID);
                retValue = new PostBackData(messageReturned, messageReturned.Code);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Now")]
        public async Task<IHttpActionResult> Now( [FromBody]NowEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            DateTime myDate;
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.SetCalendarEventAsNow(myUser.ID);
                await NewSchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                retValue = new PostBackData(ScheduleUpdateMessage.Item1);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }

        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/CalendarEvent/Update")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]EditCalEventModel myUser)    
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(); //new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                DateTimeOffset newStart = WebApiConfig.JSStartTime.AddMilliseconds( myUser.Start  );
                newStart = newStart.Add(myUser.getTImeSpan);
                DateTimeOffset newEnd = WebApiConfig.JSStartTime.AddMilliseconds( myUser.End  );
                newEnd = newEnd.Add(myUser.getTImeSpan);
                int SplitCount = (int)myUser.Split;
                TimeSpan SpanPerSplit = TimeSpan.FromMilliseconds(myUser.Duration);
                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID, myUser.EventName, newStart, newEnd, SplitCount);//, SpanPerSplit);
                await NewSchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                retValue = new PostBackData(ScheduleUpdateMessage.Item1);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            
            return Ok(retValue.getPostBack);
        }


        /*
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private bool CalEventExists(string id)
        {
            return db.CalEvents.Count(e => e.ID == id) > 0;
        }
        */

    }
}