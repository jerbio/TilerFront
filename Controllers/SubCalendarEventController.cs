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
using TilerFront.Models;

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
    }
}