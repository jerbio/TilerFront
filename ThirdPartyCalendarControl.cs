using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;




namespace TilerElements
{
    public abstract class ThirdPartyCalendarControls
    {
        protected ThirdPartyControl.CalendarTool SelectedCalendarTool;
        protected Dictionary<string, CalendarEvent> IDToCalendarEvent = new Dictionary<string, CalendarEvent>();
        ThirdPartyCalendarEvent ThirdpartyCalendarEventInfo;
        protected ThirdPartyCalendarControls()
        {}
        
        //abstract public void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent="", string entryID="");
        //abstract public string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "");

        //public virtual Dictionary<string, CalendarEvent> getAllCalendarEvents()
        //{
        //    return IDToCalendarEvent.ToDictionary(obj => obj.Key, obj => obj.Value);
        //}

        //void populateThirdpartyCalendarEventInfo ()
        //{
        //    ThirdpartyCalendarEventInfo = new ThirdPartyCalendarEvent(IDToCalendarEvent.Values);

        //}

        //virtual public CalendarEvent getThirdpartyCalendarEvent()
        //{
        //    if (ThirdpartyCalendarEventInfo==null)
        //    {
        //        populateThirdpartyCalendarEventInfo();
        //    }
        //    return ThirdpartyCalendarEventInfo;
        //}

    }
}
