using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront.Models
{
    public class WhatIfModel : AuthorizedUser
    {
        public long DurationInMs { get; set; } = -1;
        public string EventId { get; set; }
        public string FormattedAsISO8601 { get; set; }

        public AuthorizedUser User
        {
            get
            {
                return new AuthorizedUser { UserName = UserName, UserID = UserID, MobileFlag = MobileFlag, TimeZoneOffset = TimeZoneOffset };
            }
        }

        public TimeSpan Duration
        {
            get
            {
                TimeSpan retValue;
                if (DurationInMs != -1)
                {
                    retValue = TimeSpan.FromMilliseconds(DurationInMs);
                }
                else
                {
                    retValue = System.Xml.XmlConvert.ToTimeSpan(FormattedAsISO8601);
                    if(string.IsNullOrEmpty(FormattedAsISO8601))
                    {
                        throw new CustomErrors("Properties of WhatIfModel not properly initialized");
                    }
                }
                
                return retValue;
            }
        }
    }
}