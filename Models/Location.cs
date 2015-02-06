using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TilerFront.Models
{
    public class Location
    {
        public double Long { get; set; }   
        public double Lat { get; set; }
        public string Address { get; set; }
        public string Tag { get; set; }
        public bool isNull { get; set; }
    }
}