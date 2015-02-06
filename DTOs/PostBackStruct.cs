using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class PostBackStruct
    {
        public PostError Error { get; set; }
        public dynamic Content { get; set; }
    }
}