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
           //get
           {
            DateTimeOffset refNow = DateTimeOffset.UtcNow;
            refNow = refNow.Add(-this.getTImeSpan);
            return refNow;
           }
        }

        async public Task<UserAccountDirect> getUserAccount()
        {
            Controllers.UserController myUserController = new Controllers.UserController();
            ApplicationUser User = await myUserController.GetUser(UserID, UserName);
            return new UserAccountDirect(User);
        }
    }
}