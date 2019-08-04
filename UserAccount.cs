
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using TilerElements;
using TilerFront.Models;




namespace TilerFront
{
    public abstract class UserAccount
    {
        protected LogControl UserLog;
        protected string ID="";
        protected string Name;
        protected string Username;
        string Password;
        protected TilerDbContext _Database;

        public UserAccount()
        {
            Username = "";
            Password = "";
        }

        public abstract Task<bool> Login();

        /// <summary>
        /// Gets the tilerUser account associated with the userAccount
        /// </summary>
        /// <returns></returns>
        public virtual TilerUser getTilerUser()
        {
            return SessionUser;// if this is null check if you made call to login()
        }

        protected TilerUser SessionUser
        {
            get
            {
                return UserLog.getTilerRetrievedUser();
            }
        }


        virtual protected async Task<Dictionary<string, CalendarEvent>>  getAllCalendarElements(TimeLine RangeOfLookup, ReferenceNow now)
        {
            Dictionary<string, CalendarEvent> retValue=new Dictionary<string,CalendarEvent>();
            retValue = await UserLog.getAllEnabledCalendar(RangeOfLookup, now).ConfigureAwait(false);
            return retValue;
        }

        virtual async protected Task<DateTimeOffset> getDayReferenceTime()
        {
            DateTimeOffset retValue = UserLog.getDayReferenceTime();
            return retValue;
        }

        virtual public bool DeleteAllCalendarEvents()
        {
            bool retValue = false;
            
            if (UserLog.Status)
            {
                UserLog.deleteAllCalendarEvents();
                retValue = true;
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }

        virtual public void UpdateReferenceDayTime(DateTimeOffset referenceTime)
        {
            UserLog.UpdateReferenceDayInXMLLog(referenceTime);
        }

        virtual async public Task Commit(IEnumerable<CalendarEvent> AllEvents, CalendarEvent calendarEvent, String LatestID, ReferenceNow now)
        {
            await UserLog.Commit(AllEvents, calendarEvent, LatestID, now).ConfigureAwait(false);
        }

        virtual async public Task DiscardChanges()
        {
            await UserLog.DiscardChanges().ConfigureAwait(false);
        }

        virtual public LogControl ScheduleLogControl
        {
            get
            {
                return UserLog;
            }
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
                return UserLog.Status;
            }
        }

        public string UserID
        {
            get 
            {
                return ID;
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

        virtual public LogControl ScheduleData
        {
            get
            {
                return UserLog;
            }
        }

        virtual protected TilerDbContext Database
        {
            get
            {
                return _Database;
            }
        }

        public ReferenceNow Now
        {
            get
            {
                return UserLog.Now;
            }
            set
            {
                UserLog.Now = value;
            }
        }

        #endregion

    }
}
