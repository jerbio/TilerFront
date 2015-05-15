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
//using TilerGoogleCalendarLib;


namespace TilerFront.Controllers
{
    public class SubCalendarEventController : ApiController
    {
        /*
        private TilerFrontContext db = new TilerFrontContext();

        
        // GET api/SubCalendarEvent
        public IQueryable<SubCalEvent> GetSubCalEvents()
        {
            return db.SubCalEvents;
        }

        

        // GET api/SubCalendarEvent/5
        [ResponseType(typeof(SubCalEvent))]
        public async Task<IHttpActionResult> GetSubCalEvent(string id)
        {
            SubCalEvent subcalevent = await db.SubCalEvents.FindAsync(id);
            if (subcalevent == null)
            {
                return NotFound();
            }

            return Ok(subcalevent);
        }

        // PUT api/SubCalendarEvent/5
        public async Task<IHttpActionResult> PutSubCalEvent(string id, SubCalEvent subcalevent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != subcalevent.ID)
            {
                return BadRequest();
            }

            db.Entry(subcalevent).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubCalEventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/SubCalendarEvent
        [ResponseType(typeof(SubCalEvent))]
        public async Task<IHttpActionResult> PostSubCalEvent(SubCalEvent subcalevent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.SubCalEvents.Add(subcalevent);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SubCalEventExists(subcalevent.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = subcalevent.ID }, subcalevent);
        }

        // DELETE api/SubCalendarEvent/5
        [ResponseType(typeof(SubCalEvent))]
        public async Task<IHttpActionResult> DeleteSubCalEvent(string id)
        {
            SubCalEvent subcalevent = await db.SubCalEvents.FindAsync(id);
            if (subcalevent == null)
            {
                return NotFound();
            }

            db.SubCalEvents.Remove(subcalevent);
            await db.SaveChangesAsync();

            return Ok(subcalevent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SubCalEventExists(string id)
        {
            return db.SubCalEvents.Count(e => e.ID == id) > 0;
        }
        */


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/SubCalendarEvent/Update")]
        public async Task<IHttpActionResult> UpdateCalEvent([FromBody]EditSubCalEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect(); //new UserAccountDirect(myUser.UserName, myUser.UserID);
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
                            
                            My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myNow);

                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await ScheduleController.getThirdPartyAuthentication(retrievedUser.UserID, myUser.ThirdPartyUserID, 2);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty);
                            await googleControl.updateSubEvent(myUser).ConfigureAwait(false);
                            Dictionary<string, CalendarEvent> AllCalendarEvents = (await googleControl.getCalendarEvents().ConfigureAwait(false)).ToDictionary(obj => obj.ID, obj => obj);
                            GoogleThirdPartyControl googleEvents = new GoogleThirdPartyControl(AllCalendarEvents);
                            await NewSchedule.updateDataSetWithThirdPartyDataAndTriggerNewAddition(googleEvents).ConfigureAwait(false);

                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            DateTimeOffset myNow = myUser.getRefNow();
                            myNow = DateTimeOffset.UtcNow;
                            My24HourTimerWPF.Schedule NewSchedule = new My24HourTimerWPF.Schedule(retrievedUser, myNow);

                            await ScheduleController.updatemyScheduleWithGoogleThirdpartyCalendar(NewSchedule, retrievedUser.UserID).ConfigureAwait(false);

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
                            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = NewSchedule.BundleChangeUpdate(myUser.EventID, myUser.EventName, newStart, newEnd, Begin, Deadline, SplitCount);//, SpanPerSplit);
                            await NewSchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2).ConfigureAwait(false);
                            retValue = new PostBackData(ScheduleUpdateMessage.Item1);
                        }
                        break;
                    default:
                        break;
                }
            }

            return Ok(retValue.getPostBack);
        }
    }
}