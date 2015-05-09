using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class EditSubCalEventModel : EditCalEventModel
    {
        public long CalStart { get; set; }
        public long CalEnd { get; set; }
        //public long Duration { get; set; }
    }
}