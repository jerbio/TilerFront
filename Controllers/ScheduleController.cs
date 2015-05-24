using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Web;
using TilerFront.Models;
using TilerElements;
using DBTilerElement;
//using TilerGoogleCalendarLib;
using System.Collections.Concurrent;
//using System.Web.Http.Cors;

namespace TilerFront.Controllers
{
    //[EnableCors(origins: "*", headers: "accept, authorization, origin", methods: "DELETE,PUT,POST,GET")]
    //[EnableCors("*", "*", "*",)]
    /// <summary>
    /// Represents a users schedule. Provides access to schedule creation, modification and deletion
    /// </summary>
    public class ScheduleController : ApiController
    {
        
        // GET api/schedule
        /// <summary>
        /// Retrieve Events within a time frame. Required elements are UserID and UserName. Provided starttime and Endtime for the range of the schedule allows for retrieval of schedule within a timerange
        /// </summary>
        /// <param name="myAuthorizedUser"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> GetSchedule([FromUri] getScheduleModel myAuthorizedUser)
        {
            UserAccountDirect myUserAccount = await myAuthorizedUser.getUserAccountDirect();
            HttpContext myCOntext = HttpContext.Current;
            await myUserAccount.Login();
            PostBackData returnPostBack;
            if (myUserAccount.Status)
            {
                DateTimeOffset StartTime = new DateTimeOffset(myAuthorizedUser.StartRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTImeSpan);
                DateTimeOffset EndTime = new DateTimeOffset(myAuthorizedUser.EndRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTImeSpan);
                TimeLine TimelineForData = new TimeLine(StartTime.AddYears(-100), EndTime.AddYears(100));
                

                LogControl LogAccess = myUserAccount.ScheduleLogControl;
                List<IndexedThirdPartyAuthentication> AllIndexedThirdParty = await getAllThirdPartyAuthentication(myUserAccount.UserID).ConfigureAwait(false);

                List<GoogleTilerEventControl> AllGoogleTilerEvents = AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl(obj)).ToList();
                //AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl(obj)).ToList();
                foreach (IndexedThirdPartyAuthentication obj in AllIndexedThirdParty)
                {
                    var GoogleTilerEventControlobj = new GoogleTilerEventControl(obj);
                }
                
                
                



                //List<Task<List<CalendarEvent>>> getAllCalTasks = AllGoogleTilerEvents.Select(obj => obj.getCalendarEvents()).ToList();

                List<CalendarEvent> ScheduleData = new List<CalendarEvent>();

                Task<ConcurrentBag<CalendarEvent>> GoogleCalEventsTask =  GoogleTilerEventControl.getAllCalEvents(AllGoogleTilerEvents);

                Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>> ProfileData =await LogAccess.getProfileInfo(TimelineForData);

                IEnumerable<CalendarEvent> GoogleCalEvents = await GoogleCalEventsTask.ConfigureAwait(false);

                ScheduleData = ScheduleData.Concat(ProfileData.Item1.Values.Where(obj => obj.isActive)).ToList();

                ScheduleData = ScheduleData.Concat(GoogleCalEvents).ToList();
                IEnumerable<CalendarEvent> NonRepeatingEvents = ScheduleData.Where(obj => !obj.RepetitionStatus);

                


                //IEnumerable<CalendarEvent> RepeatingEvents = ScheduleData.Where(obj => obj.RepetitionStatus).SelectMany(obj => obj.Repeat.RecurringCalendarEvents);
                IList<UserSchedule.repeatedEventData> RepeatingEvents = ScheduleData.AsParallel().Where(obj => obj.RepetitionStatus).
                    Select(obj => new UserSchedule.repeatedEventData 
                        { 
                            ID = obj.Calendar_EventID.ToString(), 
                            Latitude = obj.myLocation.XCoordinate, 
                            Longitude = obj.myLocation.YCoordinate, 
                            RepeatAddress = obj.myLocation.Address, 
                            RepeatAddressDescription = obj.myLocation.Description, 
                            RepeatCalendarName = obj.Name, 
                            RepeatCalendarEvents = obj.Repeat.RecurringCalendarEvents().AsParallel().
                                Select(obj1 => obj1.ToCalEvent(TimelineForData)).ToList(),
                            RepeatEndDate = obj.End,
                            RepeatStartDate = obj.Start,
                            RepeatTotalDuration = obj.ActiveDuration 
                        }).ToList();

                
                UserSchedule currUserSchedule = new UserSchedule { NonRepeatCalendarEvent = NonRepeatingEvents.Select(obj => obj.ToCalEvent(TimelineForData)).ToArray(), RepeatCalendarEvent = RepeatingEvents };
                InitScheduleProfile retValue = new InitScheduleProfile { Schedule = currUserSchedule, Name = myUserAccount.Usersname };
                returnPostBack = new PostBackData(retValue, 0);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }
            
            return Ok(returnPostBack.getPostBack);
        }
        
