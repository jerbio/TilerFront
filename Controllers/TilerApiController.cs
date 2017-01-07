using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using System.Web;
using TilerFront.Models;
using TilerElements;
using DBTilerElement;
//using TilerGoogleCalendarLib;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Web.Http.Cors;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace TilerFront
{
    /// <summary>
    /// Tiler controller that provides custom tiler functionality
    /// </summary>
    public class TilerApiController: ApiController
    {
        protected ApplicationDbContext db = new ApplicationDbContext();
    }
}
