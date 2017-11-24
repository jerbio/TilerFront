
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
            return SessionUser;
        }

        protected TilerUser SessionUser
        {
            get
            {
                return UserLog.getTilerRetrievedUser();
            }
        }


        virtual protected Dictionary<string, CalendarEvent>  getAllCalendarElements(TimeLine RangeOfLookup, string desiredDirectory="")
        {
            Dictionary<string, CalendarEvent> retValue=new Dictionary<string,CalendarEvent>();
            retValue = UserLog.getAllCalendarFromXml(RangeOfLookup);
            return retValue;
        }

        virtual async protected Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {
            DateTimeOffset retValue = await  UserLog.getDayReferenceTime(desiredDirectory);
            return retValue;
        }

        virtual public bool DeleteAllCalendarEvents()
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

        virtual public void UpdateReferenceDayTime(DateTimeOffset referenceTime)
        {
            UserLog.UpdateReferenceDayInXMLLog(referenceTime);
        }

        virtual async public Task Commit(IEnumerable<CalendarEvent> AllEvents, CalendarEvent calendarEvent, String LatestID)
        {
            await UserLog.Commit(AllEvents, calendarEvent, LatestID).ConfigureAwait(false);
        }

        
#if ForceReadFromXml
#else
        async public Task batchMigrateXML()
        {
            await UserLog.BatchMigrateXML();
        }
        /// <summary>
        /// This inserts a new entry cassandra into cassandra and updates the search engines. Use this when writing data to cassandra db.
        /// </summary>
        /// <param name="newCalEvent"></param>
        /// <returns></returns>

        virtual async public Task AddNewEventToLog(CalendarEvent newCalEvent)
        {
            if(LogControl.useCassandra)
            {
                await UserLog.AddNewEventToCassandra(newCalEvent);
            }
        }
#endif



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

        public string getFullLogDir
        {
            get 
            {
                return UserLog.getFullLogDir;
            }
        }

        virtual public LogControl ScheduleData
        {
            get
            {
                return UserLog;
            }
        }

#endregion 

    }
}
