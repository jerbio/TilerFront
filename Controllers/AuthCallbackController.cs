
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading;
using System.Threading.Tasks;


using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Services;

namespace TilerFront.Controllers
{
    public class AuthCallbackController : Google.Apis.Auth.OAuth2.Mvc.Controllers.AuthCallbackController
    {
        protected override Google.Apis.Auth.OAuth2.Mvc.FlowMetadata FlowData
        {
            get 
            {
                string name = User.Identity.Name;
                var RetValue= new AppFlowMetadata();
                return RetValue;
            }
        }
    }
}