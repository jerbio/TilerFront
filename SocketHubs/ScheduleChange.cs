using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json.Linq;
using TilerElements;
using DBTilerElement;
using System.Threading.Tasks;

namespace TilerFront.SocketHubs
{
    public class ScheduleChange : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public void refereshDataFromSockets(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.sendToAll(name, message);
        }
        internal void broadCastRequestToChangeSchedule(string userId, string uniquerequestId)
        {

        }

        public void triggerRefreshData() {
            string who = HttpContext.Current.User.Identity.GetUserId();
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<TilerFront.SocketHubs.ScheduleChange>();
            dynamic triggerRefreshRequest = new JObject();
            triggerRefreshRequest.refreshData = new JObject();
            triggerRefreshRequest.refreshData.trigger = true;
            context.Clients.Group(who).refereshDataFromSockets(triggerRefreshRequest);
        }

        public void pausedEventUpdate(Models.PausedEvent PausedEvent, IEnumerable<SubCalendarEvent> SubEvents)
        {
            string who = HttpContext.Current.User.Identity.GetUserId();
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<TilerFront.SocketHubs.ScheduleChange>();
            dynamic pauseEventRequest = new JObject();
            pauseEventRequest.pauseData = new JObject();
            pauseEventRequest.pauseData.pausedEvent = new JObject(PausedEvent);
            JArray jobjectSubeevents = new JArray(SubEvents.Select(subEvent=>subEvent.ToSubCalEvent()).ToArray());
            pauseEventRequest.pauseData.subevents = jobjectSubeevents;
            context.Clients.Group(who).refereshDataFromSockets(pauseEventRequest);
        }

        public override Task OnConnected()
        {
            string UserId = HttpContext.Current.User.Identity.GetUserId();

            Groups.Add(Context.ConnectionId, UserId);

            return base.OnConnected();
        }
    }
}