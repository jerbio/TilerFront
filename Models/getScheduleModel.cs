using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    /// <summary>
    /// Represents schedule for user. The StartRange is the start time in JS in milliseconds. The EndRange is the start time in JS in milliseconds. Default is 2 weeks before today to 90 days from today.
    /// </summary>
    public class getScheduleModel:AuthorizedUser
    {
        public long StartRange { get; set; }
        public long EndRange { get; set; }
    }
}