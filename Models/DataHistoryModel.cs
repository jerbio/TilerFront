using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;
using static TilerElements.UserActivity;

namespace TilerFront.Models
{
    public class DataHistoryModel:AuthorizedUser
    {
        ActivityType _UserActivity = ActivityType.None;
        public string UserActivityType {
            get
            {
                return _UserActivity.ToString();
            }
            set
            {
                _UserActivity = Utility.ParseEnum<ActivityType>(value);
            }
        }

        public ActivityType ActivityType
        {
            get
            {
                return _UserActivity;
            }
        }

        public long Start { get; set; } = -1;
        public long End { get; set; } = -1;

        public TimeLine TimeLine {
            get
            {
                DateTimeOffset start = Start == -1 ? DateTimeOffset.Now.AddDays(-7) : DateTimeOffset.FromUnixTimeMilliseconds(this.Start);
                DateTimeOffset end= End == -1 ? start.AddDays(7): DateTimeOffset.FromUnixTimeMilliseconds(this.End);

                TimeLine retValue = new TimeLine(start, end);
                return retValue;
            }
        }
    }
}