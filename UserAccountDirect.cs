using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using TilerFront.Models;
using System.Threading.Tasks;
using System.Data.Entity;

namespace TilerFront
{
    public class UserAccountDirect:UserAccount
    {
        ApplicationDbContext Database;
        protected UserAccountDirect()
        {
            
        }



        public UserAccountDirect(string userId, ApplicationDbContext database)
        {
            ID = userId;
            Database = database;
        }

        /// <summary>
        /// Essentially initializes the user log and pulls a user's account
        /// </summary>
        /// <returns></returns>
        public override async System.Threading.Tasks.Task<bool> Login()
        {
            HttpContext ctx = HttpContext.Current;
            DBTilerElement.DB_TilerUser tilerUser = new DBTilerElement.DB_TilerUser()
            {
                Id = ID
            };
            UserLog = new LogControlDirect(tilerUser, Database, "");
            if (ctx != null)
            {
                await UserLog.Initialize();
            }

            

            bool retValue = UserLog.Status;
            return retValue;
        }


        override protected Dictionary<string, CalendarEvent> getAllCalendarElements(TimeLine RangeOfLookup, string desiredDirectory = "")
        {
            Dictionary<string, CalendarEvent> retValue = new Dictionary<string, CalendarEvent>();
            retValue = UserLog.getAllCalendarFromXml(RangeOfLookup);
            return retValue;
        }

        async override protected Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {
            DateTimeOffset retValue = await UserLog.getDayReferenceTime(desiredDirectory);
            return retValue;
        }


        override public bool DeleteAllCalendarEvents()
        {
            bool retValue = false;

            if (UserLog.Status)
            {
                UserLog.deleteAllCalendarEvets();
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }


        override public void UpdateReferenceDayTime(DateTimeOffset referenceTime)
        {
            UserLog.UpdateReferenceDayInXMLLog(referenceTime);
        }

        

        #region properties
        public int LastEventTopNodeID
        {
            get
            {
                if (UserLog.Status)
                {
                    return UserLog.LastUserID;
                }
                return 0;
            }
        }


        public bool Status
        {
            get
            {
                bool retValue = (SessionUser != null) && UserLog.Status;
                return retValue;
            }
        }

        public string UserID
        {
            get
            {
                return SessionUser.Id;
            }
        }

        public string UserName
        {
            get
            {
                return Username;
            }
        }

        public string Usersname
        {
            get
            {
                return Name;
            }
        }

        public string getFullLogDir
        {
            get
            {
                return UserLog.getFullLogDir;
            }
        }

        virtual public LogControl ScheduleLogControl
        {
            get
            {
                return UserLog;
            }
        }

        #endregion 

    }
}