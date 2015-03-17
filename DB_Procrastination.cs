using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    internal class DB_Procrastination:Procrastination
    {
        internal DB_Procrastination(DateTimeOffset FromTimeData, DateTimeOffset BeginTimeData, int DisLikedSection)
        {
            FromTime = FromTimeData;
            BeginTIme = BeginTimeData;
            SectionOfDay = DisLikedSection;
        }
    }
}