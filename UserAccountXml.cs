using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class UserAccountXml:UserAccountDirect
    {
        public UserAccountXml(TilerUser user)
        {
            UserLog = new LogControlXml(user, "");
            ID = SessionUser.Id;
        }

        /// <summary>
        /// Function logs in the current user. It simply checks to see if the id already exists
        /// </summary>
        /// <returns></returns>
        public override async System.Threading.Tasks.Task<bool> Login()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
            {
                await (UserLog as LogControlXml).Initialize();
            }


            bool retValue = UserLog.Status && (SessionUser != null);
            return retValue;
        }

        /*
        public override async System.Threading.Tasks.Task<bool> Login()
        {
            await UserLog.Initialize();
            return UserLog.Status;
        }
        */
    }

}