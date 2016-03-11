using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;


namespace TilerFront.Models
{
    public class AuthorizedUser : DTOs.VerifiedUser
    {
        public bool MobileFlag { get; set; }
        public int TimeZoneOffset { get; set; }
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

        async public Task<UserAccountDirect> getUserAccountDirect(bool Passive=true)
        {
            Controllers.UserController myUserController = new Controllers.UserController();
            ApplicationUser User ;

            if (Passive)
            {
                User = new ApplicationUser() { UserName = UserName, Id = UserID,FullName="" };
            }
            else
            {
                User = await myUserController.GetUser(UserID, UserName);
            }
            return new UserAccountDirect(User, Passive);
        }

        async public Task<UserAccountDebug> getUserAccountDebug(bool Passive = true)
        {
            Controllers.UserController myUserController = new Controllers.UserController();
            ApplicationUser User;

            if (Passive)
            {
                User = new ApplicationUser() { UserName = UserName, Id = UserID, FullName = "" };
            }
            else
            {
                User = await myUserController.GetUser(UserID, UserName);
            }
            return new UserAccountDebug(User, Passive);
        }
    }
}