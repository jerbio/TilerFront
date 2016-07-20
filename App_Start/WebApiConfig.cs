using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using TilerElements.Wpf;
namespace TilerFront
{
    public static class WebApiConfig
    {
        //public static class WebApiConfig
        
            
            public static void Register(HttpConfiguration config)
            {
                // Web API configuration and services

                // Web API routes

                //config.EnableCors();
                //config.EnableCors(new EnableCorsAttribute(origins: "*", headers: "accept, authorization, origin", methods: "DELETE,PUT,POST,GET"));
                config.MapHttpAttributeRoutes();
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                config.Routes.MapHttpRoute(
                    name: "ActionApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
            }


        /*
		public static Models.SubCalEvent ToSubCalEvent(this TilerElements.SubCalendarEvent SubCalendarEventEntry, TilerElements.CalendarEvent CalendarEventEntry=null)
		{
			Models.SubCalEvent retValue = new Models.SubCalEvent();
			retValue.ID = SubCalendarEventEntry.ID;
			retValue.CalendarID = SubCalendarEventEntry.SubEvent_ID.getRepeatCalendarEventID();
			
			retValue.SubCalStartDate = (long)(SubCalendarEventEntry.Start - JSStartTime).TotalMilliseconds;
			retValue.SubCalEndDate = (long)(SubCalendarEventEntry.End - JSStartTime).TotalMilliseconds;
			retValue.SubCalTotalDuration = SubCalendarEventEntry.ActiveDuration;
			retValue.SubCalRigid = SubCalendarEventEntry.Rigid;
			retValue.SubCalAddressDescription = SubCalendarEventEntry.myLocation.Description;
			retValue.SubCalAddress = SubCalendarEventEntry.myLocation.Address;
			if (CalendarEventEntry != null)
			{
				retValue.SubCalCalendarName = CalendarEventEntry.Name;
				retValue.SubCalCalEventStart = (long)(CalendarEventEntry.Start - JSStartTime).TotalMilliseconds;
				retValue.SubCalCalEventEnd = (long)(CalendarEventEntry.End - JSStartTime).TotalMilliseconds;
			}

			retValue.SubCalEventLong = SubCalendarEventEntry.myLocation.YCoordinate;
			retValue.SubCalEventLat = SubCalendarEventEntry.myLocation.XCoordinate;
			retValue.RColor = SubCalendarEventEntry.UIParam.UIColor.R;
			retValue.GColor = SubCalendarEventEntry.UIParam.UIColor.G;
			retValue.BColor = SubCalendarEventEntry.UIParam.UIColor.B;
			retValue.OColor = SubCalendarEventEntry.UIParam.UIColor.O;
			retValue.isComplete = SubCalendarEventEntry.isComplete;
			retValue.isEnabled = SubCalendarEventEntry.isEnabled;
			retValue.Duration = (long)SubCalendarEventEntry.ActiveDuration.TotalMilliseconds;
			retValue.otherPartyID = SubCalendarEventEntry.ThirdPartyID;
			retValue.EventPreDeadline = (long)SubCalendarEventEntry.PreDeadline.TotalMilliseconds;
			retValue.Priority = SubCalendarEventEntry.EventPriority;
			retValue.Conflict = String.Join(",", SubCalendarEventEntry.Conflicts.getConflictingEventIDs());
			retValue.ColorSelection = SubCalendarEventEntry.UIParam.UIColor.User;
			return retValue;
		}

		public static Models.CalEvent ToCalEvent(this TilerElements.CalendarEvent CalendarEventEntry, TilerElements.TimeLine Range = null)
		{
            Models.CalEvent retValue = new Models.CalEvent();
            retValue.ID = CalendarEventEntry.ID;
            retValue.CalendarName = CalendarEventEntry.Name;
            retValue.StartDate = (long)(CalendarEventEntry.Start - JSStartTime).TotalMilliseconds;
            retValue.EndDate = (long)(CalendarEventEntry.End - JSStartTime).TotalMilliseconds;
            retValue.TotalDuration = CalendarEventEntry.Duration;
            retValue.Rigid = CalendarEventEntry.Rigid;
            retValue.AddressDescription = CalendarEventEntry.Location.Description;
            retValue.Address = CalendarEventEntry.Location.Address;
            retValue.Longitude = CalendarEventEntry.Location.YCoordinate;
            retValue.Latitude = CalendarEventEntry.Location.XCoordinate;
            retValue.NumberOfSubEvents = CalendarEventEntry.AllSubEvents.Count();// CalendarEventEntry.NumberOfSplit;// AllSubEvents.Count();
            retValue.RColor = CalendarEventEntry.UIParam.UIColor.R;
            retValue.GColor = CalendarEventEntry.UIParam.UIColor.G;
            retValue.BColor = CalendarEventEntry.UIParam.UIColor.B;
            retValue.OColor = CalendarEventEntry.UIParam.UIColor.O;
            retValue.ColorSelection = CalendarEventEntry.UIParam.UIColor.User;
            retValue.NumberOfCompletedTasks = CalendarEventEntry.CompletionCount;
            retValue.NumberOfDeletedEvents = CalendarEventEntry.DeletionCount;

            TimeSpan FreeTimeLeft = CalendarEventEntry.RangeSpan - CalendarEventEntry.Duration;
            long TickTier1 = (long)(FreeTimeLeft.Ticks * (.667));
            long TickTier2 = (long)(FreeTimeLeft.Ticks * (.865));
            long TickTier3 = (long)(FreeTimeLeft.Ticks * (1));
            retValue.Tiers = new long[] { TickTier1, TickTier2, TickTier3 };
            if (Range != null)
            {
                retValue.AllSubCalEvents = CalendarEventEntry.ActiveSubEvents.Where(obj => obj.RangeTimeLine.InterferringTimeLine(Range) != null).Select(obj => obj.ToSubCalEvent(CalendarEventEntry)).ToList();
            }
            else
            {
                retValue.AllSubCalEvents = CalendarEventEntry.ActiveSubEvents.Select(obj => obj.ToSubCalEvent(CalendarEventEntry)).ToList();
            }

            return retValue;
        }

        public static Models.Location ToLocationModel(this Location_Elements LocationEntry)
        {
            Models.Location retValue = new Models.Location();
            retValue.Address = LocationEntry.Address;
            retValue.Tag = LocationEntry.Description;
            retValue.Long = LocationEntry.YCoordinate;
            retValue.Lat = LocationEntry.XCoordinate;
            retValue.isNull = LocationEntry.isNull;
            return retValue;
        }
    */

    }
}
