﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TilerFront.Models;
using DBTilerElement;
using System.Threading;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Newtonsoft.Json;

using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Oauth2.v2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

using TilerElements;
using System.Dynamic;

namespace TilerFront.Controllers
{
    [Authorize]
    public class ManageController : TilerController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.StartOfDaySuccess ? "Tiler has updated the end of your day."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                Id = userId,
                UserName = User.Identity.GetUserName(),
                FullName = User.Identity.Name
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }
#region End of day
        //
        // GET: /Manage/ChangeStartOfDay
        public ActionResult ChangeStartOfDay()
        {
            TilerUser myUser = UserManager.FindById(User.Identity.GetUserId());
            long Milliseconds = (long)(myUser.EndfOfDay.AddDays(10) - TilerElementExtension.JSStartTime).TotalMilliseconds;
            var model = new ChangeStartOfDayModel
            {
                TimeOfDay = Milliseconds.ToString()
            };
            return View(model);
        }
        //
        // POST: /Manage/ChangeStartOfDay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeStartOfDay(ChangeStartOfDayModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //bool isDST = Convert.ToBoolean(model.Dst);
            var result= IdentityResult.Failed(new string[]{"Invalid Time Start Of Time"});

            DateTimeOffset TimeOfDay = new DateTimeOffset();
            if (DateTimeOffset.TryParse(model.TimeOfDay, out TimeOfDay))
            {  
                /*
                if(isDST)
                {
                    TimeOfDay=TimeOfDay.AddHours(-1);
                }
                */
                TilerUser myUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                myUser.EndfOfDay=TimeOfDay.DateTime;
                myUser.TimeZone = model.TimeZone;
                myUser.EndfOfDayString = model.TimeOfDay;
                myUser.updateTimeZone();
                result = await UserManager.UpdateAsync(myUser);
            }

            if(result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.StartOfDaySuccess });
            }
            AddErrors(result);
            return View(model);
        }


#endregion

