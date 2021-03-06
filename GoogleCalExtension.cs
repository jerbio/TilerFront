﻿using System;
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
using Location = TilerElements.Location;

namespace TilerFront
{
    public static class GoogleCalExtension
    {
        static HashSet<string> GoogleIDs = new HashSet<string>();
        
        public static SubCalEvent ToRepeatInstance(this Event googleEvent, EventID CalendarID,uint CurrentCount)
        {
            SubCalEvent retValue = new SubCalEvent();
            retValue.ThirdPartyEventID = googleEvent.Id;
            retValue.ThirdPartyType = ThirdPartyControl.CalendarTool.google.ToString();
            retValue.ThirdPartyUserID = googleEvent.Organizer.Email;
            EventID SubEVentID = EventID.generateRepeatGoogleSubCalendarEventID(CalendarID, CurrentCount);
            retValue.ID = SubEVentID.ToString();
            retValue.CalendarID = SubEVentID.getRepeatCalendarEventID();


            retValue.SubCalStartDate = (long)(new DateTimeOffset(googleEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(googleEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            retValue.SubCalTotalDuration = (googleEvent.End.DateTime.Value - googleEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = googleEvent.Location;// SubCalendarEventEntry.Location.Description;
            retValue.SubCalAddress = googleEvent.Location;
            retValue.SubCalCalendarName = googleEvent.Summary;

            retValue.SubCalCalEventStart = retValue.SubCalStartDate;
            retValue.SubCalCalEventEnd = retValue.SubCalEndDate;
            retValue.isThirdParty = true;
            retValue.isReadOnly = false;
            if(googleEvent.ExtendedProperties!=null && googleEvent.ExtendedProperties.Private__!=null && googleEvent.ExtendedProperties.Private__.ContainsKey(GoogleTilerEventControl.tilerReadonlyKey))
            {
                retValue.isReadOnly = Convert.ToBoolean(googleEvent.ExtendedProperties.Private__[GoogleTilerEventControl.tilerReadonlyKey]);
            }
            


            retValue.isComplete = false;
            retValue.isEnabled = true;
            retValue.Duration = (long)retValue.SubCalTotalDuration.TotalMilliseconds;
            //retValue.EventPreDeadline = (long)SubCalendarEventEntry.PreDeadline.TotalMilliseconds;
            retValue.Priority = 0;
            //retValue.Conflict = String.Join(",", SubCalendarEventEntry.Conflicts.getConflictingEventIDs());
            retValue.ColorSelection = 0;
            return retValue;
        }
        
        public static SubCalEvent ToSubCal(this Event googleEvent, EventID AuthenticationID, uint CurrentCount)
        {
            SubCalEvent retValue = new SubCalEvent();
            retValue.ThirdPartyEventID = googleEvent.Id;
            retValue.ThirdPartyType = ThirdPartyControl.CalendarTool.google.ToString();
            retValue.ThirdPartyUserID = googleEvent.Organizer?.Email;


            retValue.ID = AuthenticationID.getIDUpToRepeatDayCalendarEvent()+"_" + CurrentCount + "_1";
            retValue.CalendarID = AuthenticationID.getIDUpToRepeatDayCalendarEvent() + "_" + CurrentCount + "_0";
            retValue.isThirdParty = true;
            retValue.SubCalAddressDescription = googleEvent.Location;


            retValue.SubCalStartDate = (long)(new DateTimeOffset(googleEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(googleEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            retValue.SubCalTotalDuration = (googleEvent.End.DateTime.Value - googleEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = googleEvent.Location;// SubCalendarEventEntry.Location.Description;
            retValue.SubCalAddress = googleEvent.Location;
            retValue.SubCalCalendarName = googleEvent.Summary;
            retValue.isReadOnly = false;
            if (googleEvent.ExtendedProperties != null && googleEvent.ExtendedProperties.Private__ != null && googleEvent.ExtendedProperties.Private__.ContainsKey(GoogleTilerEventControl.tilerReadonlyKey))
            {
                retValue.isReadOnly = Convert.ToBoolean(googleEvent.ExtendedProperties.Private__[GoogleTilerEventControl.tilerReadonlyKey]);
            }
            if (retValue.ThirdPartyUserID == null || retValue.SubCalCalendarName == null)
            {
                retValue.SubCalCalendarName = "busy";
            }

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
            DateTimeOffset now = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeLine CalculationTimeLine = new TimeLine(now.AddDays(Utility.defaultBeginDay), now.AddDays(Utility.defaultEndDay));

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
                    RepetitionData.TimeMax = DateTime.Now.AddDays(Utility.defaultEndDay);
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

        public async static Task<IEnumerable<CalendarEvent>> getAllCalEvents(IList<Event> AllSubCals, CalendarService CalendarServiceData, string UserID,EventID AuthenticationID, TimeLine CalculationTimeLine, bool retrieveLocationFromGoogle, Location defaultLocation = null)
        {
            List<Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string, Event> RepeatingIDs = new Dictionary<string, Event>();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            ConcurrentBag<CalendarEvent> RetValue = new ConcurrentBag<CalendarEvent>();
            if (CalculationTimeLine == null) {
                CalculationTimeLine = new TimeLine(now.AddDays(Utility.defaultBeginDay), now.AddDays(Utility.defaultEndDay));
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
                        locations.Add(calEvent.LocationObj);
                        if (defaultLocation != null && (calEvent.LocationObj!= null && (calEvent.LocationObj.isNull || calEvent.LocationObj.isDefault)))
                        {
                            calEvent.Location_DB = defaultLocation;
                            foreach(var eachSubEvent in calEvent.AllSubEvents)
                            {
                                eachSubEvent.Location_DB = defaultLocation;
                            }
                        }
                    }
                }
            }

            if (retrieveLocationFromGoogle)
            {
                batchValidateLocations(locations);
            } else
            {
                foreach(TilerElements.Location location in locations)
                {
                    location.IsVerified = true;
                }
            }
            return RetValue;
        }

        static void batchValidateLocations(IEnumerable<TilerElements.Location> iterlocations)
        {
            ILookup<string, TilerElements.Location> addressesToLocations = iterlocations.ToLookup(location => location.Address.Trim(), location => location);
            foreach (var kvL in addressesToLocations)
            {
                TilerElements.Location location = new TilerElements.Location(kvL.Key);
                if (location.verify())
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