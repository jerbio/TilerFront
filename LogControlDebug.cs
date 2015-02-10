using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
namespace TilerFront
{
    public class LogControlDebug:LogControlDirect
    {
        string UserID;
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }

        public override async System.Threading.Tasks.Task Initialize()
        {
            CurrentLog = "";
            //if (VerifiedUser.Item1)
            {
#if ForceReadFromXml
#else
                myCassandraAccess = new CassandraUserLog.CassandraLog(ID);
#endif
                
                    CurrentLog = UserID + ".xml";
                    string LogDir = (WagTapLogLocation + CurrentLog);
                    string myCurrDir = Directory.GetCurrentDirectory();
                    Console.WriteLine("Log DIR is:" + LogDir);
                    LogStatus = File.Exists(LogDir);
            }


        }

    }
}