        /// <summary>
        /// Retrieves the third party authentication credentials needed to retrieve third party calendar. Attaches an index for multiple calendar retrieval. Based on a specific userID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        static async Task<List<IndexedThirdPartyAuthentication>> getAllThirdPartyAuthentication(string ID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            List<ThirdPartyCalendarAuthenticationModel> AllAuthentications = ( db.ThirdPartyAuthentication.Where(obj => ID == obj.TilerID).ToList());
            List<IndexedThirdPartyAuthentication> RetValue = AllAuthentications.Select((obj, i) => new IndexedThirdPartyAuthentication(obj,(uint) i)).ToList();
            return RetValue;
        }


        /// <summary>
        /// Retrieves the third party authentication credentials needed to retrieve third party calendar. Gets a specific calendar authentication. You need full credentials to get specific oauth credentials
        /// </summary>
        /// <param name="TilerUserID"></param>
        /// <param name="ThirdpaartyUserID"></param>
        /// <param name="ThirdPartyType"></param>
        /// <returns></returns>
        async static public Task<ThirdPartyCalendarAuthenticationModel> getThirdPartyAuthentication(string TilerUserID, string ThirdpaartyUserID, int ThirdPartyType)
        {
            Object[] Param = { TilerUserID, ThirdpaartyUserID, ThirdPartyType };
            ApplicationDbContext db = new ApplicationDbContext();
            ThirdPartyCalendarAuthenticationModel RetValue = await db.ThirdPartyAuthentication.FindAsync(Param);
            return RetValue;
        }

        /// <summary>
        /// Handles the trigger when new google notification comes through.
        /// </summary>
        /// <param name="GoogleNotificationID"></param>
        /// <returns></returns>
        static async public Task googleNotificationTrigger(string GoogleNotificationID)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            ThirdPartyCalendarAuthenticationModel ThirdPartAuthData= db.ThirdPartyAuthentication.Where(obj => obj.ID == GoogleNotificationID).Single();
            object[] LookUpParams = {ThirdPartAuthData.TilerID};
            ApplicationUser myUser = db.Users.Find(LookUpParams);
            await notificationTrigger(myUser).ConfigureAwait(false);
        }


        static async Task notificationTrigger(ApplicationUser TilerUser)
        {
            UserAccountDirect RetrievedUSer = new UserAccountDirect(TilerUser, true);
            ApplicationDbContext db = new ApplicationDbContext();
            //DateTimeOffset CurrentTime = DateTimeOffset.Parse("5/8/2015 5:35:00 AM +00:00");//.AddDays(-1);// DateTimeOffset.UtcNow.AddDays(-1);
            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;//.AddDays(-1);
            My24HourTimerWPF.Schedule TilerSchedule = new My24HourTimerWPF.Schedule(RetrievedUSer, CurrentTime);
            await updatemyScheduleWithGoogleThirdpartyCalendar(TilerSchedule, RetrievedUSer.UserID).ConfigureAwait(false);
            await TilerSchedule.UpdateScheduleDueToExternalChanges().ConfigureAwait(false);
        }


        // GET api/schedule/5
        [NonAction]
        public async Task<IHttpActionResult> GetScheduleById([FromBody]AuthorizedUser myAuthorizedUser)
        {
            return Ok("return");
        }


        /// <summary>
        /// Function retrives the third party credential. Credential are used to retrieve calendar event. The retrieved events are used to populate mySchedule
        /// </summary>
        /// <param name="mySchedule"></param>
        /// <param name="TilerUserID"></param>
        /// <returns></returns>
        static internal async Task updatemyScheduleWithGoogleThirdpartyCalendar(My24HourTimerWPF.Schedule mySchedule, string TilerUserID)
        {
            List<IndexedThirdPartyAuthentication> AllIndexedThirdParty = await getAllThirdPartyAuthentication(TilerUserID).ConfigureAwait(false);
            List<GoogleTilerEventControl> AllGoogleTilerEvents = AllIndexedThirdParty.Select(obj => new GoogleTilerEventControl (obj)).ToList();

            Tuple<List<GoogleTilerEventControl>, GoogleThirdPartyControl> GoogleEvents = await GoogleTilerEventControl.getThirdPartyControlForIndex(AllGoogleTilerEvents).ConfigureAwait(false);
            Task DeleteInvalidAuthentication = ManageController.delelteGoogleAuthentication(GoogleEvents.Item1.Select(obj => obj.getDBAuthenticationData()));
            mySchedule.updateDataSetWithThirdPartyData(GoogleEvents.Item2);
            await DeleteInvalidAuthentication.ConfigureAwait(false);
        }


        /// <summary>
        /// Modify user schedule. It clears out time frame. Procrastinates all Subcalendar events, essentially freeing up sometime.
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/ProcrastinateAll")]
        public async Task<IHttpActionResult> ProcrastinateAll([FromBody]ProcrastinateModel UserData)
        {

            AuthorizedUser myAuthorizedUser = UserData.User;
            
            TimeDuration ProcrastinateDuration = UserData.ProcrastinateDuration;
            TimeSpan fullTimeSpan = myAuthorizedUser.getTImeSpan;
            UserAccountDirect myUserAccount = await UserData.getUserAccountDirect();
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUserAccount, DateTimeOffset.UtcNow);

            await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, UserData.UserID).ConfigureAwait(false);




            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.ProcrastinateAll(ProcrastinateDuration.TotalTimeSpan);
            await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }

        /// <summary>
        /// Procrastinate a given event. 
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Procrastinate")]
        public async Task<IHttpActionResult> ProcrastinateSubCalendarEvent([FromBody]ProcrastinateEventModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            TimeDuration ProcrastinateDuration = UserData.ProcrastinateDuration;
            TimeSpan fullTimeSpan = myAuthorizedUser.getTImeSpan;
            UserAccountDirect myUser = await UserData.getUserAccountDirect();
            await myUser.Login();

            DateTimeOffset myNow = DateTimeOffset.Parse("5/5/2015 2:45:00 PM");
            myNow = DateTimeOffset.UtcNow;// myAuthorizedUser.getRefNow();
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser,myNow);

            await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, UserData.UserID).ConfigureAwait(false);

            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.ProcrastinateJustAnEvent(UserData.EventID, ProcrastinateDuration.TotalTimeSpan);
            await MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }


        /// <summary>
        /// Marks an Event as complete.  Note this also triggers a readjust to the schedule
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvent([FromBody]getEventModel UserData)
        {
            UserAccountDirect retrievedUser = await UserData.getUserAccountDirect();
            await retrievedUser.Login();
            PostBackData retValue = new PostBackData("", 1);
            if (retrievedUser.Status)
            {
                string CalendarType = UserData.ThirdPartyType.ToLower();

                switch (CalendarType)
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, UserData.ThirdPartyUserID, 2);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty);
                            await googleControl.deleteSubEvent(UserData).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    case "tiler":
                        {
                            DateTimeOffset myNow = DateTimeOffset.UtcNow;
                            //myNOw = UserData.getRefNow();
                            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, myNow );
                            await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, UserData.UserID).ConfigureAwait(false);

                            MySchedule.markSubEventAsCompleteCalendarEventAndReadjust(UserData.EventID);
                            retValue = new PostBackData("\"Success\"", 0);
                        }
                        break;
                    default:
                        break;
                }
            }
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Marks a series of Event as complete. It takes a collection of IDs and then marks them as complete
        /// </summary>
        /// <param name="UserData"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvents([FromBody]getEventModel UserData)
        {
            UserAccountDirect myUser = await UserData.getUserAccountDirect();
            await myUser.Login();
            DateTimeOffset myNow = DateTimeOffset.UtcNow;
            //myNow = UserData.getRefNow();
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, myNow);
            await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, UserData.UserID).ConfigureAwait(false);

            IEnumerable<string> AllEVentIDs = UserData.EventID.Split(',');
            MySchedule.markSubEventsAsComplete(AllEVentIDs);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }


        /// <summary>
        /// Sets the provided event as now. The ID has to be a Sub event ID
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Now")]
        public async Task<IHttpActionResult> Now([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();// new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                DateTimeOffset myNow = DateTimeOffset.UtcNow;
                //myNow = UserData.getRefNow();

                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, myNow);

                await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, myUser.UserID).ConfigureAwait(false);

                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue0 = MySchedule.SetEventAsNow(myUser.EventID, true);
                await MySchedule.UpdateWithProcrastinateSchedule(retValue0.Item2);
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Undoes the last schedule changing effect triggered on tiler.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Undo")]
        public async Task<IHttpActionResult> Undo([FromBody]AuthorizedUser myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();// new UserAccountDirect(myUser.UserName, myUser.UserID);
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                retrievedUser.ScheduleLogControl.Undo();
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }

        

        /// <summary>
        /// Deletes a Sub event from Tiler. The deletion does not trigger a schedule readjustment.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> DeleteEvent([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();
            await retrievedUser.Login();
            PostBackData retValue= new PostBackData("", 1);
            if (retrievedUser.Status)
            {
                string CalendarType = myUser.ThirdPartyType.ToLower();

                switch(CalendarType )
                {
                    case "google":
                        {
                            Models.ThirdPartyCalendarAuthenticationModel AllIndexedThirdParty = await getThirdPartyAuthentication(retrievedUser.UserID, myUser.ThirdPartyUserID, 2);
                            GoogleTilerEventControl googleControl = new GoogleTilerEventControl(AllIndexedThirdParty);
                            await googleControl.deleteSubEvent(myUser).ConfigureAwait(false);
                            retValue = new PostBackData("\"Success\"", 0);   
                        }
                        break;
                    case "tiler":
                        {
                            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                            
                            await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, myUser.UserID).ConfigureAwait(false);

                            MySchedule.deleteSubCalendarEvent(myUser.EventID);
                            retValue = new PostBackData("\"Success\"", 0);   
                        }
                        break;
                    default:
                        break;
                }


                
            }
            
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Option needed to satisfy a delete request for deletion of a subevent.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> HandleOptionsEvent([FromBody]getEventModel myUser)
        {
            return Ok();
        }

        /// <summary>
        /// Deletes multiple subevents.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events")]
        public async Task<IHttpActionResult> DeleteEvents([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, myUser.getRefNow());
                await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, myUser.UserID).ConfigureAwait(false);
                IEnumerable<string> AllEVentIDs = myUser.EventID.Split(',');
                MySchedule.deleteSubCalendarEvents(AllEVentIDs);
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }


        /// <summary>
        /// Option needed to satisfy a delete request for multiple subevents.
        /// </summary>
        /// <param name="myUser"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events")]
        public async Task<IHttpActionResult> HandleOptionsEvents([FromBody]getEventModel myUser)
        {
            return Ok();
        }


        /// <summary>
        /// Adds an Event to Tiler. Returns data formatted as tiler endpoint together. Returns the earliest sub calendarevent generated. 
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> NewCalEvent([FromBody]UnregisteredEvent newEvent)//, [FromUri]AuthorizedUser userEntry)
        {
            string BColor = newEvent.BColor;
            string RColor = newEvent.RColor;
            string GColor = newEvent.GColor;
            string Opacity = newEvent.Opacity;
            string ColorSelection = newEvent.ColorSelection;
            string Count = newEvent.Count; ;
            string DurationDays = newEvent.DurationDays; ;
            string DurationHours = newEvent.DurationHours; ;
            string DurationMins = newEvent.DurationMins; ;
            string EndDay = newEvent.EndDay; ;
            string EndHour = newEvent.EndHour; ;
            string EndMins = newEvent.EndMins; ;
            string EndMonth = newEvent.EndMonth; ;
            string EndYear = newEvent.EndYear; ;

            string LocationAddress = newEvent.LocationAddress; ;
            string LocationTag = newEvent.LocationTag; ;
            string Name = newEvent.Name; ;

            string RepeatData = newEvent.RepeatData; ;
            string RepeatEndDay = newEvent.RepeatEndDay; ;
            string RepeatEndMonth = newEvent.RepeatEndMonth; ;
            string RepeatEndYear = newEvent.RepeatEndYear; ;
            string RepeatStartDay = newEvent.RepeatStartDay; ;
            string RepeatStartMonth = newEvent.RepeatStartMonth; ;
            string RepeatStartYear = newEvent.RepeatStartYear; ;
            string RepeatType = newEvent.RepeatType; ;
            string RepeatWeeklyData = newEvent.RepeatWeeklyData; ;
            string Rigid = newEvent.Rigid; ;
            string StartDay = newEvent.StartDay; ;
            string StartHour = newEvent.StartHour; ;
            string StartMins = newEvent.StartMins; ;
            string StartMonth = newEvent.StartMonth; ;
            string StartYear = newEvent.StartYear; ;
            string RepeatFrequency = newEvent.RepeatFrequency; ;


            string restrictionPreference = newEvent.isRestricted;

            bool restrictionFlag = Convert.ToBoolean(restrictionPreference);

            string StartTime = StartHour + ":" + StartMins;
            string EndTime = EndHour + ":" + EndMins;

            DateTimeOffset StartDateEntry = new DateTimeOffset(Convert.ToInt32(StartYear), Convert.ToInt32(StartMonth), Convert.ToInt32(StartDay), 0, 0, 0, new TimeSpan());
            DateTimeOffset EndDateEntry = new DateTimeOffset(Convert.ToInt32(EndYear), Convert.ToInt32(EndMonth), Convert.ToInt32(EndDay), 0, 0, 0, new TimeSpan());

            TimeSpan fullTimeSpan = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
            string EventDuration = TimeSpan.FromSeconds(fullTimeSpan.TotalSeconds * Convert.ToInt32(Count)).ToString();

            bool RigidScheduleFlag = Convert.ToBoolean(Rigid);
            TilerElements.Location EventLocation = new TilerElements.Location(LocationAddress, LocationTag);

            Repetition MyRepetition = new Repetition();
            DateTimeOffset RepeatStart = new DateTimeOffset();
            DateTimeOffset RepeatEnd = new DateTimeOffset();
            bool RepetitionFlag = false;
            TilerColor userColor = new TilerColor(Convert.ToInt32(RColor), Convert.ToInt32(GColor), Convert.ToInt32(BColor), Convert.ToInt32(Opacity), Convert.ToInt32(ColorSelection));

            if (RigidScheduleFlag)//this needs to be called after the initialization of restrictionFlag
            {
                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);
                FullStartTime = FullStartTime.Add(newEvent.getTImeSpan);
                FullEndTime = FullEndTime.Add(newEvent.getTImeSpan);
                EventDuration = (FullEndTime - FullStartTime).ToString();
                restrictionFlag = false;
            }

            

            if (!string.IsNullOrEmpty(RepeatType))
            {
                

                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                FullStartTime = FullStartTime.Add(newEvent.getTImeSpan);
                FullEndTime = FullEndTime.Add(newEvent.getTImeSpan);

                RepeatEnd = new DateTimeOffset(Convert.ToInt32(RepeatEndYear), Convert.ToInt32(RepeatEndMonth), Convert.ToInt32(RepeatEndDay), 23, 59, 0, new TimeSpan());
                RepeatEnd = RepeatEnd.Add(newEvent.getTImeSpan);
                if (!RigidScheduleFlag)
                {
                    DateTimeOffset newEndTime = FullEndTime;

                    string Frequency = RepeatFrequency.Trim().ToUpper();
                    switch(Frequency)
                    {
                        case "DAILY":
                            FullEndTime = FullStartTime.AddDays(1);
                            break;
                        case "WEEKLY":
                            FullEndTime = FullStartTime.AddDays(7);
                            break;
                        case "MONTHLY":
                            FullEndTime = FullStartTime.AddMonths(1);
                            break;
                        case "YEARLY":
                            FullEndTime = FullStartTime.AddYears(1);
                            break;
                        default:
                            break;
                    }

                    RepeatEnd = newEndTime;
                }


                

                RepeatStart = StartDateEntry;
                int[] selectedDaysOftheweek={};
                RepeatWeeklyData = string.IsNullOrEmpty( RepeatWeeklyData )?"":RepeatWeeklyData.Trim();
                if (!string.IsNullOrEmpty(RepeatWeeklyData))
                {
                    selectedDaysOftheweek = RepeatWeeklyData.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => Convert.ToInt32(obj)).ToArray();
                }

                
                //RepeatEnd = (DateTimeOffset.Now).AddDays(7);
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency, new TimeLine(FullStartTime, FullEndTime), selectedDaysOftheweek);
                EndDateEntry = RepeatEnd;
            }

            

            UserAccountDirect myUser = await newEvent.getUserAccountDirect();
            PostBackData retValue;
            await myUser.Login();

            Task HoldUpForWriteNewEvent;
            Task CommitChangesToSchedule;
            if (myUser.Status)
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                myNow = DateTimeOffset.UtcNow;
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, myNow);

                await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, myUser.UserID).ConfigureAwait(false);

                CalendarEvent newCalendarEvent;
                if(restrictionFlag )
                {
                    string TimeString = StartDateEntry.Date.ToShortDateString() + " " + StartTime;
                    DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime; ;
                    StartDateTime = StartDateTime.Add(newEvent.getTImeSpan);
                    TimeString = EndDateEntry.Date.ToShortDateString() + " " + EndTime;
                    DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    EndDateTime = EndDateTime.Add(newEvent.getTImeSpan);


                    RestrictionProfile myRestrictionProfile = CreateRestrictionProfile(newEvent.RestrictionStart, newEvent.RestrictionEnd, newEvent.isWorkWeek, newEvent.getTImeSpan);
                    newCalendarEvent = new CalendarEventRestricted(Name, StartDateTime, EndDateTime, myRestrictionProfile, TimeSpan.Parse(EventDuration), MyRepetition, false, true, Convert.ToInt32(Count), RigidScheduleFlag, new Location_Elements(), new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData());
                }
                else
                {
                    DateTimeOffset StartData = DateTimeOffset.Parse(StartTime+" "+StartDateEntry.Date.ToShortDateString()).UtcDateTime;
                    StartData = StartData.Add(newEvent.getTImeSpan);
                    DateTimeOffset EndData = DateTimeOffset.Parse(EndTime + " " + EndDateEntry.Date.ToShortDateString()).UtcDateTime;
                    EndData = EndData.Add(newEvent.getTImeSpan);
                    newCalendarEvent = new CalendarEvent(Name, StartData, EndData, Count, "", EventDuration, MyRepetition, true, RigidScheduleFlag, "", true, EventLocation, true, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), false);
                }

                
                newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent);
                string BeforemyName = newCalendarEvent.ToString(); //BColor + " -- " + Count + " -- " + DurationDays + " -- " + DurationHours + " -- " + DurationMins + " -- " + EndDay + " -- " + EndHour + " -- " + EndMins + " -- " + EndMonth + " -- " + EndYear + " -- " + GColor + " -- " + LocationAddress + " -- " + LocationTag + " -- " + Name + " -- " + RColor + " -- " + RepeatData + " -- " + RepeatEndDay + " -- " + RepeatEndMonth + " -- " + RepeatEndYear + " -- " + RepeatStartDay + " -- " + RepeatStartMonth + " -- " + RepeatStartYear + " -- " + RepeatType + " -- " + RepeatWeeklyData + " -- " + Rigid + " -- " + StartDay + " -- " + StartHour + " -- " + StartMins + " -- " + StartMonth + " -- " + StartYear;
                string AftermyName = newCalendarEvent.ToString();
