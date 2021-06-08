using DBTilerElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using TilerFront.Models;

namespace TilerFront.Controllers
{
    [System.Web.Http.Authorize]
    public class LocationController : TilerApiController
    {
        [System.Web.Http.HttpGet]
        [ResponseType(typeof(PostBackStruct))]
        [System.Web.Http.Route("api/Location/Name")]
        public async Task<IHttpActionResult> LocationName([FromUri] NameSearchModel SearchData)
        {
            UserAccount retrievedUser = await SearchData.getUserAccount(db);
            await retrievedUser.Login();

            PostBackData retValue = new PostBackData("", 4);
            if (retrievedUser.Status)
            {
                IEnumerable<TilerElements.Location> retrievedCalendarEvents = retrievedUser.ScheduleLogControl.getLocationsByDescription(SearchData.Data).ToList();
                retValue = new PostBackData(retrievedCalendarEvents.Select(obj => obj.ToLocationModel()).ToList(), 0);
            }

            return Ok(retValue.getPostBack);
        }
    }
}