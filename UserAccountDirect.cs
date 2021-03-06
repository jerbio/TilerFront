﻿using System;
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
        
        protected UserAccountDirect()
        {
            
        }



        public UserAccountDirect(string userId, TilerDbContext database)
        {
            ID = userId;
            _Database = database;
        }

        /// <summary>
        /// Essentially initializes the user log and pulls a user's account
        /// </summary>
        /// <returns></returns>
        public override async System.Threading.Tasks.Task<bool> Login()
        {
            HttpContext ctx = HttpContext.Current;
            TilerUser tilerUser = Database.Users
                .Include(eachUser => eachUser.ScheduleProfile_DB.PausedTile_DB)
                .SingleOrDefault(eachUser => eachUser.Id == ID);
            if(tilerUser!=null && tilerUser.ScheduleProfile == null)
            {
                tilerUser.initializeScheduleProfile();
            }
            UserLog = new LogControlDirect(tilerUser, Database as ApplicationDbContext);

            bool retValue = tilerUser != null;
            return retValue;
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

        #endregion 

    }
}