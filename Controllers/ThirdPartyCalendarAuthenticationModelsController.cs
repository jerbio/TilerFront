using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TilerFront.Models;

namespace TilerFront.Controllers
{
    public class ThirdPartyCalendarAuthenticationModelsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ThirdPartyCalendarAuthenticationModels
        public async Task<ActionResult> Index()
        {
            return View(await db.ThirdPartyAuthentication.ToListAsync());
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TilerID,Email,ProviderID,ID,isLongLived,Token,RefreshToken,Deadline")] ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel)
        {
            if (ModelState.IsValid)
            {
                db.ThirdPartyAuthentication.Add(thirdPartyCalendarAuthenticationModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TilerID,Email,ProviderID,ID,isLongLived,Token,RefreshToken,Deadline")] ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thirdPartyCalendarAuthenticationModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            db.ThirdPartyAuthentication.Remove(thirdPartyCalendarAuthenticationModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Authenticate(string TilerID, string Email, string Provider)
        {
            Object[] ParamS = { TilerID, Email, Convert.ToInt32( Provider )};
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(ParamS);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
