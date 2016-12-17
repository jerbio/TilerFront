using System;
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

namespace TilerFront.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db = new ApplicationDbContext();
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
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
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
            long Milliseconds = (long)(new DateTimeOffset(myUser.LastChange.AddDays(10)) - TilerElementExtension.JSStartTime).TotalMilliseconds;
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
            string TimeString = model.TimeOfDay + TilerElementExtension.JSStartTime.Date.ToShortDateString();
            var result= IdentityResult.Failed(new string[]{"Invalid Time Start Of Time"});

            DateTimeOffset TimeOfDay = new DateTimeOffset();
            if (DateTimeOffset.TryParse(TimeString,out TimeOfDay))
            {  
                /*
                if(isDST)
                {
                    TimeOfDay=TimeOfDay.AddHours(-1);
                }
                */
                TilerUser myUser = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                TimeSpan OffSetSpan= TimeSpan.FromMinutes(Convert.ToInt32( model.TimeZoneOffSet));
                TimeOfDay = TimeOfDay.ToOffset(OffSetSpan);
                myUser.LastChange=TimeOfDay.DateTime;
                result= await UserManager.UpdateAsync(myUser);
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
            return View((await db.ThirdPartyAuthentication.Where(obj => userID == obj.TilerID).ToListAsync()).Select(obj => obj.getThirdPartyOut()));
        }

        async public Task<List<ThirdPartyCalendarAuthenticationModel>> getAllThirdPartyAuthentication()
        {
            string userID = User.Identity.GetUserId();
            List<ThirdPartyCalendarAuthenticationModel> RetValue = (await db.ThirdPartyAuthentication.Where(obj => userID == obj.TilerID).ToListAsync().ConfigureAwait(false));
            return RetValue;
        }

        async Task<ThirdPartyCalendarAuthenticationModel> getGoogleAuthenticationData(string TilerUSerID,string EmailID )
        {
            Object[] Param = { TilerUSerID, EmailID, (int)TilerElements.ThirdPartyControl.CalendarTool.Google};
            ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await db.ThirdPartyAuthentication.FindAsync(Param);
            return checkingThirdPartyCalendarAuthentication;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> deleteGoogleAccount(ThirdPartyAuthenticationForView modelData)
        {
            await ThirdPartyCalendarAuthenticationModelsController.deleteGoogleAccount(modelData).ConfigureAwait(false);
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
                    Controllers.ThirdPartyCalendarAuthenticationModelsController.initializeCurrentURI(System.Web.HttpContext.Current.Request.Url.Authority);
                    bool successInCreation = await ThirdPartyCalendarAuthenticationModelsController.CreateGoogle(thirdPartyCalendarAuthentication).ConfigureAwait(false);
                    if (successInCreation)
                    {
                        ;
                    }
                    else
                    {
                        ;
                    }
                    return RedirectToAction("ImportCalendar");
                    /*
                    try
                    {
                        Object[] Param = { thirdPartyCalendarAuthentication.TilerID, thirdPartyCalendarAuthentication.Email, thirdPartyCalendarAuthentication.ProviderID };
                        ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await db.ThirdPartyAuthentication.FindAsync(Param);
                        if (checkingThirdPartyCalendarAuthentication != null)
                        {
                            checkingThirdPartyCalendarAuthentication.ID = thirdPartyCalendarAuthentication.ID;
                            checkingThirdPartyCalendarAuthentication.Token = thirdPartyCalendarAuthentication.Token;
                            checkingThirdPartyCalendarAuthentication.RefreshToken = thirdPartyCalendarAuthentication.RefreshToken;
                            checkingThirdPartyCalendarAuthentication.Deadline = thirdPartyCalendarAuthentication.Deadline;
                            db.Entry(checkingThirdPartyCalendarAuthentication).State = EntityState.Modified;
                            await db.SaveChangesAsync().ConfigureAwait(false);
                        }
                        else
                        {
                            db.ThirdPartyAuthentication.Add(thirdPartyCalendarAuthentication);
                            await db.SaveChangesAsync();
                            //RedirectToAction()

                            //Object ParamS = new {TilerID= thirdPartyCalendarAuthentication.TilerID,Email= thirdPartyCalendarAuthentication.Email,Provider = thirdPartyCalendarAuthentication.ProviderID };

                            //return RedirectToAction("Authenticate", "ThirdPartyCalendarAuthenticationModels", ParamS);

                            if(!(await SendRequestForGoogleNotification(thirdPartyCalendarAuthentication).ConfigureAwait(false)))
                            {
                                await deleteGoogleAccount(thirdPartyCalendarAuthentication.getThirdPartyOut()).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ;
                    }
                    */
                }
                else 
                {
                    Object[] Param = { thirdPartyCalendarAuthentication.TilerID, thirdPartyCalendarAuthentication.Email, thirdPartyCalendarAuthentication.ProviderID };
                    ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await db.ThirdPartyAuthentication.FindAsync(Param);
                    if (checkingThirdPartyCalendarAuthentication != null)
                    {
                        await deleteGoogleAccount(thirdPartyCalendarAuthentication.getThirdPartyOut()).ConfigureAwait(false);
                    }
                }
                return RedirectToAction("ImportCalendar");
            }

            return View("ImportCalendar");
        }

        /*
        public async Task<bool> SendRequestForGoogleNotification(ThirdPartyCalendarAuthenticationModel AuthenticationData)
        {
            bool RetValue = false;
            HttpContext Ctx = System.Web.HttpContext.Current;
            try
            {

                var url = string.Format
                (
                    "https://www.googleapis.com/calendar/v3/calendars/{0}/events/watch",
                    AuthenticationData.Email 
                );
                //url = "https://mytilerkid.azurewebsites.net/api/GoogleNotification/Trigger";
                var httpWebRequest = HttpWebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.Headers["Authorization"] =
                    string.Format("Bearer {0}", AuthenticationData.Token);
                httpWebRequest.Method = "POST";
                // added the character set to the content-type as per David's suggestion
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
                httpWebRequest.CookieContainer = new CookieContainer();

                // replaced Environment.Newline by CRLF as per David's suggestion
                GoogleNotificationRequestModel NotificationRequest = AuthenticationData.getGoogleNotificationCredentials(Ctx.Request.Url.Authority);

                var requestText = JsonConvert.SerializeObject(NotificationRequest);

                using (var stream = httpWebRequest.GetRequestStream())
                // replaced Encoding.UTF8 by new UTF8Encoding(false) to avoid the byte order mark
                using (var streamWriter = new System.IO.StreamWriter(stream, new UTF8Encoding(false)))
                {
                    streamWriter.Write(requestText);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    string result = streamReader.ReadToEnd();
                    GoogleNotificationWatchResponseModel NotificationResponse = JsonConvert.DeserializeObject<GoogleNotificationWatchResponseModel>(result);
                    db.GoogleNotificationCredentials.Add(NotificationResponse);
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                RetValue = true;


            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

            return RetValue;
        }
        //*/
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
                    Google.Apis.Oauth2.v2.Data.Userinfoplus userInfo ;
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
                        thirdpartydata.ProviderID = (int)TilerElements.ThirdPartyControl.CalendarTool.Google;
                        thirdpartydata.Token = myCredential.Token.AccessToken;
                        thirdpartydata.RefreshToken = myCredential.Token.RefreshToken;

                        return await CreateGoogle(thirdpartydata).ConfigureAwait(false);
                    }
                    else //if user hasn authenticated tiler before, then update current credentials
                    {
                        ThirdPartyCalendarAuthenticationModel retrievedAuthentication = await getGoogleAuthenticationData(UserID, Email).ConfigureAwait(false);
                        try 
                        {
                            await retrievedAuthentication.refreshAndCommitToken().ConfigureAwait(false);
                        }
                        catch
                        {
                            if (retrievedAuthentication != null)
                            {
                                deleteGoogleAccount(retrievedAuthentication.getThirdPartyOut()).RunSynchronously();
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

        /*

        private async Task<BaseClientService.Initializer> GetCredentials()
        {
            ClientSecrets secrets = new ClientSecrets
            {
                ClientId = "518133740160-i5ie6s4h802048gujtmui1do8h2lqlfj.apps.googleusercontent.com",
                ClientSecret = "NKRal5rA8NM5qHnmiigU6kWh"
            };

            String[] SCOPES = new[] { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarReadonly };

            IDataStore credentialPersistanceStore = getPersistentCredentialStore();

            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(secrets,
                    SCOPES, getUserId(), CancellationToken.None, credentialPersistanceStore);

            BaseClientService.Initializer initializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "TilerWebZy"
            };

            return initializer;
        }

        private String getUserId()
        {
            // TODO: Generate a unique user ID within your system for this user. The credential
            // data store will use this as a key to look up saved credentials.
            return User.Identity.GetUserId();
        }

        /// <summary> Returns a persistent data store for user's credentials. </summary>
        private  IDataStore getPersistentCredentialStore()
        {
            // TODO: This uses a local file store to cache credentials. You should replace this with
            // the appropriate IDataStore for your application.
            return new FileDataStore("Drive.Sample.Credentals");
        }
        */

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