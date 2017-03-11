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
        public string CalAddress { get; set; }
        public string CalAddressDescription { get; set; }
        public int AllEvents = 0;

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