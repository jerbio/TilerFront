using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class GoogleNotificationWatchResponseModel
    {
        public string kind { get; set; }//"kind": "api#channel",
        public string id { get; set; }//"id": "01234567-89ab-cdef-0123456789ab"", // ID you specified for this channel.
        public string resourceId { get; set; }//"resourceId": "o3hgv1538sdjfh", // ID of the watched resource.
        public string resourceUri { get; set; }//"resourceUri": "https://www.googleapis.com/calendar/v3/calendars/my_calendar@gmail.com/events", // Version-specific ID of the watched resource.
        public string token { get; set; }//"token": "target=myApp-myCalendarChannelDest", // Present only if one was provided.
        public long expiration { get; set; }//"expiration": 1426325213000, // Actual expiration time as Unix timestamp (in ms), if applicable.
    }
}