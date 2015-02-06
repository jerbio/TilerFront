using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class getScheduleModel:AuthorizedUser
    {
        public long StartRange { get; set; }
        public long EndRange { get; set; }
    }
}