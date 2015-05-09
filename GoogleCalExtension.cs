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

namespace TilerFront
{
    public static class GoogleCalExtension
    {
        static HashSet<string> GoogleIDs = new HashSet<string>();
        
        public static SubCalEvent ToRepeatInstance(this Google.Apis.Calendar.v3.Data.Event myEvent, EventID CalendarID,uint CurrentCount)
        {
            SubCalEvent retValue = new SubCalEvent();
            retValue.ThirdPartyEventID = myEvent.Id;
            retValue.ThirdPartyType = TilerElementExtension.ProviderNames[(int)ThirdPartyControl.CalendarTool.Google];
            retValue.ThirdPartyUserID = myEvent.Organizer.Email;
            EventID SubEVentID = EventID.generateRepeatGoogleSubCalendarEventID(CalendarID, CurrentCount);
            retValue.ID = SubEVentID.ToString();
            retValue.CalendarID = SubEVentID.getRepeatCalendarEventID();


            retValue.SubCalStartDate = (long)(new DateTimeOffset(myEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(myEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            //retValue.SubCalStartDate = (long)(DateTimeOffset.Parse(myEvent.Start.DateTimeRaw) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            //retValue.SubCalEndDate = (long)(DateTimeOffset.Parse(myEvent.End.DateTimeRaw) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalTotalDuration = (myEvent.End.DateTime.Value - myEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = myEvent.Location;// SubCalendarEventEntry.myLocation.Description;
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
            retValue.ThirdPartyType = TilerElementExtension.ProviderNames[(int)ThirdPartyControl.CalendarTool.Google];
            retValue.ThirdPartyUserID = myEvent.Organizer.Email;


            retValue.ID = AuthenticationID.getIDUpToRepeatDayCalendarEvent()+"_" + CurrentCount + "_1";
            retValue.CalendarID = AuthenticationID.getIDUpToRepeatDayCalendarEvent() + "_" + CurrentCount + "_0";
            retValue.isThirdParty = true;



            retValue.SubCalStartDate = (long)(new DateTimeOffset(myEvent.Start.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalEndDate = (long)(new DateTimeOffset(myEvent.End.DateTime.Value) - TilerElementExtension.JSStartTime).TotalMilliseconds;

            //retValue.SubCalStartDate = (long)(DateTimeOffset.Parse(myEvent.Start.DateTimeRaw) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            //retValue.SubCalEndDate = (long)(DateTimeOffset.Parse(myEvent.End.DateTimeRaw) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            retValue.SubCalTotalDuration = (myEvent.End.DateTime.Value - myEvent.Start.DateTime.Value);
            retValue.SubCalRigid = true;
            retValue.SubCalAddressDescription = myEvent.Location;// SubCalendarEventEntry.myLocation.Description;
            retValue.SubCalAddress = myEvent.Location;
            retValue.SubCalCalendarName = myEvent.Summary;

            retValue.SubCalCalEventStart = retValue.SubCalStartDate;
            retValue.SubCalCalEventEnd = retValue.SubCalEndDate;

            /*
            retValue.SubCalEventLong = SubCalendarEventEntry.myLocation.YCoordinate;
            retValue.SubCalEventLat = SubCalendarEventEntry.myLocation.XCoordinate;
            retValue.RColor = SubCalendarEventEntry.UIParam.UIColor.R;
            retValue.GColor = SubCalendarEventEntry.UIParam.UIColor.G;
            retValue.BColor = SubCalendarEventEntry.UIParam.UIColor.B;
            retValue.OColor = SubCalendarEventEntry.UIParam.UIColor.O;
            */

            retValue.isComplete = false;
            retValue.isEnabled = true;
            retValue.Duration = (long)retValue.SubCalTotalDuration.TotalMilliseconds;
            //retValue.EventPreDeadline = (long)SubCalendarEventEntry.PreDeadline.TotalMilliseconds;
            retValue.Priority = 0;
            //retValue.Conflict = String.Join(",", SubCalendarEventEntry.Conflicts.getConflictingEventIDs());
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


            RetValue = RetValueSubCalEvents.Select(obj => GoogleSubCalendarEvent .convertFromGoogleToSubCalendarEvent(obj)).ToList();
            return RetValue;
        }

        public async static Task<IEnumerable<CalendarEvent>> getAllCalEvents(IList<Google.Apis.Calendar.v3.Data.Event> AllSubCals, Google.Apis.Calendar.v3.CalendarService CalendarServiceData, string UserID,EventID AuthenticationID)
        {
            //ThirdPartyControl.CalendarTool calendarInUser = ThirdPartyControl.CalendarTool.Google;

            List<Google.Apis.Calendar.v3.Data.Event> AllSubCalNoCancels = AllSubCals.Where(obj => obj.Status != "cancelled").ToList();

            Dictionary<string, Google.Apis.Calendar.v3.Data.Event> RepeatingIDs = new Dictionary<string, Google.Apis.Calendar.v3.Data.Event>();

            ConcurrentBag<CalendarEvent> RetValue = new ConcurrentBag<CalendarEvent>();

            TimeLine CalculationTimeLine = new TimeLine(DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow.AddDays(90));

            uint i = 0;
            for (; i < AllSubCalNoCancels.Count; i++)
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
                            RetValue.Add( GoogleCalendarEvent.convertFromGoogleToCalendarEvent(  GoogleEvent.ToSubCal(AuthenticationID,i, CalendarServiceData)));
                        }
                        else
                        {
                            
                            RepeatingIDs.Add(GoogleEvent.Id, GoogleEvent);
                        }
                    }
                }
            }


            KeyValuePair<string, Google.Apis.Calendar.v3.Data.Event>[] DictAsArray = RepeatingIDs.ToArray();


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
                EventID CalendarEventID = EventID.generateGoogleCalendarEventID(myIndex);
                List<SubCalendarEvent> AllRepeatSubCals = generateRepeatSubCalendarEvent(CalendarEventID, generatedRsults.Items);
                
                //List<SubCalEvent> AllRepeatSubCals = generatedRsults.Items.Select((obj, k) => obj.ToSubCal(AuthenticationID,(myIndex + k), CalendarServiceData)).ToList();
                if (AllRepeatSubCals.Count>0)
                {
                    CalendarEvent newlyCreatedCalEVent = CalEvent.FromGoogleToRepatCalendarEvent(AllRepeatSubCals);
                    RetValue.Add(newlyCreatedCalEVent);
                }
                

            }
            //);
            return RetValue;
        }


        


        
        //System.Collections.Generic.IList<Google.Apis.Calendar.v3.Data.Event> {System.Collections.Generic.List<Google.Apis.Calendar.v3.Data.Event>}

    }
}