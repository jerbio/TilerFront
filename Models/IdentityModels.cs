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
        public System.Data.Entity.DbSet<TilerEvent> events { get; set; }
        public System.Data.Entity.DbSet<SubCalendarEvent> SubEvents { get; set; }
        public System.Data.Entity.DbSet<CalendarEvent> CalEvents { get; set; }
        public System.Data.Entity.DbSet<Repetition> Repetitions { get; set; }
        public System.Data.Entity.DbSet<RestrictionProfile> Restrictions { get; set; }
        public System.Data.Entity.DbSet<Location> Locations { get; set; }
        public System.Data.Entity.DbSet<EventDisplay> UiParams { get; set; }
        public System.Data.Entity.DbSet<MiscData> MiscData { get; set; }
        public System.Data.Entity.DbSet<RestrictionDay> RestrictionDays { get; set; }
        public System.Data.Entity.DbSet<TilerColor> UserColors { get; set; }
        public System.Data.Entity.DbSet<Undo> undos { get; set; }
        public System.Data.Entity.DbSet<CalendarEventRestricted> RestrictedCalEvents { get; set; }
        public System.Data.Entity.DbSet<SubCalendarEventRestricted> RestrictedSubCalEvents { get; set; }
        public System.Data.Entity.DbSet<RigidCalendarEvent> RigidCalEvents { get; set; }
        public System.Data.Entity.DbSet<BusyTimeLine> BusyTimelines { get; set; }
        public System.Data.Entity.DbSet<EventName> EventNames { get; set; }
        public System.Data.Entity.DbSet<EventTimeLine> EventTimeLines { get; set; }
        public System.Data.Entity.DbSet<Classification> EventType { get; set; }
        public System.Data.Entity.DbSet<GoogleTilerUser> Googleusers { get; set; }
        public System.Data.Entity.DbSet<NowProfile> NowProfiles { get; set; }
        public System.Data.Entity.DbSet<ProcrastinateCalendarEvent> ProcrastinteAlls { get; set; }
        public System.Data.Entity.DbSet<Procrastination> Procrastinations { get; set; }
        public System.Data.Entity.DbSet<Reason> Reasons { get; set; }
        public System.Data.Entity.DbSet<RestrictionTimeLine> RestrictionTimeLines { get; set; }
        public System.Data.Entity.DbSet<ThirdPartyTilerUser> ThirdPartyTilerUsers { get; set; }
        public System.Data.Entity.DbSet<TilerUserGroup> TilerUserGroups { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}