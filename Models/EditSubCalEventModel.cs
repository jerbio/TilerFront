using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class EditSubCalEventModel:AuthorizedUser
    {
        public string EventID { get; set; }
        public string EventName { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long CalStart { get; set; }
        public long CalEnd { get; set; }
        //public long Duration { get; set; }
        public long Split { get; set; }    
    }
}