using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Facebook;
using Owin;
using TilerFront.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Mvc;


using Google.Apis.Calendar.v3;
using Google.Apis.Plus.v1;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util.Store;
using TilerElements;
using TilerElements.Connectors;
using TilerElements.DB;

namespace TilerFront
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, TilerUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");


            var facebookOptions = new Microsoft.Owin.Security.Facebook.FacebookAuthenticationOptions()
            {
                Provider = new FacebookAuthenticationProvider()
                {
                    OnAuthenticated = (context) =>
                        {
                            // All data from facebook in this object. 
                            var rawUserObjectFromFacebookAsJson = context.User;
                            var myToken= context.AccessToken;
                            context.Identity.AddClaim(new Claim("ThirdPartyType", ((int)ThirdPartyControl.CalendarTool.Facebook).ToString()));
                            // Only some of the basic details from facebook 
                            // like id, username, email etc are added as claims.
                            // But you can retrieve any other details from this
                            // raw Json object from facebook and add it as claims here.
                            // Subsequently adding a claim here will also send this claim
                            // as part of the cookie set on the browser so you can retrieve
                            // on every successive request. 
                            return Task.FromResult(0);
                        }
                }
            };

            facebookOptions.Scope.Add("email");

            var googleOptions = new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "518133740160-i5ie6s4h802048gujtmui1do8h2lqlfj.apps.googleusercontent.com",
                ClientSecret = "NKRal5rA8NM5qHnmiigU6kWh",
                Provider = new GoogleOAuth2AuthenticationProvider
                {
                    OnAuthenticated = async context =>
                    {

                        context.Identity.AddClaim(new Claim(ClaimTypes.Name, context.Identity.FindFirstValue(ClaimTypes.Name)));
                        context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Identity.FindFirstValue(ClaimTypes.Email)));
                        context.Identity.AddClaim(new Claim("AccessToken", context.AccessToken));
                        context.Identity.AddClaim(new Claim("ThirdPartyType", ((int)ThirdPartyControl.CalendarTool.Google).ToString()));
                        context.Identity.AddClaim(new Claim("ExpiryDuration", context.ExpiresIn.ToString()));
                        if (context.RefreshToken != null)
                        {
                            context.Identity.AddClaim(new Claim("RefreshToken", context.RefreshToken));
                        }
                        else
                        {
                            context.Identity.AddClaim(new Claim("RefreshToken", ""));
                        }


                    }
                },
                AccessType = "offline"
            };

            googleOptions.Scope.Add("https://www.googleapis.com/auth/plus.login");
            googleOptions.Scope.Add(PlusService.Scope.UserinfoEmail);
            googleOptions.Scope.Add(CalendarService.Scope.Calendar);
            googleOptions.Scope.Add(CalendarService.Scope.CalendarReadonly);
            
            

            app.UseFacebookAuthentication(
               appId: "1530915617167749",
               appSecret: "c68800eb9d3bf8eb9fd20ac1891cda5b");

            app.UseGoogleAuthentication(googleOptions);
            
        }
    }
}