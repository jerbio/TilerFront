﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TilerFront.Models
{
    public class SubCalEvent
    {
        public string ID { get; set; }
        public string CalendarID { get; set; }
        public string SubCalCalendarName { get; set; }
        public long SubCalStartDate { get; set; }
        public long SubCalEndDate { get; set; }
        public TimeSpan SubCalTotalDuration { get; set; }
        public bool SubCalRigid { get; set; }
        public string SubCalAddressDescription { get; set; }
        public string SubCalAddress { get; set; }
        public long SubCalCalEventStart { get; set; }
        public long SubCalCalEventEnd { get; set; }
        public double SubCalEventLong { get; set; }
        public double SubCalEventLat { get; set; }
        public int RColor { get; set; }
        public int GColor { get; set; }
        public int BColor { get; set; }
        public double OColor { get; set; }
        public bool isComplete { get; set; }
        public bool isEnabled { get; set; }
        public long Duration { get; set; }
        public string otherPartyID { get; set; }
        public long EventPreDeadline { get; set; }
        public int Priority { get; set; }
        public string Conflict { get; set; }
        public int ColorSelection { get; set; }
        
    }
}