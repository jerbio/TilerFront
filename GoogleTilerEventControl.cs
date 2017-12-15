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
        string EmailID;

        public GoogleTilerEventControl(TilerFront.Models.ThirdPartyCalendarAuthenticationModel AuthenticationCredential, ApplicationDbContext db)
        {
            CalendarServiceInfo = new Google.Apis.Calendar.v3.CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = AuthenticationCredential.getGoogleOauthCredentials(),
                ApplicationName = "Tiler Web App"
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
            Events list = await getGoogleEvents().ConfigureAwait(false);
            if(list==null)
            {
                return RetValue;
            }
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(((TilerFront.Models.IndexedThirdPartyAuthentication)AuthenticationInfo).ThirdPartyIndex);

            List<CalendarEvent> RetValueList = (await GoogleCalExtension.getAllCalEvents(list.Items, CalendarServiceInfo, EmailID, googleAuthenticationID, null, false).ConfigureAwait(false)).ToList();
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

            for (int j = 0; j < AllControls.Count; j++)
            {
                GoogleTilerEventControl eachGoogleTilerEventControl = AllControls[j];
                AllConcurrentTasks.Add(eachGoogleTilerEventControl.getCalendarEvents(calucaltionTimeLine, getGoogleLocation));
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


        async Task<Events> getGoogleEvents()
        {
            
            Events RetValue = null;
            bool tryTokenRefresh = false;
            bool tryUncommitAuthentication = false;
            try
            {
                var PrepList = CalendarServiceInfo.Events.List(EmailID);
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
                        RetValue =await getGoogleEvents().ConfigureAwait(false);
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

            Events list = await getGoogleEvents().ConfigureAwait(false);
            List<CalendarEvent> RetValue = new List<CalendarEvent>();
            if(list==null)
            {
                return RetValue;
            }
            
            EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(((TilerFront.Models.IndexedThirdPartyAuthentication)AuthenticationInfo).ThirdPartyIndex);
            RetValue = (await GoogleCalExtension.getAllCalEvents(list.Items, CalendarServiceInfo, EmailID, googleAuthenticationID, CalculationTimeLine, retrieveGoogleLocation).ConfigureAwait(false)).ToList();
            
            return RetValue;
        }

        async public Task<List<CalendarEvent>> getCalendarEvents(TimeLine calculationTimeLine, bool retrieveGoogleLocation)
        {
            //var PrepList = CalendarServiceInfo.Events.List(EmailID);

            List<CalendarEvent> RetValue =new List<CalendarEvent>();
            var list = await getGoogleEvents().ConfigureAwait(false);
            if(list!=null)
            {   
                EventID googleAuthenticationID = EventID.generateGoogleAuthenticationID(0);
                RetValue = (await GoogleCalExtension.getAllCalEvents(list.Items, CalendarServiceInfo, EmailID, googleAuthenticationID, calculationTimeLine, retrieveGoogleLocation).ConfigureAwait(false)).ToList();
            }
            return RetValue;
        }

        async public Task deleteSubEvent(TilerFront.Models.getEventModel mySubEvent)
        {
            await CalendarServiceInfo.Events.Delete(EmailID, mySubEvent.ThirdPartyEventID).ExecuteAsync().ConfigureAwait(false);
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