#if ForceReadFromXml
#else
                if (LogControl.useCassandra)
                {
                    CassandraUserLog.CassandraLog quickInsert = new CassandraUserLog.CassandraLog(myUser.UserID);
                    Dictionary<string, CalendarEvent> myDict = new Dictionary<string, CalendarEvent>();
                    MySchedule.AddToSchedule(newCalendarEvent);
                    HoldUpForWriteNewEvent = myUser.AddNewEventToLog(newCalendarEvent);
                    CommitChangesToSchedule = myUser.CommitEventToLog(MySchedule.getAllCalendarEvents(), MySchedule.LastScheduleIDNumber.ToString());
                    await HoldUpForWriteNewEvent;
                    await CommitChangesToSchedule;
                }
                else
#endif
                {
                    await MySchedule.AddToScheduleAndCommit(newCalendarEvent);
                }


                CustomErrors userError = newCalendarEvent.Error;
                retValue = new PostBackData(newCalendarEvent.ActiveSubEvents.First().ToSubCalEvent(newCalendarEvent), userError.Code);
                
            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            return Ok(retValue.getPostBack);
        }



        /// <summary>
        /// Triggers a schedule update when a notification is received pertaining third party event modification
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Notification")]
        public async Task<IHttpActionResult> Notification([FromBody]AuthorizedUser newEvent)
        {
            PostBackData retValue = new PostBackData("", 1);
            try
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                myNow = DateTimeOffset.UtcNow;
                UserAccount RetrievedUser = await newEvent.getUserAccountDirect().ConfigureAwait(false);
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(RetrievedUser, myNow);
                await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, RetrievedUser.UserID).ConfigureAwait(false);
                await MySchedule.UpdateScheduleDueToExternalChanges().ConfigureAwait(false);
                retValue = new PostBackData("\"Success\"", 0);
            }
            catch 
            {
                ;
            }
            return Ok(retValue.getPostBack);
            
        }


        /// <summary>
        /// Peeks into user schedule. Generates an update to a schedule for any changes.
        /// </summary>
        /// <param name="newEvent"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Peek")]
        public async Task<IHttpActionResult> PeekCalEvent([FromBody]UnregisteredEvent newEvent)
        {
            string BColor = newEvent.BColor;
            string RColor = newEvent.RColor;
            string GColor = newEvent.GColor;
            string Opacity = newEvent.Opacity;
            string ColorSelection = newEvent.ColorSelection;
            string Count = newEvent.Count; ;
            string DurationDays = newEvent.DurationDays; ;
            string DurationHours = newEvent.DurationHours; ;
            string DurationMins = newEvent.DurationMins; ;
            string EndDay = newEvent.EndDay; ;
            string EndHour = newEvent.EndHour; ;
            string EndMins = newEvent.EndMins; ;
            string EndMonth = newEvent.EndMonth; ;
            string EndYear = newEvent.EndYear; ;

            string LocationAddress = newEvent.LocationAddress; ;
            string LocationTag = newEvent.LocationTag; ;
            string Name = newEvent.Name; ;

            string RepeatData = newEvent.RepeatData; ;
            string RepeatEndDay = newEvent.RepeatEndDay; ;
            string RepeatEndMonth = newEvent.RepeatEndMonth; ;
            string RepeatEndYear = newEvent.RepeatEndYear; ;
            string RepeatStartDay = newEvent.RepeatStartDay; ;
            string RepeatStartMonth = newEvent.RepeatStartMonth; ;
            string RepeatStartYear = newEvent.RepeatStartYear; ;
            string RepeatType = newEvent.RepeatType; ;
            string RepeatWeeklyData = newEvent.RepeatWeeklyData; ;
            string Rigid = newEvent.Rigid; ;
            string StartDay = newEvent.StartDay; ;
            string StartHour = newEvent.StartHour; ;
            string StartMins = newEvent.StartMins; ;
            string StartMonth = newEvent.StartMonth; ;
            string StartYear = newEvent.StartYear; ;
            string RepeatFrequency = newEvent.RepeatFrequency; ;


            string restrictionPreference = newEvent.isRestricted;

            bool restrictionFlag = Convert.ToBoolean(restrictionPreference);

            string StartTime = StartHour + ":" + StartMins;
            string EndTime = EndHour + ":" + EndMins;

            DateTimeOffset StartDateEntry = new DateTimeOffset(Convert.ToInt32(StartYear), Convert.ToInt32(StartMonth), Convert.ToInt32(StartDay), 0, 0, 0, new TimeSpan());
            DateTimeOffset EndDateEntry = new DateTimeOffset(Convert.ToInt32(EndYear), Convert.ToInt32(EndMonth), Convert.ToInt32(EndDay), 0, 0, 0, new TimeSpan());

            TimeSpan fullTimeSpan = new TimeSpan(Convert.ToInt32(DurationDays), Convert.ToInt32(DurationHours), Convert.ToInt32(DurationMins), 0);
            string EventDuration = TimeSpan.FromSeconds(fullTimeSpan.TotalSeconds * Convert.ToInt32(Count)).ToString();

            bool RigidScheduleFlag = Convert.ToBoolean(Rigid);
            TilerElements.Location EventLocation = new TilerElements.Location(LocationAddress, LocationTag);

            Repetition MyRepetition = new Repetition();
            DateTimeOffset RepeatStart = new DateTimeOffset();
            DateTimeOffset RepeatEnd = new DateTimeOffset();
            bool RepetitionFlag = false;
            TilerColor userColor = new TilerColor(Convert.ToInt32(RColor), Convert.ToInt32(GColor), Convert.ToInt32(BColor), Convert.ToInt32(Opacity), Convert.ToInt32(ColorSelection));

            if (RigidScheduleFlag)
            {
                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);
                FullStartTime = FullStartTime.Add(newEvent.getTImeSpan);
                FullEndTime = FullEndTime.Add(newEvent.getTImeSpan);
                EventDuration = (FullEndTime - FullStartTime).ToString();
            }

            if (!string.IsNullOrEmpty(RepeatType))
            {

                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                FullStartTime = FullStartTime.Add(newEvent.getTImeSpan);
                FullEndTime = FullEndTime.Add(newEvent.getTImeSpan);

                RepeatEnd = new DateTimeOffset(Convert.ToInt32(RepeatEndYear), Convert.ToInt32(RepeatEndMonth), Convert.ToInt32(RepeatEndDay), 23, 59, 0, new TimeSpan());
                RepeatEnd = RepeatEnd.Add(newEvent.getTImeSpan);
                if (!RigidScheduleFlag)
                {
                    DateTimeOffset newEndTime = FullEndTime;

                    string Frequency = RepeatFrequency.Trim().ToUpper();
                    switch (Frequency)
                    {
                        case "DAILY":
                            FullEndTime = FullStartTime.AddDays(1);
                            break;
                        case "WEEKLY":
                            FullEndTime = FullStartTime.AddDays(7);
                            break;
                        case "MONTHLY":
                            FullEndTime = FullStartTime.AddMonths(1);
                            break;
                        case "YEARLY":
                            FullEndTime = FullStartTime.AddYears(1);
                            break;
                        default:
                            break;
                    }

                    RepeatEnd = newEndTime;
                }




                RepeatStart = StartDateEntry;
                int[] selectedDaysOftheweek = { };
                RepeatWeeklyData = string.IsNullOrEmpty(RepeatWeeklyData) ? "" : RepeatWeeklyData.Trim();
                if (!string.IsNullOrEmpty(RepeatWeeklyData))
                {
                    selectedDaysOftheweek = RepeatWeeklyData.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => Convert.ToInt32(obj)).ToArray();
                }


                //RepeatEnd = (DateTimeOffset.Now).AddDays(7);
                RepetitionFlag = true;
                MyRepetition = new Repetition(RepetitionFlag, new TimeLine(RepeatStart, RepeatEnd), RepeatFrequency, new TimeLine(FullStartTime, FullEndTime), selectedDaysOftheweek);
                EndDateEntry = RepeatEnd;
            }

            UserAccountDirect myUser = await newEvent.getUserAccountDirect();
            PostBackData retValue;
            await myUser.Login();

            Task HoldUpForWriteNewEvent;
            Task CommitChangesToSchedule;
            if (myUser.Status)
            {
                DateTimeOffset myNow = newEvent.getRefNow();
                myNow = DateTimeOffset.UtcNow;
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, myNow);
                await updatemyScheduleWithGoogleThirdpartyCalendar(MySchedule, myUser.UserID).ConfigureAwait(false);

                CalendarEvent newCalendarEvent;
                if (restrictionFlag)
                {
                    string TimeString = StartDateEntry.Date.ToShortDateString() + " " + StartTime;
                    DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime; ;
                    StartDateTime = StartDateTime.Add(newEvent.getTImeSpan);
                    TimeString = EndDateEntry.Date.ToShortDateString() + " " + EndTime;
                    DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString).UtcDateTime;
                    EndDateTime = EndDateTime.Add(newEvent.getTImeSpan);


                    RestrictionProfile myRestrictionProfile = CreateRestrictionProfile(newEvent.RestrictionStart, newEvent.RestrictionEnd, newEvent.isWorkWeek, newEvent.getTImeSpan);
                    newCalendarEvent = new CalendarEventRestricted(Name, StartDateTime, EndDateTime, myRestrictionProfile, TimeSpan.Parse(EventDuration), MyRepetition, false, true, Convert.ToInt32(Count), RigidScheduleFlag, new Location_Elements(), new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData());
                }
                else
                {
                    DateTimeOffset StartData = DateTimeOffset.Parse(StartTime + " " + StartDateEntry.Date.ToShortDateString()).UtcDateTime;
                    StartData = StartData.Add(newEvent.getTImeSpan);
                    DateTimeOffset EndData = DateTimeOffset.Parse(EndTime + " " + EndDateEntry.Date.ToShortDateString()).UtcDateTime;
                    EndData = EndData.Add(newEvent.getTImeSpan);
                    newCalendarEvent = new CalendarEvent(Name, StartData, EndData, Count, "", EventDuration, MyRepetition, true, RigidScheduleFlag, "", true, EventLocation, true, new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData(), false);
                }


                newCalendarEvent.Repeat.PopulateRepetitionParameters(newCalendarEvent);
                string BeforemyName = newCalendarEvent.ToString(); //BColor + " -- " + Count + " -- " + DurationDays + " -- " + DurationHours + " -- " + DurationMins + " -- " + EndDay + " -- " + EndHour + " -- " + EndMins + " -- " + EndMonth + " -- " + EndYear + " -- " + GColor + " -- " + LocationAddress + " -- " + LocationTag + " -- " + Name + " -- " + RColor + " -- " + RepeatData + " -- " + RepeatEndDay + " -- " + RepeatEndMonth + " -- " + RepeatEndYear + " -- " + RepeatStartDay + " -- " + RepeatStartMonth + " -- " + RepeatStartYear + " -- " + RepeatType + " -- " + RepeatWeeklyData + " -- " + Rigid + " -- " + StartDay + " -- " + StartHour + " -- " + StartMins + " -- " + StartMonth + " -- " + StartYear;
                string AftermyName = newCalendarEvent.ToString();
