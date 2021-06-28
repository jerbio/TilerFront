using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using DBTilerElement;
using System.Collections.Concurrent;
using System.Xml;

using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using TilerFront.Models;

namespace TilerFront
{
    public class GoogleTilerEventControl
    {
        ApplicationDbContext db;
        CalendarService CalendarServiceInfo;
        TilerFront.Models.ThirdPartyCalendarAuthenticationModel AuthenticationInfo;
        const string applicationName = "Tiler Web App";
        static public readonly string tilerReadonlyKey = "tilerKey_isreadOnly";
        string EmailID;

        public GoogleTilerEventControl(TilerFront.Models.ThirdPartyCalendarAuthenticationModel AuthenticationCredential, ApplicationDbContext db)
        {
            CalendarServiceInfo = new Google.Apis.Calendar.v3.CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = AuthenticationCredential.getGoogleOauthCredentials(),
                ApplicationName = applicationName
            });
            AuthenticationInfo = AuthenticationCredential;

            EmailID = AuthenticationCredential.Email;
            this.db = db;
        }


        /// <summary>
        /// Function generates google calendar control. Its populated with events retrieved within +/- 90 days of google calendar update. It returns null if access to google api is lost.
        /// </summary>
        /// <returns></returns>
        async public Task<GoogleThirdPartyControl> getThirdPartyControlForIndex()
        {
            GoogleThirdPartyControl RetValue = null;
            DateTimeOffset now = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            TimeLine timeline = new TimeLine(now.AddDays(Utility.defaultBeginDay), now.AddDays(Utility.defaultEndDay));
            IList<Event> list = getAllGoogleEvents(timeline);
            if(list.Count < 1)
            {
                return RetValue;
            }
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(((TilerFront.Models.IndexedThirdPartyAuthentication)AuthenticationInfo).ThirdPartyIndex.ToString());

            List<CalendarEvent> RetValueList = (await GoogleCalExtension.getAllCalEvents(list, CalendarServiceInfo, EmailID, googleAuthenticationID, null, false, AuthenticationInfo.DefaultLocation).ConfigureAwait(false)).ToList();
            RetValue = new GoogleThirdPartyControl(RetValueList);
            return RetValue;
        }


        /// <summary>
        /// Function generates google calendar control. Its populated with events retrieved within +/- 90 days of google calendar update. It returns null if access to google api is lost.
        /// </summary>
        /// <returns></returns>
        async static public Task<Tuple<List<GoogleTilerEventControl>, GoogleThirdPartyControl>> getThirdPartyControlForIndex(IEnumerable<GoogleTilerEventControl> AllGoogleControls, TimeLine calucaltionTimeLine, bool getGoogleLocation)
        {
            List<GoogleTilerEventControl> AllControls = AllGoogleControls.ToList();
            GoogleThirdPartyControl RetrievedGoogleControls= null;
            List<Task<List<CalendarEvent>>> AllConcurrentTasks= new List<Task<List<CalendarEvent>>>();
            List<CalendarEvent> AllCalEvents = new List<CalendarEvent>();
            List<GoogleTilerEventControl> AllInvalids = new List<GoogleTilerEventControl>();

            for (uint j = 0; j < AllControls.Count; j++)
            {
                GoogleTilerEventControl eachGoogleTilerEventControl = AllControls[(int)j];
                AllConcurrentTasks.Add(eachGoogleTilerEventControl.getCalendarEvents(calucaltionTimeLine, getGoogleLocation, j));
            }

            int i = 0;

            foreach( Task<List<CalendarEvent>> eachTask in AllConcurrentTasks)
            {
                List<CalendarEvent> DownloadedCals = await eachTask.ConfigureAwait(false);
                if (DownloadedCals==null)
                {
                    AllInvalids.Add(AllControls[i]);
                }
                else 
                {
                    AllCalEvents.AddRange(DownloadedCals);
                }
                
                i++;
            }

            RetrievedGoogleControls = new GoogleThirdPartyControl(AllCalEvents);

            Tuple<List<GoogleTilerEventControl>, GoogleThirdPartyControl> RetValue = new Tuple<List<GoogleTilerEventControl>, GoogleThirdPartyControl>(AllInvalids, RetrievedGoogleControls);
            return RetValue;
        }


        IList<Event> getAllGoogleEvents(TimeLine timeLine, HashSet<string> excldeIds = null)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow.removeSecondsAndMilliseconds();
            ConcurrentBag<Event> retValueBag = new ConcurrentBag<Event>();

            List<string> invalidCalendarIds = new List<string>() { "SkedPal", "Family", "Holidays in United States" };

            HashSet<string> defaultRemovedIds = new HashSet<string>(invalidCalendarIds.Select(caleventIdSummary => caleventIdSummary.ToLower()));
            var allCalendars = CalendarServiceInfo.CalendarList;

            var calendarList = allCalendars.List().Execute();

            excldeIds = excldeIds ?? new HashSet<string>();

            List<Task<Events>> allCalendarEVents = new List<Task<Events>>();

            foreach (var calendar in calendarList.Items)
            {
                if(!excldeIds.Contains(calendar.Id) && !defaultRemovedIds.Contains(calendar.Summary.ToLower()))
                {
                    var eventTask = getGoogleEvents(timeLine, calendar.Id);
                    allCalendarEVents.Add(eventTask);
                }
                
            }

            foreach(var eventsTask in allCalendarEVents)
            {
                eventsTask.Wait();
                if (eventsTask.Result != null)
                {
                    var calendar = eventsTask.Result;
                    string accessrole = calendar.AccessRole;
                    string readerString = "reader";
                    string freeBusyReaderString = "freeBusyReader";
                    bool isReadonly = (!string.IsNullOrEmpty(accessrole) && !string.IsNullOrWhiteSpace(accessrole) 
                        && (accessrole.Contains(readerString) || accessrole.Contains(freeBusyReaderString)));
                    foreach (var googleEvent in eventsTask.Result.Items)
                    {
                        if(googleEvent.Start.DateTime!=null )// this is needed because of single day reminders. Think birthdays reminders and all day reminders. These don't have a date time and temporarily don have to be part of the schedule analysis
                        {
                            DateTimeOffset start = new DateTimeOffset((DateTime)googleEvent.Start.DateTime);
                            if (googleEvent.ExtendedProperties == null)
                            {
                                googleEvent.ExtendedProperties = new Event.ExtendedPropertiesData();
                                googleEvent.ExtendedProperties.Private__ = new Dictionary<string, string>();
                            }
                            if (googleEvent.ExtendedProperties.Private__ != null)
                            {
                                googleEvent.ExtendedProperties.Private__[tilerReadonlyKey] = isReadonly.ToString();
                            }
                            retValueBag.Add(googleEvent);
                        }
                    }
                }
            }
            IList<Event> retValue = retValueBag.ToList();
            return retValue;
        }

        async Task<Events> getGoogleEvents(TimeLine timeLine, string calendarId)
        {
            
            Events RetValue = null;
            bool tryTokenRefresh = false;
            bool tryUncommitAuthentication = false;
            try
            {
                var PrepList = CalendarServiceInfo.Events.List(calendarId);
                
                timeLine = timeLine ?? (timeLine = new TimeLine(DateTime.UtcNow.AddDays(Utility.defaultBeginDay), DateTime.UtcNow.AddDays(Utility.defaultEndDay)));
                PrepList.TimeMin = timeLine.Start.DateTime;
                PrepList.TimeMax = timeLine.End.DateTime;
                PrepList.SingleEvents = true;// this ensures that repeat events ge expanded
                PrepList.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                
                //RetValue = await PrepList.ExecuteAsync().ConfigureAwait(false);
                RetValue = PrepList.Execute();



            }
            catch (Exception e)
            {
                tryTokenRefresh = true;
            }

            if(tryTokenRefresh)
            {
                try
                {
                    if(await AuthenticationInfo.refreshAndCommitToken(db).ConfigureAwait(false))
                    {
                        RetValue =await getGoogleEvents(timeLine, calendarId).ConfigureAwait(false);
                    }
                    else 
                    {
                        tryUncommitAuthentication = true;
                    }
                }
                catch (Exception e)
                {
                    tryUncommitAuthentication = true;
                }
            }

            if (tryUncommitAuthentication)
            {
                try
                {
                    await AuthenticationInfo.unCommitAuthentication().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    
                }
            }

            return RetValue;
        }

        async public Task<List<CalendarEvent>> getCalendarEventsForIndex(TimeLine CalculationTimeLine, bool retrieveGoogleLocation)
        {

            IList<Event> list = getAllGoogleEvents(CalculationTimeLine);
            List<CalendarEvent> RetValue = new List<CalendarEvent>();
            if(list.Count < 1)
            {
                return RetValue;
            }
            
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(((TilerFront.Models.IndexedThirdPartyAuthentication)AuthenticationInfo).ThirdPartyIndex.ToString());
            RetValue = (await GoogleCalExtension.getAllCalEvents(list, CalendarServiceInfo, EmailID, googleAuthenticationID, CalculationTimeLine, retrieveGoogleLocation, AuthenticationInfo.DefaultLocation).ConfigureAwait(false)).ToList();
            
            return RetValue;
        }

        async public Task<List<CalendarEvent>> getCalendarEvents(TimeLine calculationTimeLine, bool retrieveGoogleLocation, uint calIndex = 0)
        {
            //var PrepList = CalendarServiceInfo.Events.List(EmailID);

            List<CalendarEvent> RetValue =new List<CalendarEvent>();
            var list = getAllGoogleEvents(calculationTimeLine);
            if(list.Count > 0)
            {   
                EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(calIndex.ToString());
                RetValue = (await GoogleCalExtension.getAllCalEvents(list, CalendarServiceInfo, EmailID, googleAuthenticationID, calculationTimeLine, retrieveGoogleLocation, AuthenticationInfo.DefaultLocation).ConfigureAwait(false)).ToList();
            }
            return RetValue;
        }

        async public Task deleteSubEvent(TilerFront.Models.getEventModel mySubEvent)
        {
            await CalendarServiceInfo.Events.Delete(EmailID, mySubEvent.ThirdPartyEventID).ExecuteAsync().ConfigureAwait(false);
        }

        async public Task deleteSubEvents(IEnumerable<string> eventIds)
        {
            List<Task> deletionTasks = new List<Task>();
            foreach(string eventId in eventIds)
            {
                var deletionTask = CalendarServiceInfo.Events.Delete(EmailID, eventId).ExecuteAsync();
                deletionTasks.Add(deletionTask);
            }

            foreach(Task task in deletionTasks)
            {
                await task.ConfigureAwait(false);
            }
        }

        async public Task updateSubEvent(TilerFront.Models.EditCalEventModel mySubEvent)
        {
            Event googleEvent = CalendarServiceInfo.Events.Get(EmailID,mySubEvent.ThirdPartyEventID).Execute();
            EventDateTime googleStart = googleEvent.Start;
            EventDateTime googleEnd = googleEvent.End;

            googleEvent.Summary = mySubEvent.EventName;

            //googleStart.Date = mySubEvent.getStart().Date.ToShortDateString();
            googleStart.DateTime = mySubEvent.getStart().DateTime;
            //googleStart.DateTime = mySubEvent.getStart().DateTime;
            //googleStart.DateTimeRaw = mySubEvent.getStart().DateTime.ToString("r");
            googleStart.DateTimeRaw = XmlConvert.ToString(mySubEvent.getStart().DateTime, XmlDateTimeSerializationMode.Utc);

            //googleEnd.Date = mySubEvent.getEnd().Date.ToShortDateString();
            googleEnd.DateTime = mySubEvent.getEnd().DateTime;
            //googleEnd.DateTime = mySubEvent.getEnd().DateTime;
            //googleEnd.DateTimeRaw = mySubEvent.getEnd().DateTime.ToString("r");
            googleEnd.DateTimeRaw = XmlConvert.ToString(mySubEvent.getEnd().DateTime, XmlDateTimeSerializationMode.Utc);
            try
            {
                

                CalendarServiceInfo.Events.Update(googleEvent, EmailID, mySubEvent.ThirdPartyEventID).Execute();
            }
            catch(Exception e)
            {
                ;
            }
            
        }

        public TilerFront.Models.ThirdPartyCalendarAuthenticationModel getDBAuthenticationData()
        {
            return AuthenticationInfo;
        }

        static async public Task<ConcurrentBag<CalendarEvent>> getAllCalEvents(IEnumerable<GoogleTilerEventControl>AllGoogleCalControl, TimeLine CalculationTimeLine, bool retrieveLocationFromGoogle = false)
        {
            ConcurrentBag<List<CalendarEvent>> RetValueListContainer = new System.Collections.Concurrent.ConcurrentBag<List<CalendarEvent>>();
            ConcurrentBag<Task<List<CalendarEvent>>> ConcurrentTask = new System.Collections.Concurrent.ConcurrentBag<System.Threading.Tasks.Task<List<TilerElements.CalendarEvent>>>();
            ConcurrentBag<CalendarEvent> RetValue = new System.Collections.Concurrent.ConcurrentBag<CalendarEvent>();
            //AllGoogleCalControl.AsParallel().ForAll(obj=>

            foreach(GoogleTilerEventControl obj in AllGoogleCalControl)
            {
                ConcurrentTask.Add(obj.getCalendarEventsForIndex(CalculationTimeLine, false));
            }
            //);

            /*
            Parallel.ForEach(ConcurrentTask, async EachTask =>
                {
                    List<CalendarEvent> ALlCalEvents = await EachTask.ConfigureAwait(false);
                    ALlCalEvents.ForEach(obj1 => RetValue.Add(obj1));
                }
                );
            */

            
            foreach(Task<List<CalendarEvent>> EachTask in ConcurrentTask)
            {
                List<CalendarEvent> ALlCalEvents =await EachTask.ConfigureAwait(false);
                ALlCalEvents.ForEach(obj1=>RetValue.Add(obj1));
            }

            return RetValue;
        }




#region 
        public string CalendarUserID
        {
            get
            {
                return EmailID;
            }
        }
#endregion



    }

}
