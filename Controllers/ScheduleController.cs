using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using TilerFront.Models;
using TilerElements;

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
        [ResponseType(typeof(PostBackStruct))]
        public async Task<IHttpActionResult> GetSchedule([FromUri] getScheduleModel myAuthorizedUser)
        {
            UserAccountDirect myUserAccount = await myAuthorizedUser.getUserAccountDirect();
            await myUserAccount.Login();
            PostBackData returnPostBack;
            if (myUserAccount.Status)
            {
                DateTimeOffset StartTime = new DateTimeOffset(myAuthorizedUser.StartRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTImeSpan);
                DateTimeOffset EndTime = new DateTimeOffset(myAuthorizedUser.EndRange * TimeSpan.TicksPerMillisecond, new TimeSpan()).AddYears(1969).Add(-myAuthorizedUser.getTImeSpan);
                TimeLine TimelineForData = new TimeLine(StartTime.AddYears(-100), EndTime.AddYears(100));

                LogControl LogAccess = myUserAccount.ScheduleLogControl;
                Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>> ProfileData =await LogAccess.getProfileInfo(TimelineForData);
                IEnumerable<CalendarEvent> ScheduleData = ProfileData.Item1.Values.Where(obj=>obj.isActive);
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

        // GET api/schedule/5
        [NonAction]
        public async Task<IHttpActionResult> GetScheduleById([FromBody]AuthorizedUser myAuthorizedUser)
        {
            return Ok("return");
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
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUserAccount, new DateTime(myAuthorizedUser.getRefNow().Ticks));
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.ProcrastinateAll(ProcrastinateDuration.TotalTimeSpan);
            MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }


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
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, new DateTime(myAuthorizedUser.getRefNow().Ticks));
            Tuple<CustomErrors, Dictionary<string, CalendarEvent>> ScheduleUpdateMessage = MySchedule.ProcrastinateJustAnEvent(UserData.EventID, ProcrastinateDuration.TotalTimeSpan);
            MySchedule.UpdateWithProcrastinateSchedule(ScheduleUpdateMessage.Item2);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }

        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvent([FromBody]getEventModel UserData)
        {
            UserAccountDirect myUser = await UserData.getUserAccountDirect();
            await myUser.Login();
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, new DateTime(UserData.getRefNow().Ticks));
            MySchedule.markSubEventAsCompleteCalendarEventAndReadjust(UserData.EventID);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }


        [HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events/Complete")]
        public async Task<IHttpActionResult> CompleteSubCalendarEvents([FromBody]getEventModel UserData)
        {
            UserAccountDirect myUser = await UserData.getUserAccountDirect();
            await myUser.Login();
            My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, new DateTime(UserData.getRefNow().Ticks));
            IEnumerable<string> AllEVentIDs = UserData.EventID.Split(',');
            MySchedule.markSubEventsAsComplete(AllEVentIDs);
            PostBackData myPostData = new PostBackData("\"Success\"", 0);
            return Ok(myPostData.getPostBack);
        }



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
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, new DateTime(myUser.getRefNow().Ticks));
                Tuple<CustomErrors, Dictionary<string, CalendarEvent>> retValue0 = MySchedule.SetEventAsNow(myUser.EventID, true);
                MySchedule.UpdateWithProcrastinateSchedule(retValue0.Item2);
                retValue = new PostBackData("\"Success\"", 0);
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }



        [HttpDelete]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> DeleteEvent([FromBody]getEventModel myUser)
        {
            UserAccountDirect retrievedUser = await myUser.getUserAccountDirect();
            await retrievedUser.Login();
            PostBackData retValue;
            if (retrievedUser.Status)
            {
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, new DateTime( myUser.getRefNow().Ticks));
                MySchedule.deleteSubCalendarEvent(myUser.EventID);
                retValue = new PostBackData("\"Success\"", 0);   
            }
            else
            {
                retValue = new PostBackData("", 1);
            }
            return Ok(retValue.getPostBack);
        }
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Event")]
        public async Task<IHttpActionResult> HandleOptionsEvent([FromBody]getEventModel myUser)
        {
            return Ok();
        }


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
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(retrievedUser, new DateTime(myUser.getRefNow().Ticks));
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
        [HttpOptions]
        [ResponseType(typeof(PostBackStruct))]
        [Route("api/Schedule/Events")]
        public async Task<IHttpActionResult> HandleOptionsEvents([FromBody]getEventModel myUser)
        {
            return Ok();
        }

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

            if (RigidScheduleFlag)
            {
                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                EventDuration = (FullEndTime - FullStartTime).ToString();
            }

            if (!string.IsNullOrEmpty(RepeatType))
            {

                DateTimeOffset FullStartTime = new DateTimeOffset(StartDateEntry.Year, StartDateEntry.Month, StartDateEntry.Day, Convert.ToInt32(StartTime.Split(':')[0]), Convert.ToInt32(StartTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(StartDateEntry + " " + StartTime);
                DateTimeOffset FullEndTime = new DateTimeOffset(EndDateEntry.Year, EndDateEntry.Month, EndDateEntry.Day, Convert.ToInt32(EndTime.Split(':')[0]), Convert.ToInt32(EndTime.Split(':')[1]), 0, new TimeSpan());// DateTimeOffset.Parse(EndDateEntry + " " + EndTime);

                RepeatStart = StartDateEntry;
                int[] selectedDaysOftheweek={};
                RepeatWeeklyData = string.IsNullOrEmpty( RepeatWeeklyData )?"":RepeatWeeklyData.Trim();
                if (!string.IsNullOrEmpty(RepeatWeeklyData))
                {
                    selectedDaysOftheweek = RepeatWeeklyData.Split(',').Where(obj => !String.IsNullOrEmpty(obj)).Select(obj => Convert.ToInt32(obj)).ToArray();
                }

                RepeatEnd = new DateTimeOffset(Convert.ToInt32(RepeatEndYear), Convert.ToInt32(RepeatEndMonth), Convert.ToInt32(RepeatEndDay), 0, 0, 0, new TimeSpan());
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
                My24HourTimerWPF.Schedule MySchedule = new My24HourTimerWPF.Schedule(myUser, new DateTime(newEvent.getRefNow().Ticks));
                CalendarEvent newCalendarEvent;
                if(restrictionFlag )
                {
                    string TimeString = StartDateEntry.LocalDateTime.ToShortDateString() + " " + StartTime;
                    DateTimeOffset StartDateTime = DateTimeOffset.Parse(TimeString);
                    TimeString = EndDateEntry.LocalDateTime.ToShortDateString() + " " + EndTime;
                    DateTimeOffset EndDateTime = DateTimeOffset.Parse(TimeString);


                    RestrictionProfile myRestrictionProfile = CreateRestrictionProfile(newEvent.RestrictionStart, newEvent.RestrictionEnd, newEvent.isWorkWeek);
                    newCalendarEvent = new CalendarEventRestricted(Name, StartDateTime, EndDateTime, myRestrictionProfile, TimeSpan.Parse(EventDuration), MyRepetition, false, true, Convert.ToInt32(Count), RigidScheduleFlag, new Location_Elements(), new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), new EventDisplay(true, userColor, userColor.User < 1 ? 0 : 1), new MiscData());
                }
                else
                {
                    newCalendarEvent = new CalendarEvent(Name, StartTime, StartDateEntry, EndTime, EndDateEntry, Count, "", EventDuration, MyRepetition, true, RigidScheduleFlag, "", true, EventLocation, true, new EventDisplay(true, userColor, userColor.User<1?0:1), new MiscData(), false);
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

        RestrictionProfile CreateRestrictionProfile(string Start, string End,string workWeek, string DaySelection="")
        { 
            DateTimeOffset RestrictStart = DateTimeOffset.Parse(Start);
            DateTimeOffset RestrictEnd = DateTimeOffset.Parse(End);
            
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
        

        // PUT api/schedule/5
        //[NonAction]
        public async Task<IHttpActionResult> Put(int id, [FromBody]string value)
        {
            return Ok("return");
        }

        // DELETE api/schedule/5
        [NonAction]
        public void Delete(int id)
        {
            return;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
