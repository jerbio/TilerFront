using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    internal class DB_RestrictionTimeLine : RestrictionTimeLine
    {
        internal DB_RestrictionTimeLine(DateTimeOffset StartData, DateTimeOffset EndData, TimeSpan SpanData)
        {
            this.StartTimeOfDay=StartData;
            this.EndTimeOfDay = EndData;
            this.RangeTimeSpan = SpanData;
        }
    }
}