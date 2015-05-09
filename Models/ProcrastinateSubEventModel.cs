using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    /// <summary>
    /// Represents a request for procrastination of an event. EventID is required.
    /// </summary>
    public class ProcrastinateEventModel : ProcrastinateModel
    {
        public string EventID { get; set; }
    }
}