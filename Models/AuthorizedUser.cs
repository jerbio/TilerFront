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
        /// Longitude of user
        /// </summary>
        public string UserLongitude { get; set; }
        /// <summary>
        /// Latitude of user
        /// </summary>
        public string UserLatitude { get; set; }
        /// <summary>
        /// IsLocationVerified
        /// </summary>
        public string UserLocationVerified { get; set; }
        /// <summary>
        /// TimeZone desired by the user. This defaults to UTC
        /// </summary>
        public string TimeZone { get; set; } = "UTC";
        protected DateTimeOffset refNow = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
        public TimeSpan getTimeSpan
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
            //string timeInString = "11/25/2019 4:50:00 PM";
            string timeInString = "";

            if (string.IsNullOrEmpty(timeInString))
            {
                return refNow;
            }
            else
            {
                DateTimeOffset retValue = DateTimeOffset.Parse(timeInString);
                return retValue;
            }
            
        }

        async virtual public Task<UserAccount> getUserAccount(TilerDbContext db=null)
        {
            if (db != null)
            {
                return new UserAccountDirect(UserID, db);
            }
            else
            {
                TilerUser User = new TilerUser() { UserName = UserName, Id = UserID};
                return new UserAccountXml(User);
            }
            
        }

        public Location getCurrentLocation ()
        {
            bool locationIsVerified = false;
            Location retValue = null;
            double longitude = 0;
            double latitude = 0;
            if (!string.IsNullOrEmpty(UserLocationVerified) && !string.IsNullOrWhiteSpace(UserLocationVerified)
                &&!string.IsNullOrEmpty(UserLongitude) && !string.IsNullOrWhiteSpace(UserLongitude)
                && !string.IsNullOrEmpty(UserLatitude) && !string.IsNullOrWhiteSpace(UserLatitude)
                )
            {
                longitude = Convert.ToDouble(UserLongitude);
                latitude = Convert.ToDouble(UserLatitude);
                locationIsVerified = Convert.ToBoolean(UserLocationVerified);
            }

            if (locationIsVerified)
            {
                retValue = new Location(latitude, longitude);
            }
            else
            {
                retValue = Location.getDefaultLocation();
            }

            return retValue;
        }

        //async public Task<UserAccountDebug> getUserAccountDebug(bool Passive = true)
        //{
        //    Controllers.UserController myUserController = new Controllers.UserController();
        //    TilerUser User;
        //    ApplicationDbContext db = new ApplicationDbContext();
        //    if (Passive)
        //    {
        //        User = new TilerUser() { UserName = UserName, Id = UserID, FullName = "" };
        //    }
        //    else
        //    {
        //        User = await myUserController.GetUser(UserID, UserName);
        //    }
        //    return new UserAccountDebug(User);
        //}
    }
}