using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Threading.Tasks;
using TilerFront.Models;
using System.Web.Http.Description;

namespace TilerFront.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class GoogleNotificationController : TilerApiController
    {
        [HttpPost]
        [Route("api/GoogleNotification/Trigger")]
        async public Task<System.Web.Http.IHttpActionResult> Trigger()
        {
            /*db.GoogleNotificationCredentials.Add(GoogleResponse);
            await db.SaveChangesAsync().ConfigureAwait(false);*/
            OkNegotiatedContentResult<string> retValue = new OkNegotiatedContentResult<string>("thumbs up", this);
            //return retValue;
            System.Web.HttpContext myContext = System.Web.HttpContext.Current;


            bool continueIntoTrigger = myContext.Request.Headers["X-Goog-Resource-State"].ToLower() == "exists";
            if (continueIntoTrigger)
            {
                string ChannelID = myContext.Request.Headers["X-Goog-Channel-ID"];
                ScheduleController.googleNotificationTrigger(ChannelID, db);
            }

            return retValue;
        }
        ///*
        [HttpGet]
        [Route("api/GoogleNotification/GoogNextNotif")]
        async public Task<System.Web.Http.IHttpActionResult> GoogNextNotif(Models.GoogleNotificationRequestModel GoogleResponse)
        {
            bool continueIntoTrigger = true;
            if (continueIntoTrigger)
            {
                string ChannelID = "2090f290-682a-47a7-9bfc-dcdc9f685993";
                ScheduleController.googleNotificationTrigger(ChannelID, db);
            }
            OkNegotiatedContentResult<string> retValue = new OkNegotiatedContentResult<string>("thumbs up", this);
            return retValue;
        }
    }
}
