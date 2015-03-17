using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Configuration;
using System.Web.Http.Description;
using TilerFront.Models;
//using System.Web.Http.Cors;

namespace TilerFront.Controllers
{
    //[EnableCors(origins: "*", headers: "accept, authorization, origin", methods: "DELETE,PUT,POST,GET")]
    public class UserController : ApiController
    {
        //private TilerFrontContext db = new TilerFrontContext();
        private ApplicationDbContext db = new ApplicationDbContext();
        
        // GET api/User
        [NonAction]
        public IQueryable<ApplicationUser> GetUsers()
        {   
            return db.Users;
        }
        /*
        // GET api/User/5
        [NonAction]
        [ResponseType(typeof(ApplicationUser))]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            ApplicationUser user = await db.Users.SingleAsync(obj => obj.UserID == id); //.Asy(id); //await db.Users.Where(obj=>obj.UserID.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        */


        async public Task<ApplicationUser> GetUser(string ID,string userName)
        {
            List<ApplicationUser> AllUsers = await db.Users.Where(obj => obj.Id == ID).ToListAsync();

            ApplicationUser user = null;
            if (AllUsers.Count > 0)
            {
                if (user != null)
                {
                    if (user.UserName.ToLower() != userName.ToLower())
                    {
                        user = null;
                    }
                }
                user = AllUsers[0];
            }
            return user;
        }


        public async Task SaveUser(ApplicationUser user)
        {
            
            var store = new UserStore<ApplicationUser>(db);

            var manager = new ApplicationUserManager(store);
            await manager.UpdateAsync(user);
            var ctx = store.Context;
            await ctx.SaveChangesAsync();
        }

        /*
        [NonAction]
        // POST api/User
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = user.UserID }, user);
        }
        
        [NonAction]
        // DELETE api/User/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> DeleteUser(int id)
        {
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return Ok(user);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserID == id) > 0;
        }
        
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/User/SignIn")]
        public async Task<IHttpActionResult> SignIn([FromBody]UnAuthorizedUser unAuthUser )
        {
            UserAccount myUser = new UserAccount(unAuthUser.UserName, unAuthUser.Password);
            bool LogInSuccess = await myUser.Login();
            bool sendMobileSignal = IsMobileDevice(Request.Headers.UserAgent.ToString());
            DTOs.VerifiedUser myAuthorizedUser = new AuthorizedUser { UserID = 0, UserName = "", MobileFlag = sendMobileSignal } as DTOs.VerifiedUser;
            PostBackData retData;
            if (LogInSuccess)
            {
                myAuthorizedUser = (DTOs.VerifiedUser)new AuthorizedUser { UserID = myUser.UserID, UserName = myUser.UserName, MobileFlag = sendMobileSignal };
                retData = new PostBackData(myAuthorizedUser as DTOs.VerifiedUser, 0);
            }
            else
            {
                retData = new PostBackData("", 1);

            }

            
            PostBackStruct retValue = retData.getPostBack;
            //return (IHttpActionResult)retValue;
            return Ok(retValue);
        }

        */

        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/User/Location")]
        public async Task<IHttpActionResult> Location([FromUri]NameSearchModel SearchData)
        {
            UserAccountDirect retrievedUser = await SearchData.getUserAccountDirect();
            await retrievedUser.Login();

            PostBackData retValue = new PostBackData("", 4);
            if (retrievedUser.Status)
            {
                IEnumerable<TilerElements.Location_Elements> retrievedCalendarEvents = await retrievedUser.ScheduleLogControl.getCachedLocationByName(SearchData.Data);
                retValue = new PostBackData(retrievedCalendarEvents.Select(obj => obj.ToLocationModel()).ToList(), 0);
            }

            return Ok(retValue.getPostBack);
        }
        

        private static string[] mobileDevices = new string[] {"iphone","ppc","android",
                                                      "windows ce","blackberry",
                                                      "opera mini","mobile","android","windows phone","palm",
                                                      "portable","opera mobi" };

        public static bool IsMobileDevice(string userAgent)
        {
            // TODO: null check
            userAgent = userAgent.ToLower();
            return mobileDevices.Any(x => userAgent.Contains(x));
        }
    }
}