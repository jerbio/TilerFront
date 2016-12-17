using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using TilerElements;

namespace TilerFront.Models
{
    // You can add profile data for the user by adding more properties to your TilerUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    //public class TilerUser : IdentityUser
    //{
    //    public string FullName { get; set; }
    //    public DateTime LastChange { get; set; }
    //    public string UserName { get; set; }
    //    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<TilerUser> manager)
    //    {
    //        Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType


    //       var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
    //        Add custom user claims here



    //        return userIdentity;
    //    }
    //}

    public class ApplicationDbContext : IdentityDbContext<TilerUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public System.Data.Entity.DbSet<ThirdPartyCalendarAuthenticationModel> ThirdPartyAuthentication { get; set; }
        public System.Data.Entity.DbSet<GoogleNotificationWatchResponseModel> GoogleNotificationCredentials { get; set; }
        public System.Data.Entity.DbSet<PausedEvent> PausedEvents { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}