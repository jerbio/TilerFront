﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TilerFront.Models
{
    public class CalEvent
    {
        public string ID { get; set; }
        public string CalendarName { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public bool Rigid { get; set; }
        public string AddressDescription { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int NumberOfSubEvents { get; set; }
        public int NumberOfDeletedEvents { get; set; }
        public int NumberOfCompletedTasks { get; set; }
        public int RColor { get; set; }
        public int GColor { get; set; }
        public int BColor { get; set; }
        public double OColor { get; set; }
        public int ColorSelection { get; set; }
        public IList<long> Tiers { get; set; }
        public IList<SubCalEvent> AllSubCalEvents { get; set; }

        
    }
}