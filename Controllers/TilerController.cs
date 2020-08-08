using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using TilerFront.Models;
using TilerElements;
using System.Data.Entity.Validation;
using System.Net;
using System.Data.Entity;

namespace TilerFront
{
    [Authorize]
    /// <summary>
    /// Tiler controller that provides custom tiler functionality
    /// </summary>
    public class TilerController : Controller
    {
        protected ApplicationDbContext dbContext;
        protected TilerController(): base ()
        {
            dbContext = new ApplicationDbContext();
            dbContext.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
        
        protected async Task dbSaveChangesAsync()
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Had issues sending updating your request\n" + e.ToString());
            }
        }

        /// <summary>
        /// This is to be called when you need save the last change to the database
        /// </summary>
        /// <param name="db">The database context for accessing db</param>
        /// <param name="user">The referemce tiler user</param>
        /// <returns></returns>
        public static async Task saveLatestChange(TilerDbContext db, TilerUser user)
        {
            TilerUser retrievedUser = await ((db.Users) as DbSet<TilerUser>) .FindAsync(user.Id).ConfigureAwait(false);
            if(user.ClearAllId != retrievedUser.ClearAllId)
            {
                retrievedUser.ClearAllId = user.ClearAllId;
            }
            retrievedUser.LastScheduleModification = DateTime.UtcNow;
            Task waitForDbSave = null;
            if (!string.IsNullOrEmpty(retrievedUser.PasswordHash))
            {
                EntityState currentState = db.Entry(retrievedUser).State;
                if (currentState == EntityState.Detached)
                {
                    db.Entry(retrievedUser).State = EntityState.Added;
                }
                db.Entry(retrievedUser).State = EntityState.Modified;
                TilerController.dbSaveChanges(db as ApplicationDbContext);
            }

            if(waitForDbSave != null)
            {
                await waitForDbSave;
            }
        }

        public DbContext DbContext
        {
            get
            {
                return dbContext;
            }
        }

        static public async Task dbSaveChangesAsync(ApplicationDbContext db)
        {
            try
            {
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Had issues sending updating your request\n" + e.ToString());
            }
        }

        static public void dbSaveChanges(ApplicationDbContext db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "Had issues sending updating your request\n" + e.ToString());
            }
        }
    }
}
