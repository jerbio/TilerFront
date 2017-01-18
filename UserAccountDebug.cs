using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class UserAccountDebug:UserAccountDirect
    {
        public UserAccountDebug(TilerUser user)
        {
            UserLog = new LogControlDebug(user, "");
            ID = SessionUser.Id;
        }

        public override async System.Threading.Tasks.Task<bool> Login()
        {
            HttpContext ctx = HttpContext.Current;
            if (ctx != null)
            {
                await UserLog.Initialize();
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