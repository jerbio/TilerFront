using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerCore;
using TilerElements;

namespace TilerFront
{
    public class DB_Schedule:Schedule
    {

        protected DB_Schedule(Dictionary<string, CalendarEvent> allEventDictionary, DateTimeOffset starOfDay, Dictionary<string, Location> locations, DateTimeOffset referenceNow, TilerUser user):base(allEventDictionary, starOfDay, locations, referenceNow, user)
        {

        }
        protected UserAccount myAccount;
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow, DateTimeOffset startOfDay):base()
        {
            Initialize(referenceNow, startOfDay).Wait();
            myAccount = AccountEntry;
        }
        public DB_Schedule(UserAccount AccountEntry, DateTimeOffset referenceNow)
        {
            Initialize(referenceNow).Wait();
            myAccount = AccountEntry;
        }
        async virtual protected Task Initialize(DateTimeOffset referenceNow)
        {
            DateTimeOffset StartOfDay = await myAccount.ScheduleData.getDayReferenceTime().ConfigureAwait(false);
            _Now = new ReferenceNow(referenceNow, StartOfDay);
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo().ConfigureAwait(false);
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();

                    //EventIDGenerator.Initialize((uint)(this.LastScheduleIDNumber));
                }
                Locations = profileData.Item3;
            }
        }

        async virtual protected Task Initialize(DateTimeOffset referenceNow, DateTimeOffset StartOfDay)
        {
            if (!myAccount.Status)
            {
                throw new Exception("Using non verified tiler Account, try logging into account first.");
            }
            _Now = new ReferenceNow(referenceNow, StartOfDay);
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location>> profileData = await myAccount.ScheduleData.getProfileInfo().ConfigureAwait(false);
            if (profileData != null)
            {
                DateTimeOffset referenceDayTimeNow = new DateTimeOffset(Now.calculationNow.Year, Now.calculationNow.Month, Now.calculationNow.Day, profileData.Item2.Hour, profileData.Item2.Minute, profileData.Item2.Second, new TimeSpan());// profileData.Item2;
                ReferenceDayTIime = Now.calculationNow < referenceDayTimeNow ? referenceDayTimeNow.AddDays(-1) : referenceDayTimeNow;
                AllEventDictionary = profileData.Item1;
                if (AllEventDictionary != null)
                {
                    //setAsComplete();
                    EventID.Initialize((uint)(myAccount.LastEventTopNodeID));
                    initializeThirdPartyCalendars();
                    updateThirdPartyCalendars(ThirdPartyControl.CalendarTool.outlook, new List<CalendarEvent>() { });
                    CompleteSchedule = getTimeLine();
                }
                Locations = profileData.Item3;
            }
        }

    }
}
