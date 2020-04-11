using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBTilerElement;
using TilerElements;
namespace TilerFront
{
    public class OutLookConnector : OutlookThirdPartyControl
    {
        public OutLookConnector()
        {

        }
        public OutLookConnector (Dictionary<string, CalendarEvent> CalendarData):base(CalendarData)
        {

        }

        public override void removeAllEventsFromOutLook(IEnumerable<CalendarEvent> ArrayOfCalendarEvents)
        {
            base.removeAllEventsFromOutLook(ArrayOfCalendarEvents);
        }
        public override void WriteToOutlook(CalendarEvent MyEvent)
        {
            base.WriteToOutlook(MyEvent);
        }
    }
}