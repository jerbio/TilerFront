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
    public class LogControlDebug:LogControlDirect
    {
        string UserID;
        /*
        public LogControlDebug(string userid)
        {
            UserID = userid;
        }
        */
        public LogControlDebug(TilerUser User, string logLocation=""):base(User, null,logLocation)
        {
        }


        async public override Task<DateTimeOffset> getDayReferenceTime(string NameOfFile = "")
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

        public override Task updateBigData(XmlDocument oldData, XmlDocument newData)
        {

            Task retValue = new Task(() => { });
            retValue.RunSynchronously();
            retValue.Wait();
            return retValue;
        }
    }
}