using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBTilerElement;

namespace TilerFront.Models
{
    public class EditCalEventModel : getEventModel
    {
        public string EventName { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long Duration { get; set; }
        public long Split { get; set; }


        public DateTimeOffset getStart()
        {
            DateTimeOffset newStart = TilerElementExtension.JSStartTime.AddMilliseconds(this.Start);
            newStart = newStart.Add(this.getTImeSpan);
            return newStart;
        }


        public DateTimeOffset getEnd()
        {
            DateTimeOffset newEnd = TilerElementExtension.JSStartTime.AddMilliseconds(this.End);
            newEnd = newEnd.Add(this.getTImeSpan);
            return newEnd;
        }
    }
}