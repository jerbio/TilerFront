using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    internal class DB_RestrictionProfile:RestrictionProfile
    {
        internal DB_RestrictionProfile(List<Tuple<DayOfWeek, RestrictionTimeLine>> RestrictionTimeLineData)
        {
            this.DaySelection = new Tuple<DayOfWeek, RestrictionTimeLine>[7];
            foreach (Tuple<DayOfWeek, RestrictionTimeLine> eachTuple in RestrictionTimeLineData)
            {
                DaySelection[(int)eachTuple.Item1] = eachTuple;
            }

            this.NoNull_DaySelections = RestrictionTimeLineData.OrderBy(obj => obj.Item1).ToArray();
            InitializeOverLappingDictionary();
        }
    }
}