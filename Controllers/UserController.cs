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
using DBTilerElement;
using TilerElements;
using TilerElements.Wpf;
using TilerElements.DB;
using DBTilerElement;
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
        public IQueryable<TilerUser> GetUsers()
        {   
            return db.Users;
        }


        async public Task<TilerUser> GetUser(string ID,string userName)
        {
            List<TilerUser> AllUsers = await db.Users.Where(obj => obj.Id == ID).ToListAsync();

            TilerUser user = null;
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


        public async Task SaveUser(TilerUser user)
        {
            
            var store = new UserStore<TilerUser>(db);

            var manager = new ApplicationUserManager(store);
            await manager.UpdateAsync(user);
            var ctx = store.Context;
            await ctx.SaveChangesAsync();
        }

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
                IEnumerable<Location_Elements> retrievedCalendarEvents = await retrievedUser.Location.getCachedLocationByName(SearchData.Data);
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