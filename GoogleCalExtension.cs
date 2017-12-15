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
        
        public static SubCalEvent ToRepeatInstance(this Google.Apis.Calendar.v3.Data.Event myEvent, EventID CalendarID,uint CurrentCount)
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
        
        public static SubCalEvent ToSubCal(this Google.Apis.Calendar.v3.Data.Event myEvent, EventID AuthenticationID, uint CurrentCount,Google.Apis.Calendar.v3.CalendarService CalendarServiceData)
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

        public async static Task<IEnumerable<SubCalendarEvent>> getAllSubCallEvents(IList<Google.Apis.Calendar.v3.Data.Event> AllSubCals, Google.Apis.Calendar.v3.CalendarService CalendarServiceData, string UserID, EventID AuthenticationID)
        {
            //ThirdPartyControl.CalendarTool calendarInUser =  ThirdPartyControl.CalendarTool.Google;
            
            List<Google.Apis.Calendar.v3.Data.Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string,Google.Apis.Calendar.v3.Data.Event > RepeatingIDs = new Dictionary<string,Google.Apis.Calendar.v3.Data.Event>();

            ConcurrentBag<SubCalEvent> RetValue = new ConcurrentBag<SubCalEvent>();

            TimeLine CalculationTimeLine = new TimeLine(DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow.AddDays(90));

            uint i = 0;
            for (; i < AllSubCalNoCancels.Count;i++ )
            {
                Google.Apis.Calendar.v3.Data.Event GoogleEvent = AllSubCalNoCancels[(int)i];

                

                if (GoogleEvent.Start.DateTime != null) 
                {
                    TimeLine EventRange = new TimeLine(GoogleEvent.Start.DateTime.Value.ToUniversalTime(), GoogleEvent.End.DateTime.Value.ToUniversalTime());
                    if (EventRange.InterferringTimeLine(CalculationTimeLine) != null)
                    {

                        if (GoogleEvent.Recurrence == null)
                        {
                            GoogleIDs.Add(GoogleEvent.Id);
                            RetValue.Add(GoogleEvent.ToSubCal(AuthenticationID,i, CalendarServiceData));
                        }
                        else
                        {
                            
                            RepeatingIDs.Add(GoogleEvent.Id, GoogleEvent);
                        }
                    }
                }
            }


            KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event> []DictAsArray = RepeatingIDs.ToArray();


            //foreach (KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event> eachKeyValuePair in RepeatingIDs)

            for (uint j = 0; j < DictAsArray.Length; j++)
            

            //Parallel.For(0, DictAsArray.Length, async (j) =>
                {
                    uint myIndex = i + j;
                    KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event> eachKeyValuePair = DictAsArray[j];
                    var RepetitionData = CalendarServiceData.Events.Instances(UserID, eachKeyValuePair.Key);
                    RepetitionData.ShowDeleted = false;
                    RepetitionData.TimeMax = DateTime.Now.AddDays(90);
                    var generatedRsults = await RepetitionData.ExecuteAsync().ConfigureAwait(false);
                    List<SubCalEvent> AllRepeatSubCals = generatedRsults.Items.Select(obj => obj.ToSubCal(AuthenticationID,(myIndex), CalendarServiceData)).ToList();
                    AllRepeatSubCals.ForEach(obj => {
                        if (!GoogleIDs.Contains(obj.ThirdPartyEventID))
                        {RetValue.Add(obj); }
                    });

                }
            //);
            return RetValue.Select(obj=> GoogleSubCalendarEvent.convertFromGoogleToSubCalendarEvent( obj));
        }

        

        static List<SubCalendarEvent> generateRepeatSubCalendarEvent(EventID CalendarEventID, IList<Google.Apis.Calendar.v3.Data.Event> AllSubCalEvents)
        {
            uint j = 1;
            List<SubCalEvent> RetValueSubCalEvents = new List<SubCalEvent>();
            List<SubCalendarEvent> RetValue = new List<SubCalendarEvent>();
            foreach(Google.Apis.Calendar.v3.Data.Event eachEvent in AllSubCalEvents)
            {
                if (!GoogleIDs.Contains(eachEvent.Id))
                {
                    RetValueSubCalEvents.Add(ToRepeatInstance(eachEvent, CalendarEventID, j++));
                }
            }


            RetValue = RetValueSubCalEvents.Select(obj => GoogleSubCalendarEvent .convertFromGoogleToSubCalendarEvent(obj, new TilerElements.Location(obj.SubCalAddressDescription))).ToList();
            return RetValue;
        }

        public async static Task<IEnumerable<CalendarEvent>> getAllCalEvents(IList<Google.Apis.Calendar.v3.Data.Event> AllSubCals, Google.Apis.Calendar.v3.CalendarService CalendarServiceData, string UserID,EventID AuthenticationID, TimeLine CalculationTimeLine, bool retrieveLocationFromGoogle)
        {
            List<Google.Apis.Calendar.v3.Data.Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string, Google.Apis.Calendar.v3.Data.Event> RepeatingIDs = new Dictionary<string, Google.Apis.Calendar.v3.Data.Event>();

            ConcurrentBag<CalendarEvent> RetValue = new ConcurrentBag<CalendarEvent>();
            if (CalculationTimeLine == null) {
                CalculationTimeLine = new TimeLine(DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow.AddDays(90));
            }

            ConcurrentBag<TilerElements.Location> locations = new ConcurrentBag<TilerElements.Location>();
            uint i = 0;
            for (; i < AllSubCalNoCancels.Count; i++)
            {
                Google.Apis.Calendar.v3.Data.Event GoogleEvent = AllSubCalNoCancels[(int)i];
                if (GoogleEvent.Start.DateTime != null)
                {
                    TimeLine EventRange = new TimeLine(GoogleEvent.Start.DateTime.Value.ToUniversalTime(), GoogleEvent.End.DateTime.Value.ToUniversalTime());
                    if (EventRange.InterferringTimeLine(CalculationTimeLine) != null || GoogleEvent.Recurrence != null)
                    {
                        if (GoogleEvent.Recurrence == null)
                        {
                            GoogleIDs.Add(GoogleEvent.Id);
                            CalendarEvent calEvent = GoogleCalendarEvent.convertFromGoogleToCalendarEvent(GoogleEvent.ToSubCal(AuthenticationID, i, CalendarServiceData));
                            RetValue.Add(calEvent);
                            locations.Add(calEvent.Location);
                        }
                        else
                        {
                            RepeatingIDs.Add(GoogleEvent.Id, GoogleEvent);
                        }
                    }
                }
            }
            KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event>[] DictAsArray = RepeatingIDs.ToArray();
            for (uint j = 0; j < DictAsArray.Length; j++)
            {
                uint myIndex = i + j;
                KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event> eachKeyValuePair = DictAsArray[j];
                var RepetitionData = CalendarServiceData.Events.Instances(UserID, eachKeyValuePair.Key);
                RepetitionData.ShowDeleted = false;
                RepetitionData.TimeMax = DateTime.Now.AddDays(90);
                var generatedRsults = await RepetitionData.ExecuteAsync().ConfigureAwait(false);
                EventID CalendarEventID = EventID.generateGoogleCalendarEventID(myIndex);
                List<Event> googleEventsWithinRange = generatedRsults.Items.Where(googleEvent => googleEvent.End.DateTime.Value.ToUniversalTime() > CalculationTimeLine.Start && CalculationTimeLine.End > googleEvent.Start.DateTime.Value.ToUniversalTime()).ToList();
                List<SubCalendarEvent> AllRepeatSubCals = generateRepeatSubCalendarEvent(CalendarEventID, googleEventsWithinRange);
                
                if (AllRepeatSubCals.Count>0)
                {
                    CalendarEvent newlyCreatedCalEVent = CalEvent.FromGoogleToRepatCalendarEvent(AllRepeatSubCals);
                    newlyCreatedCalEVent.AllSubEvents.AsParallel().ForAll((subEvent) => locations.Add(subEvent.Location));
                    RetValue.Add(newlyCreatedCalEVent);
                }
            }

            if(retrieveLocationFromGoogle)
            {
                HashSet<TilerElements.Location> hashLocation = new HashSet<TilerElements.Location>(locations);
                batchValidateLocations(hashLocation);
            }
            return RetValue;
        }

        static void batchValidateLocations (IEnumerable<TilerElements.Location> iterlocations)
        {
            ConcurrentDictionary<string, ConcurrentBag<TilerElements.Location>> addressesToLocations = new ConcurrentDictionary<string, ConcurrentBag<TilerElements.Location>>();
            iterlocations.AsParallel().ForAll((location) => {
                ConcurrentBag<TilerElements.Location> locations;
                string address = location.Address;
                address = address.Replace(",", "_");
                address = Regex.Replace(address, @"\s+", "_");
                address = Regex.Replace(address, @"_+", "_");
                if (addressesToLocations.ContainsKey(address))
                {
                    locations = addressesToLocations[address];
                }
                else
                {
                    locations = new ConcurrentBag<TilerElements.Location>();
                    bool addSuccess = false;
                    while (!addSuccess)
                    {

                        addSuccess = addressesToLocations.TryAdd(address, locations);
                        if(!addSuccess)
                        {
                            addSuccess = addressesToLocations.ContainsKey(address);
                        }
                    }
                }
                if (!String.IsNullOrEmpty(address))
                {
                    locations.Add(location);
                }
            });

            addressesToLocations.AsParallel().ForAll((kvp) =>
            {
                TilerElements.Location location = new TilerElements.Location(kvp.Key);
                if (location.Validate())
                {
                    kvp.Value.AsParallel().ForAll((eachLocation) =>
                    {
                        eachLocation.update(location);
                    });
                }
            });
        }
        


        
        //System.Collections.Generic.IList<Google.Apis.Calendar.v3.Data.Event> {System.Collections.Generic.List<Google.Apis.Calendar.v3.Data.Event>}

    }
}