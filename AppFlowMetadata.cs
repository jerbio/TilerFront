using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;


using Google.Apis.Calendar.v3;
using Google.Apis.Plus.v1;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util.Store;

namespace TilerFront
{
    public class AppFlowMetadata : FlowMetadata
    {
        
        public AppFlowMetadata()
        {

        }

        ///*
        
        public AppFlowMetadata(string ID)
        {
            string UserID = "";
            UserID = ID;
        }
        //*/
        private static readonly IAuthorizationCodeFlow flow =
            new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = "518133740160-i5ie6s4h802048gujtmui1do8h2lqlfj.apps.googleusercontent.com",
                    ClientSecret = "NKRal5rA8NM5qHnmiigU6kWh"
                },
                Scopes = new[] { PlusService.Scope.UserinfoEmail, CalendarService.Scope.Calendar, CalendarService.Scope.CalendarReadonly},
                DataStore = new FileDataStore("Drive.Api.Auth.Store")
            });

        public static IAuthorizationCodeFlow getFlow()
        {
            return flow;
        }

        public override string GetUserId(Controller controller)
        {
            // In this sample we use the session to store the user identifiers.
            // That's not the best practice, because you should have a logic to identify
            // a user. You might want to use "OpenID Connect".
            // You can read more about the protocol in the following link:
            // https://developers.google.com/accounts/docs/OAuth2Login.
            ///*
            var user = controller.Session["user"];
            if (user == null)
            {
                user = Guid.NewGuid();
                controller.Session["user"] = user;
            }

            return user.ToString();
            /*
            string RetValue = UserID;

            if (string.IsNullOrEmpty(RetValue))
            {
                RetValue = user.ToString();
            }

            return RetValue;
            //*/
        }

        

        /*
        public override string AuthCallback
        {
            get
            {
                return @"/Manage/ImportGoogle";
            }
        }
        //*/

        public override IAuthorizationCodeFlow Flow
        {
            get { return flow; }
        }
    }
}