#if ForceReadFromXml
#else
                if (LogControl.useCassandra)
                {
                    CassandraUserLog.CassandraLog quickInsert = new CassandraUserLog.CassandraLog(myUser.UserID);
                    Dictionary<string, CalendarEvent> myDict = new Dictionary<string, CalendarEvent>();
                    MySchedule.AddToSchedule(newCalendarEvent);
                    HoldUpForWriteNewEvent = myUser.AddNewEventToLog(newCalendarEvent);
                    CommitChangesToSchedule = myUser.CommitEventToLog(MySchedule.getAllCalendarEvents(), MySchedule.LastScheduleIDNumber.ToString());
                    await HoldUpForWriteNewEvent;
                    await CommitChangesToSchedule;
                }
                else
#endif
                {
                    
                }
                Tuple<List<SubCalendarEvent>[], DayTimeLine[], List<SubCalendarEvent>> peekingEvents = MySchedule.peekIntoSchedule(newCalendarEvent);
                PeekResult peekData = new PeekResult(peekingEvents.Item1, peekingEvents.Item2, peekingEvents.Item3);
                
                CustomErrors userError = newCalendarEvent.Error;
                retValue = new PostBackData(peekData, userError.Code);

            }
            else
            {
                retValue = new PostBackData("", 1);
            }

            return Ok(retValue.getPostBack);
        }

        /// <summary>
        /// Creates a restriction profile to be used when generating a new event.
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <param name="workWeek"></param>
        /// <param name="TimeZoneOffSet"></param>
        /// <param name="DaySelection"></param>
        /// <returns></returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        RestrictionProfile CreateRestrictionProfile(string Start, string End,string workWeek,TimeSpan TimeZoneOffSet ,string DaySelection="")
        { 
            DateTimeOffset RestrictStart = DateTimeOffset.Parse(Start).UtcDateTime;
            RestrictStart=RestrictStart.Add(TimeZoneOffSet);
            DateTimeOffset RestrictEnd = DateTimeOffset.Parse(End).UtcDateTime;
            RestrictEnd=RestrictEnd.Add(TimeZoneOffSet);
            bool WorkWeekFlag = Convert.ToBoolean(workWeek);

            List<mTuple<bool, DayOfWeek>> allElements = (new mTuple<bool, System.DayOfWeek>[7]).ToList();
            allElements[(int)DayOfWeek.Sunday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Sunday);
            allElements[(int)DayOfWeek.Monday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Monday);
            allElements[(int)DayOfWeek.Tuesday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Tuesday);
            allElements[(int)DayOfWeek.Wednesday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Wednesday);
            allElements[(int)DayOfWeek.Thursday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Thursday);
            allElements[(int)DayOfWeek.Friday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Friday);
            allElements[(int)DayOfWeek.Saturday] = new mTuple<bool, System.DayOfWeek>(false, DayOfWeek.Saturday);


            DayOfWeek[] selectedDaysOftheweek = { };

            if (!string.IsNullOrEmpty(DaySelection))
            {
                selectedDaysOftheweek = DaySelection.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => RestrictionProfile.AllDaysOfWeek[Convert.ToInt32(obj)]).ToArray();
            }
            else 
            {
                selectedDaysOftheweek = RestrictionProfile.AllDaysOfWeek.ToArray();
            }
            RestrictionProfile retValue;
            if (WorkWeekFlag)
            {
                retValue = new RestrictionProfile(7, DayOfWeek.Monday, RestrictStart, RestrictEnd);
            }
            else
            {
                RestrictionTimeLine  RestrictionTimeLine = new TilerElements.RestrictionTimeLine(RestrictStart,RestrictEnd);
                retValue = new RestrictionProfile(selectedDaysOftheweek, RestrictionTimeLine);
            }
            return retValue;
        }
        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
