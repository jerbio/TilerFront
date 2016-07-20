using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using TilerElements.Wpf;
using TilerElements.DB;
using System.Threading.Tasks;

namespace TilerFront
{
    public class UserAccountDirect : UserAccount
    {
        //LogControl UserLog;
        protected UserAccountDirect()
        {

        }



        public UserAccountDirect(TilerUser user, bool Passive = false)
        {
            sessionUser = user;
            ID = sessionUser.Id;
            throw new NotImplementedException();
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
            return UserLog.Status;
        }





        async override protected Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {
            DateTimeOffset retValue = sessionUser.ReferenceDay;
            return retValue;
        }


        async protected Task<DateTimeOffset> getDayReferenceTimeFromXml(string desiredDirectory = "")
        {

            DateTimeOffset retValue = sessionUser.ReferenceDay;
            return retValue;
        }



        override async public Task<CustomErrors> DeleteLog()
        {
            return await UserLog.DeleteLog();
        }


        override public bool DeleteAllCalendarEvents()
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



        #region properties



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


        virtual public ScheduleControl ScheduleLogControl
        {
            get
            {
                return UserLog;
            }
        }

        #endregion 

    }
}