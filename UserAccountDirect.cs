using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using System.Threading.Tasks;

namespace TilerFront
{
    public class UserAccountDirect:UserAccount
    {
        protected Models.ApplicationUser sessionUser;
        //LogControl UserLog;
        protected UserAccountDirect()
        {
            
        }



        public UserAccountDirect(Models.ApplicationUser user, bool Passive=false)
        {
            sessionUser = user;
            UserLog = new LogControlDirect(sessionUser, "", Passive);
            ID = sessionUser.Id;
        }

        /*
        public UserAccountDirect(string UserName,string USerID, bool Passive)
        {
            if (!Passive)
            {
                sessionUser = new Models.ApplicationUser();
                sessionUser.UserName = UserName;
                sessionUser.Id = USerID;
            }

            UserLog = new LogControlDirect(sessionUser, "", Passive);
        }*/

        public override async System.Threading.Tasks.Task<bool> Login()
        {
            HttpContext ctx = HttpContext.Current;
            
            if (ctx != null)
            {
                await UserLog.Initialize();
            }
            return UserLog.Status;
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


        async protected Task<DateTimeOffset> getDayReferenceTimeFromXml(string desiredDirectory = "")
        {

            DateTimeOffset retValue = await ((LogControlDirect)UserLog).getDayReferenceTimeFromXml(desiredDirectory);
            return retValue;
        }


        async public Task<CustomErrors> Register(Models.ApplicationUser user)
        {
            CustomErrors retValue = UserLog.genereateNewLogFile(user.Id);
            return retValue;
        }

        override async public Task<CustomErrors> DeleteLog()
        {
            return await UserLog.DeleteLog();
        }
        
        async public Task CommitEventToLog(IEnumerable<CalendarEvent> AllEvents, string LatestID, string LogFile = "")
        {
            await ((LogControlDirect)UserLog).WriteToLog(AllEvents, LatestID, LogFile);
            sessionUser.LastChange = DateTimeOffset.Now.DateTime;
            Task SaveChangesToDB = new Controllers.UserController().SaveUser(sessionUser);
            await SaveChangesToDB;
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
                return UserLog.Status;
            }
        }

        public string UserID
        {
            get
            {
                return sessionUser.Id;
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