using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using TilerElements;

namespace TilerFront.Models
{
    public class AuthorizedUser : DTOs.VerifiedUser
    {
        public bool MobileFlag { get; set; }
        public int TimeZoneOffset { get; set; }
        /// <summary>
        /// TimeZone desired by the user. This defaults to UTC
        /// </summary>
        public string TimeZone { get; set; } = "UTC";
        protected DateTimeOffset refNow = DateTimeOffset.UtcNow;
        public TimeSpan getTImeSpan
        {
            get 
            {
                int TimeSpanint = TimeZoneOffset;
                TimeSpan TimeZoneSpan = TimeSpan.FromMinutes(TimeSpanint);
                return TimeZoneSpan;
            }
        }

        public DateTimeOffset getRefNow()
        {
            return refNow;
        }

        async public Task<UserAccountDirect> getUserAccountDirect(ApplicationDbContext db)
        {
            return new UserAccountDirect(UserID, db);
        }

        async public Task<UserAccountDebug> getUserAccountDebug(bool Passive = true)
        {
            Controllers.UserController myUserController = new Controllers.UserController();
            TilerUser User;

            if (Passive)
            {
                User = new TilerUser() { UserName = UserName, Id = UserID, FullName = "" };
            }
            else
            {
                User = await myUserController.GetUser(UserID, UserName);
            }
            return new UserAccountDebug(User);
        }
    }
}