using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using TilerElements;
using DBTilerElement;

namespace TilerFront.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    /*public class TilerUser //: TilerUser
    {
    }*/

    public class ApplicationDbContext : ScheduleContext//IdentityDbContext<TilerUser>
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