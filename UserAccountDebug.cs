using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront
{
    public class UserAccountDebug:UserAccount
    {
        
        public UserAccountDebug(string UserID)
        {
            UserLog = new LogControlDebug(UserID);
        }

        public override async System.Threading.Tasks.Task<bool> Login()
        {
            await UserLog.Initialize();
            return UserLog.Status;
        }
    }
}