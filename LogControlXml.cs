using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using TilerElements;
using System.Threading.Tasks;
using System.Xml;

namespace TilerFront
{
    public class LogControlXml:LogControlDirect
    {
        string UserID;
        public static string CurrentLog;
        public static string WagTapLogLocation;
        /*
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }
        */
        public LogControlXml(TilerUser User, string logLocation=""):base(User, null)
        {
        }


        override public DateTimeOffset getDayReferenceTime()
        {
            DateTimeOffset retValue = _TilerUser.EndfOfDay;
            return retValue;
        }

        public override bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        virtual async public Task Initialize()
        {
            CurrentLog = "";

            CurrentLog = ID.ToString() + ".xml";
            string LogDir = (WagTapLogLocation + CurrentLog);
            string myCurrDir = Directory.GetCurrentDirectory();
            Console.WriteLine("Log DIR is:" + LogDir);
            LogStatus = File.Exists(LogDir);

            _TilerUser = Database.Users.Find(ID);
            UserName = _TilerUser.UserName;
        }


        public override Task updateBigData(XmlDocument oldData, XmlDocument newData)
        {

            Task retValue = new Task(() => { });
            retValue.RunSynchronously();
            retValue.Wait();
            return retValue;
        }
    }
}