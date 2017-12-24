using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using TilerElements;
using TilerFront.Models;

namespace TilerFront.Controllers
{
    public class WhatIfController : TilerApiController
    {
        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/PushedAll")]
        public async Task<IHttpActionResult> pushed([FromBody]WhatIfModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccountDirect myUserAccount = await UserData.getUserAccountDirect(db);
            await myUserAccount.Login();
            PostBackData returnPostBack;
            if (myUserAccount.Status)
            {
                DB_Schedule MySchedule = new DB_Schedule(myUserAccount, myAuthorizedUser.getRefNow());
                Tuple<Health, Health> evaluation;
                evaluation = await MySchedule.WhatIfPushedAll(UserData.Duration, null);

                JObject before = evaluation.Item1.ToJson();
                JObject after = evaluation.Item2.ToJson();
                JObject resultData = new JObject();
                resultData.Add("before", before);
                resultData.Add("after", after);
                returnPostBack = new PostBackData(resultData, 0);
                return Ok(returnPostBack.getPostBack);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }

        [System.Web.Http.HttpPost]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/WhatIf/PushedNext")]
        public async Task<IHttpActionResult> pushedNext([FromBody]WhatIfModel UserData)
        {
            AuthorizedUser myAuthorizedUser = UserData.User;
            UserAccountDirect myUserAccount = await UserData.getUserAccountDirect(db);
            await myUserAccount.Login();
            PostBackData returnPostBack;
            if (myUserAccount.Status)
            {
                DB_Schedule MySchedule = new DB_Schedule(myUserAccount, myAuthorizedUser.getRefNow());
                Tuple<Health, Health> evaluation;
                
                
                if (String.IsNullOrEmpty(UserData.EventId))
                {
                    var calEventAndSubEvent = MySchedule.getNearestEventToNow();
                    EventID eventId = new EventID(calEventAndSubEvent.Item2.getId);
                    evaluation = await MySchedule.WhatIfPushed(UserData.Duration, eventId, null);
                }
                else
                {
                    evaluation = await MySchedule.WhatIfPushed(UserData.Duration, new EventID(UserData.EventId), null);
                }

                JObject before = evaluation.Item1.ToJson();
                JObject after = evaluation.Item2.ToJson();
                JObject resultData = new JObject();
                resultData.Add("before", before);
                resultData.Add("after", after);
                returnPostBack = new PostBackData(resultData, 0);
                return Ok(returnPostBack.getPostBack);
            }
            else
            {
                returnPostBack = new PostBackData("", 1);
            }

            return Ok(returnPostBack.getPostBack);
        }
    }
}