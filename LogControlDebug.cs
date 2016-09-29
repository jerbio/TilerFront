﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
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
        public LogControlDebug(Models.ApplicationUser User, string logLocation="", bool Passive=false):base(User,logLocation,Passive)
        {
            
        }


        public override async System.Threading.Tasks.Task<DateTimeOffset> getDayReferenceTime(string NameOfFile = "")
        {
            return await base.getDayReferenceTimeFromXml(NameOfFile);
        }

    }
}