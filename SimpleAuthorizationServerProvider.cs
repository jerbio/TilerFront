using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using TilerElements;
using TilerFront.Controllers;

namespace TilerFront
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            using (AccountController accountController = new AccountController())
            {
                var userStore = new UserStore<TilerUser>(accountController.DbContext);
                ApplicationUserManager applicationUserManager = new ApplicationUserManager(userStore);
                ApplicationSignInManager signInManager = new ApplicationSignInManager(applicationUserManager, context.OwinContext.Authentication);
                var signInStatus = await signInManager.PasswordSignInAsync(context.UserName, context.Password, true, true).ConfigureAwait(false);

                switch (signInStatus)
                {
                    case SignInStatus.Success:
                    {
                        context.SetError("Success", "User login successfull.");
                    }
                    break;
                    case SignInStatus.LockedOut:
                    {
                        context.SetError("user_locked_out", "The user is locked out.");
                        return;
                    }
                    case SignInStatus.RequiresVerification:
                    {
                        context.SetError("user_verification_requiried", "User has not verified account. User needs to check email for verification steps");
                        return;
                    }
                    case SignInStatus.Failure:
                    default:
                        {
                            context.SetError("invalid_grant", "The user name or password is incorrect.");
                            return;
                        }
                }
            }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("sub", context.UserName));
            identity.AddClaim(new Claim("role", "user"));

            context.Validated(identity);

        }
    }
}