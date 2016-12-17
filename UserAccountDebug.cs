﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class UserAccountDebug:UserAccountDirect
    {
        public UserAccountDebug(TilerUser user, bool Passive = false)
        {
            sessionUser = user;
            UserLog = new LogControlDebug(user, "", Passive);
            ID = sessionUser.Id;
        }
        protected override async System.Threading.Tasks.Task<DateTimeOffset> getDayReferenceTime(string desiredDirectory = "")
        {

            return await base.getDayReferenceTimeFromXml(desiredDirectory);
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