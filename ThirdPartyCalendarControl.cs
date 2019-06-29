using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TilerElements;




namespace TilerElements
{
    public abstract class ThirdPartyCalendarControl
    {
        protected ThirdPartyControl.CalendarTool SelectedCalendarTool;
        protected Dictionary<string, CalendarEvent> IDToCalendarEvent = new Dictionary<string, CalendarEvent>();
        ThirdPartyCalendarEvent ThirdpartyCalendarEventInfo;
        protected ThirdPartyCalendarControl()
        {}
        
        abstract public void DeleteAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent="", string entryID="");
        abstract public string AddAppointment(SubCalendarEvent ActiveSection, string NameOfParentCalendarEvent = "");

        public virtual Dictionary<string, CalendarEvent> getAllCalendarEvents()
        {
            return IDToCalendarEvent.ToDictionary(obj => obj.Key, obj => obj.Value);
        }

        void populateThirdpartyCalendarEventInfo ()
        {
            ThirdpartyCalendarEventInfo = new ThirdPartyCalendarEvent(IDToCalendarEvent.Values);

        }

        virtual public CalendarEvent getThirdpartyCalendarEvent()
        {
            if (ThirdpartyCalendarEventInfo==null)
            {
                populateThirdpartyCalendarEventInfo();
            }
            return ThirdpartyCalendarEventInfo;
        }

    }

    class ThirdPartyCalendarEvent:CalendarEvent
    {
        protected ThirdPartyCalendarEvent()
        {

        }



        public ThirdPartyCalendarEvent(IEnumerable<CalendarEvent>AllCalendarEvent)
        {
            this._EventDuration = new TimeSpan(50);
            this._Splits = 1;
            this._AverageTimePerSplit= this._EventDuration;
            this._UiParams = new EventDisplay();
            this.UnDesignables = new HashSet<SubCalendarEvent>();
            this.UniqueID = EventID.generateGoogleCalendarEventID((uint)AllCalendarEvent.Count());
            this._UserDeleted = false;
            //this._Users= new List<string>();
            this.StartDateTime = DateTimeOffset.Now.AddDays(-90);
            this.EndDateTime = this.StartDateTime.AddDays(180);
            this._Enabled = true;
            this._Complete = false;
            this._DeletedCount = 1;
            this._CompletedCount = 1;
            this._Name = new EventName(null, this, "GOOGLE MOTHER EVENT");
            this._DataBlob = new MiscData();
            if (AllCalendarEvent.Count() > 0)
            {
                this._EventRepetition = new Repetition(true, this.RangeTimeLine, "Daily", AllCalendarEvent.ToArray());
            }
            this._ProfileOfNow = new NowProfile();
        }
    }
}
