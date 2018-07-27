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
    public class ApplicationDbContext : TilerDbContext
    {
        public ApplicationDbContext(string connectionName = "DefaultConnection") : base(connectionName)
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