#region ThirdParty Authentincation
        // Get: /Manage/ImportCalendar
        public async Task<ActionResult> ImportCalendar()
        {
            string userID = User.Identity.GetUserId();
            TilerUser myUser = UserManager.FindById(User.Identity.GetUserId());
            dynamic model = new ExpandoObject();
            model.thirdpartyCalendars = (await dbContext.ThirdPartyAuthentication.Include(o=>o.DefaultLocation).Where(obj => userID == obj.TilerID).ToListAsync()).Select(obj => obj.getThirdPartyOut());
            model.user = myUser;
            return View(model);
        }

        async public Task<List<ThirdPartyCalendarAuthenticationModel>> getAllThirdPartyAuthentication()
        {
            string userID = User.Identity.GetUserId();
            List<ThirdPartyCalendarAuthenticationModel> RetValue = (await dbContext.ThirdPartyAuthentication.Include(o => o.DefaultLocation).Where(obj => userID == obj.TilerID).ToListAsync().ConfigureAwait(false));
            return RetValue;
        }

        async Task<ThirdPartyCalendarAuthenticationModel> getGoogleAuthenticationData(string TilerUSerID,string EmailID )
        {
            Object[] Param = { TilerUSerID, EmailID, TilerElements.ThirdPartyControl.CalendarTool.google.ToString()};
            ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await dbContext.ThirdPartyAuthentication.FindAsync(Param);
            return checkingThirdPartyCalendarAuthentication;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> deleteGoogleAccount(ThirdPartyAuthenticationForView modelData)
        {
            ThirdPartyCalendarAuthenticationModelsController thirdPartyController = new ThirdPartyCalendarAuthenticationModelsController();
            await thirdPartyController.deleteGoogleAccount(modelData).ConfigureAwait(false);
            return RedirectToAction("ImportCalendar");
        }

        internal static async Task delelteGoogleAuthentication(IEnumerable<ThirdPartyCalendarAuthenticationModel> AllAuthenticaitonData)
        {

            List<Task<bool>> AllConcurrentTasks = new List<System.Threading.Tasks.Task<bool>>();
            

            foreach (ThirdPartyCalendarAuthenticationModel eachThirdPartyCalendarAuthenticationModel in AllAuthenticaitonData)
            {
                AllConcurrentTasks.Add(eachThirdPartyCalendarAuthenticationModel.unCommitAuthentication());
            }


            foreach (Task<bool> eachTask in AllConcurrentTasks)
            {
                bool UncommitStatus = await eachTask.ConfigureAwait(false);
            }
        }


        public async Task<ActionResult> CreateGoogle([Bind(Include = "ID,isLongLived,Email,Token,RefreshToken,ProviderID,Deadline")] ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthentication,bool deletGoogleCalendarFromLog=false)
        {
            if (ModelState.IsValid)
            {
                bool makeAdditionCall = false;
                if (!deletGoogleCalendarFromLog)
                {
                    ThirdPartyCalendarAuthenticationModelsController thirdPartyController = new ThirdPartyCalendarAuthenticationModelsController();
                    if (System.Web.HttpContext.Current != null)// this helps save the reurn uri for notification
                    {
                        thirdPartyController.initializeCurrentURI(System.Web.HttpContext.Current.Request.Url.Authority);
                    }
                    bool successInCreation = await thirdPartyController.CreateGoogle(thirdPartyCalendarAuthentication).ConfigureAwait(false);
                    if (successInCreation)
                    {
                        ;
                    }
                    else
                    {
                        ;
                    }
                    return RedirectToAction("ImportCalendar");
                }
                else 
                {
                    Object[] Param = { thirdPartyCalendarAuthentication.TilerID, thirdPartyCalendarAuthentication.Email, thirdPartyCalendarAuthentication.ProviderID };
                    ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await dbContext.ThirdPartyAuthentication.FindAsync(Param);
                    if (checkingThirdPartyCalendarAuthentication != null)
                    {
                        await deleteGoogleAccount(thirdPartyCalendarAuthentication.getThirdPartyOut()).ConfigureAwait(false);
                    }
                }
                return RedirectToAction("ImportCalendar");
            }

            return View("ImportCalendar");
        }

        async public Task<ActionResult> ImportGoogle(CancellationToken cancellationToken)
        {
            string UserID = User.Identity.GetUserId();
            //var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata(UserID)).
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(cancellationToken);
            Google.Apis.Auth.OAuth2.UserCredential myCredential = result.Credential;



            if (myCredential != null)
            {
                var service = new Oauth2Service(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = myCredential,
                        ApplicationName = "ASP.NET MVC Sample"
                    });

                    bool ResetThirdparty = false;

                    ThirdPartyCalendarAuthenticationModel thirdpartydata = new ThirdPartyCalendarAuthenticationModel();
                    Google.Apis.Oauth2.v2.Data.Userinfo userInfo ;
                    try
                    {
                        userInfo = service.Userinfo.Get().Execute();
                    }
                    catch (Exception e)
                    {
                        ResetThirdparty = true;
                        return RedirectToAction("ImportGoogle");
                    }


                
                    string Email = userInfo.Email;

                    if (myCredential.Token.RefreshToken!=null)//if user hasn't authenticated tiler before
                    {
                        thirdpartydata.Email = Email;
                        thirdpartydata.TilerID = UserID;
                        thirdpartydata.ID = myCredential.UserId;
                        thirdpartydata.isLongLived = false;
                        double totalSeconds = myCredential.Token.ExpiresInSeconds == null ? 0 : (double)myCredential.Token.ExpiresInSeconds;
                        DateTime myDate = myCredential.Token.Issued.AddSeconds(totalSeconds);
                        thirdpartydata.Deadline = new DateTimeOffset(myDate);
                        thirdpartydata.ProviderID = TilerElements.ThirdPartyControl.CalendarTool.google.ToString();
                        thirdpartydata.Token = myCredential.Token.AccessToken;
                        thirdpartydata.RefreshToken = myCredential.Token.RefreshToken;

                        return await CreateGoogle(thirdpartydata).ConfigureAwait(false);
                    }
                    else //if user hasn authenticated tiler before, then update current credentials
                    {
                        ThirdPartyCalendarAuthenticationModel retrievedAuthentication = await getGoogleAuthenticationData(UserID, Email).ConfigureAwait(false);
                        try 
                        {
                            await retrievedAuthentication.refreshAndCommitToken(dbContext).ConfigureAwait(false);
                        }
                        catch
                        {
                            if (retrievedAuthentication != null)
                            {
                                deleteGoogleAccount(retrievedAuthentication.getThirdPartyOut()).RunSynchronously();
                            }
                            else if(myCredential.Token!=null)
                            {
                                thirdpartydata.Email = Email;
                                thirdpartydata.TilerID = UserID;
                                thirdpartydata.ID = myCredential.UserId;
                                thirdpartydata.isLongLived = false;
                                double totalSeconds = myCredential.Token.ExpiresInSeconds == null ? 0 : (double)myCredential.Token.ExpiresInSeconds;
                                DateTime myDate = myCredential.Token.IssuedUtc.AddSeconds(totalSeconds);
                                thirdpartydata.Deadline = new DateTimeOffset(myDate);
                                thirdpartydata.ProviderID = TilerElements.ThirdPartyControl.CalendarTool.google.ToString();
                                thirdpartydata.Token = myCredential.Token.AccessToken;
                                thirdpartydata.RefreshToken = myCredential.Token.RefreshToken;
                                await thirdpartydata.unCommitAuthentication().ConfigureAwait(false);
                                return await ImportGoogle(cancellationToken).ConfigureAwait(false);
                        }
                        }
                        
                        return RedirectToAction("ImportCalendar");
                    }
            }
            else
            {
                //return View();
                return new RedirectResult(result.RedirectUri);
            }
        }
#endregion

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            StartOfDaySuccess,
            Error
        }

#endregion
    }
}