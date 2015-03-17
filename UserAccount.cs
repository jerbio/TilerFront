
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading.Tasks;
using TilerElements;




namespace TilerFront
{
    public class UserAccount
    {
        
        protected LogControl UserLog;
        protected string ID="";
        protected string Name;
        protected string Username;
        string Password;
        protected TilerFront.DBControl UserAccountDBAccess;
        public UserAccount()
        {
            Username = "";
            Password = "";
        }
        /*
        public UserAccount(string UserName, string PassWord)
        {
            this.Username = UserName;
            this.Password = TilerFront.DBControl.encryptString(PassWord);
        }
        */
        public UserAccount(string UserName, string UserID)
        {
            this.Username = UserName;
            this.ID = UserID;
            this.Password = "";
        }

        virtual public async Task<bool> Login()
        {
            if(string.IsNullOrEmpty(ID))
            {
                UserAccountDBAccess = new DBControl(Username, Password);
                UserLog = new LogControl(UserAccountDBAccess);
            }
            else
            {
                UserAccountDBAccess = new DBControl(Username, ID);
                UserLog = new LogControl(UserAccountDBAccess);
            }
            await UserLog.Initialize();
            ID = UserLog.LoggedUserID;
            Name = UserLog.Usersname;
            
            return UserLog.Status;
        }




        async virtual public Task<Tuple<string, CustomErrors>> RegisterOld(string FirstName, string LastName, string Email, string UserName, string PassWord)
        {
            CustomErrors retValue = new CustomErrors(false,"success");
            { 
                PassWord=(DBControl.encryptString(PassWord));
            }
            UserAccountDBAccess = new DBControl(UserName, PassWord);
            Tuple<string, CustomErrors> registrationStatus = await UserAccountDBAccess.RegisterUser(FirstName, LastName, Email);//, UserName, PassWord);
            retValue = registrationStatus.Item2;
            UserLog = new LogControl(UserAccountDBAccess);
            await UserLog.Initialize();
            if (!registrationStatus.Item2.Status)
            {
                Username = UserName;
                Password = PassWord;
                retValue =UserLog.genereateNewLogFile(registrationStatus.Item1.ToString());

                if (retValue.Status && retValue.Code >= 20000000)//error 20000000 denotes log creation issue
                {
                    UserAccountDBAccess.deleteUser();
                }
            }

            Tuple<string, CustomErrors> RetValue = new Tuple<string, CustomErrors>(registrationStatus.Item1, retValue);

            return RetValue;
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

        virtual async public Task<CustomErrors> DeleteLog()
        {
            return await UserLog.DeleteLog();
        }

        virtual public void UpdateReferenceDayTime(DateTimeOffset referenceTime)
        {
            UserLog.UpdateReferenceDayInXMLLog(referenceTime);
        }
        /*
        public void CommitEventToLog(CalendarEvent MyCalEvent)
        {
            UserLog.WriteToLog(MyCalEvent);
        }
        */
        virtual async public Task  CommitEventToLogOld(IEnumerable<CalendarEvent> AllEvents,string LatestID,string LogFile="")
        {
            await UserLog.WriteToLogOld(AllEvents, LatestID, LogFile).ConfigureAwait(false);
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
