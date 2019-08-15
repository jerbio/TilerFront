using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DBTilerElement;
using TilerElements;
using System.Threading.Tasks;
using System.Collections.Concurrent;


using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using System.Text.RegularExpressions;

namespace TilerFront
{
    public static class GoogleCalExtension
    {
        static HashSet<string> GoogleIDs = new HashSet<string>();
        
        public static SubCalEvent ToRepeatInstance(this Event myEvent, EventID CalendarID,uint CurrentCount)
        {
            SubCalEvent retValue = new SubCalEvent();
            retValue.ThirdPartyEventID = myEvent.Id;
            retValue.ThirdPartyType = ThirdPartyControl.CalendarTool.google.ToString();
            retValue.ThirdPartyUserID = myEvent.Organizer.Email;
            EventID SubEVentID = EventID.generateRepeatGoogleSubCalendarEventID(CalendarID, CurrentCount);
            retValue.ID = SubEVentID.ToString();
            retValue.CalendarID = SubEVentID.getRepeatCalendarEventID();


            retValue.SubCalStartDate = (long)(new DateTimeOffset(myEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(myEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            retValue.SubCalTotalDuration = (myEvent.End.DateTime.Value - myEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = myEvent.Location;// SubCalendarEventEntry.Location.Description;
            retValue.SubCalAddress = myEvent.Location;
            retValue.SubCalCalendarName = myEvent.Summary;

            retValue.SubCalCalEventStart = retValue.SubCalStartDate;
            retValue.SubCalCalEventEnd = retValue.SubCalEndDate;
            retValue.isThirdParty = true;

            

            retValue.isComplete = false;
            retValue.isEnabled = true;
            retValue.Duration = (long)retValue.SubCalTotalDuration.TotalMilliseconds;
            //retValue.EventPreDeadline = (long)SubCalendarEventEntry.PreDeadline.TotalMilliseconds;
            retValue.Priority = 0;
            //retValue.Conflict = String.Join(",", SubCalendarEventEntry.Conflicts.getConflictingEventIDs());
            retValue.ColorSelection = 0;
            return retValue;
        }
        
        public static SubCalEvent ToSubCal(this Event myEvent, EventID AuthenticationID, uint CurrentCount)
        {
            SubCalEvent retValue = new SubCalEvent();
            retValue.ThirdPartyEventID = myEvent.Id;
            retValue.ThirdPartyType = ThirdPartyControl.CalendarTool.google.ToString();
            retValue.ThirdPartyUserID = myEvent.Organizer.Email;


            retValue.ID = AuthenticationID.getIDUpToRepeatDayCalendarEvent()+"_" + CurrentCount + "_1";
            retValue.CalendarID = AuthenticationID.getIDUpToRepeatDayCalendarEvent() + "_" + CurrentCount + "_0";
            retValue.isThirdParty = true;
            retValue.SubCalAddressDescription = myEvent.Location;


            retValue.SubCalStartDate = (long)(new DateTimeOffset(myEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(myEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            retValue.SubCalTotalDuration = (myEvent.End.DateTime.Value - myEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = myEvent.Location;// SubCalendarEventEntry.Location.Description;
            retValue.SubCalAddress = myEvent.Location;
            retValue.SubCalCalendarName = myEvent.Summary;

            retValue.SubCalCalEventStart = retValue.SubCalStartDate;
            retValue.SubCalCalEventEnd = retValue.SubCalEndDate;

            retValue.isComplete = false;
            retValue.isEnabled = true;
            retValue.Duration = (long)retValue.SubCalTotalDuration.TotalMilliseconds;
            retValue.Priority = 0;
            retValue.ColorSelection = 0;
            return retValue;
        }

        public async static Task<IEnumerable<SubCalendarEvent>> getAllSubCallEvents(IList<Event> AllSubCals, CalendarService CalendarServiceData, string UserID, EventID AuthenticationID)
        {
            //ThirdPartyControl.CalendarTool calendarInUser =  ThirdPartyControl.CalendarTool.Google;
            
            List<Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string, Event> RepeatingIDs = new Dictionary<string, Event>();

            ConcurrentBag<SubCalEvent> RetValue = new ConcurrentBag<SubCalEvent>();

            TimeLine CalculationTimeLine = new TimeLine(DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow.AddDays(90));

            uint i = 0;
            for (; i < AllSubCalNoCancels.Count;i++ )
            {
                Event GoogleEvent = AllSubCalNoCancels[(int)i];

                

                if (GoogleEvent.Start.DateTime != null) 
                {
                    TimeLine EventRange = new TimeLine(GoogleEvent.Start.DateTime.Value.ToUniversalTime(), GoogleEvent.End.DateTime.Value.ToUniversalTime());
                    if (EventRange.InterferringTimeLine(CalculationTimeLine) != null)
                    {

                        if (GoogleEvent.Recurrence == null)
                        {
                            GoogleIDs.Add(GoogleEvent.Id);
                            RetValue.Add(GoogleEvent.ToSubCal(AuthenticationID,i));
                        }
                        else
                        {
                            
                            RepeatingIDs.Add(GoogleEvent.Id, GoogleEvent);
                        }
                    }
                }
            }


            KeyValuePair<string, Event> []DictAsArray = RepeatingIDs.ToArray();


            //foreach (KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event> eachKeyValuePair in RepeatingIDs)

            for (uint j = 0; j < DictAsArray.Length; j++)
            

            //Parallel.For(0, DictAsArray.Length, async (j) =>
                {
                    uint myIndex = i + j;
                    KeyValuePair<string, Event> eachKeyValuePair = DictAsArray[j];
                    var RepetitionData = CalendarServiceData.Events.Instances(UserID, eachKeyValuePair.Key);
                    RepetitionData.ShowDeleted = false;
                    RepetitionData.TimeMax = DateTime.Now.AddDays(90);
                    var generatedRsults = await RepetitionData.ExecuteAsync().ConfigureAwait(false);
                    List<SubCalEvent> AllRepeatSubCals = generatedRsults.Items.Select(obj => obj.ToSubCal(AuthenticationID,(myIndex))).ToList();
                    AllRepeatSubCals.ForEach(obj => {
                        if (!GoogleIDs.Contains(obj.ThirdPartyEventID))
                        {RetValue.Add(obj); }
                    });

                }
            //);
            return RetValue.Select(obj=> GoogleSubCalendarEvent.convertFromGoogleToSubCalendarEvent( obj));
        }

        

        static List<SubCalendarEvent> generateRepeatSubCalendarEvent(EventID CalendarEventID, IList<Event> AllSubCalEvents)
        {
            uint j = 1;
            List<SubCalEvent> RetValueSubCalEvents = new List<SubCalEvent>();
            List<SubCalendarEvent> RetValue = new List<SubCalendarEvent>();
            foreach(Event eachEvent in AllSubCalEvents)
            {
                if (!GoogleIDs.Contains(eachEvent.Id))
                {
                    RetValueSubCalEvents.Add(ToRepeatInstance(eachEvent, CalendarEventID, j++));
                }
            }


            RetValue = RetValueSubCalEvents.Select(obj => GoogleSubCalendarEvent .convertFromGoogleToSubCalendarEvent(obj, new TilerElements.Location(obj.SubCalAddressDescription))).ToList();
            return RetValue;
        }

        public async static Task<IEnumerable<CalendarEvent>> getAllCalEvents(IList<Event> AllSubCals, CalendarService CalendarServiceData, string UserID,EventID AuthenticationID, TimeLine CalculationTimeLine, bool retrieveLocationFromGoogle)
        {
            List<Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string, Event> RepeatingIDs = new Dictionary<string, Event>();

            ConcurrentBag<CalendarEvent> RetValue = new ConcurrentBag<CalendarEvent>();
            if (CalculationTimeLine == null) {
                CalculationTimeLine = new TimeLine(DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow.AddDays(90));
            }

            ConcurrentBag<TilerElements.Location> locations = new ConcurrentBag<TilerElements.Location>();
            uint i = 0;
            for (; i < AllSubCalNoCancels.Count; i++)
            {
                Event GoogleEvent = AllSubCalNoCancels[(int)i];
                if (GoogleEvent.Start.DateTime != null)
                {
                    TimeLine EventRange = new TimeLine(GoogleEvent.Start.DateTime.Value.ToUniversalTime(), GoogleEvent.End.DateTime.Value.ToUniversalTime());
                    if (EventRange.InterferringTimeLine(CalculationTimeLine) != null || GoogleEvent.Recurrence != null) // took out manual check for repetition because of we set the property singleEvents in  the function getGoogleEvents in class GoogleTilerEventControl.cs. If you need the manual look up run "git checkout 8db3dab166909255ce112" and jump back to this file you should see logic about this below.
                    {
                        GoogleIDs.Add(GoogleEvent.Id);
                        CalendarEvent calEvent = GoogleCalendarEvent.convertFromGoogleToCalendarEvent(GoogleEvent.ToSubCal(AuthenticationID, i));
                        RetValue.Add(calEvent);
                        locations.Add(calEvent.Location);
                    }
                }
            }

            if (retrieveLocationFromGoogle)
            {
                batchValidateLocations(locations);
            }
            return RetValue;
        }

        static void batchValidateLocations(IEnumerable<TilerElements.Location> iterlocations)
        {
            ILookup<string, TilerElements.Location> addressesToLocations = iterlocations.ToLookup(location => location.Address.Trim(), location => location);
            foreach (var kvL in addressesToLocations)
            {
                TilerElements.Location location = new TilerElements.Location(kvL.Key);
                if (location.Validate())
                {
                    var allLocations = addressesToLocations[kvL.Key];

                    allLocations.AsParallel().ForAll((eachLocation) =>
                    {
                        eachLocation.update(location);
                    });
                }
            }
        }




        //System.Collections.Generic.IList<Google.Apis.Calendar.v3.Data.Event> {System.Collections.Generic.List<Google.Apis.Calendar.v3.Data.Event>}

    }
}