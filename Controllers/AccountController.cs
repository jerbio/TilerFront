using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Web.Http.Description;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity.EntityFramework;
using TilerFront.Models;


namespace TilerFront.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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

        [AllowAnonymous]
        public ActionResult TilerUser(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        [AllowAnonymous]
        public ActionResult TilerUser(TilerUnAuthorizedModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
        
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    if (Request.Browser.IsMobileDevice)
                    {
                        return RedirectToAction("Mobile", "Account");
                    }
                    else
                    {
                        return RedirectToAction("Desktop", "Account");
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        // POST: /Account/SignIn
        [HttpPost]
        [AllowAnonymous]
        [ResponseType(typeof(PostBackStruct))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(LoginViewModel model, string returnUrl)
        {
            JsonResult RetValue = new JsonResult();
            string LoopBackUrl = "";
            PostBackData retPost;
            if (!ModelState.IsValid)
            {
                string AllErrors = string.Join("\n", ModelState.Values.SelectMany(obj => obj.Errors.Select(obj1 => obj1.ErrorMessage)));
                retPost = new PostBackData(AllErrors, 3);
                RetValue.Data = (retPost.getPostBack);
                return RetValue;
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);
            
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        

                        if (Request.Browser.IsMobileDevice)
                        {
                            LoopBackUrl = "/Account/Mobile";
                        }
                        else
                        {
                            LoopBackUrl = "/Account/Desktop"; 
                        }

                        retPost = new PostBackData(LoopBackUrl, 0);
                        RetValue.Data = retPost.getPostBack;
                        return RetValue;
                    }

                case SignInStatus.LockedOut:
                    {
                        retPost = new PostBackData("User Locked out", 7);
                        RetValue.Data = retPost.getPostBack;
                        return RetValue;
                    }
                case SignInStatus.RequiresVerification:
                    {
                        retPost = new PostBackData("Verify User", 7);
                        RetValue.Data = retPost.getPostBack;
                        return RetValue;
                        //return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

                    }
                case SignInStatus.Failure:
                default:
                    {
                        retPost = new PostBackData("Invalid login attempt", 1);
                        RetValue.Data = retPost.getPostBack;
                        
                        return RetValue;
                    }
            }
                 
        }


        public static bool IsMobileDevice(string userAgent)
        {
            // TODO: null check
            userAgent = userAgent.ToLower();
            return mobileDevices.Any(x => userAgent.Contains(x));
        }

        private static string[] mobileDevices = new string[] {"iphone","ppc","android",
                                                      "windows ce","blackberry",
                                                      "opera mini","mobile","android","windows phone","palm",
                                                      "portable","opera mobi" };


        async public Task<UserAccountDirect> LoginStatic(LoginViewModel model)
        {
            
            UserAccountDirect RetValue = null;
            string LoopBackUrl = "";
            
            if (!ModelState.IsValid)
            {
                string AllErrors = string.Join("\n", ModelState.Values.SelectMany(obj => obj.Errors.Select(obj1 => obj1.ErrorMessage)));
                return RetValue;
            }



            

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    {
                        UserController myUserCtrl = new UserController();
                        ApplicationUser SessionUser = await myUserCtrl.GetUser(User.Identity.GetUserId(), User.Identity.GetUserName());
                        RetValue = new UserAccountDirect(SessionUser);
                        return RetValue;   
                    }
                default:
                    {
                        return RetValue;
                    }
            }

        }




        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Desktop", "Account");
                    //return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
               "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            return callbackUrl;
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                int Min=Convert.ToInt32(model.TimeZoneOffSet);
                TimeSpan OffSet = TimeSpan.FromMinutes(Min);
                DateTimeOffset EndOfDay = new DateTimeOffset(2014, 1, 1, 22, 0, 0, OffSet);
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName, LastChange = EndOfDay.UtcDateTime};
                var logGenerationresult = await generateLog(user);
                //var result = logGenerationresult.Item1;
                //if (result.Succeeded)
                //{
                    var result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        /*
                        if (logGenerationresult.Item2 > 0)
                        {
                            string CurrentLogLocation = LogControl.getLogLocation();
                            DBControl newDB = new DBControl(model.Username, logGenerationresult.Item2);
                            LogControl OldLog = new LogControl(newDB);
                            string OldLogLocation = BundleConfig.OldLog;
                            LogControl.UpdateLogLocation(OldLogLocation);
                            UserAccount OldUserAccount = new UserAccount(model.Username, logGenerationresult.Item2);
                            await OldUserAccount.Login();
                            newDB.deleteUser();
                            await OldUserAccount.DeleteLog();
                            LogControl.UpdateLogLocation(CurrentLogLocation);
                        }
                        */
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account",
                           new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id,
                           "Confirm your account", "Please confirm your account by clicking <a href=\""
                           + callbackUrl + "\">here </a>");

                        

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                        
                        //return RedirectToAction("Index", "Home");
                        return RedirectToAction("Desktop", "Account");
                    }
                    else
                    {
                        LogControlDirect LogToBedeleted = new LogControlDirect(user, "", true);
                        await LogToBedeleted.DeleteLog();
                    }
                //}
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> SignUp(RegisterViewModel model)
        {

            PostBackData retPost = new PostBackData("Failed to register user", 3);
            JsonResult RetValue = new JsonResult();
            if (ModelState.IsValid)
            {
                int Min = Convert.ToInt32(model.TimeZoneOffSet);
                TimeSpan OffSet = TimeSpan.FromMinutes(Min);
                DateTimeOffset EndOfDay = new DateTimeOffset(2014, 1, 1, 22, 0, 0, OffSet);
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email, FullName = model.FullName, LastChange = EndOfDay.UtcDateTime };
                var logGenerationresult = await generateLog(user);
                var result = logGenerationresult.Item1;
                if (result.Succeeded)
                {
                    result = await UserManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        /*
                        if (logGenerationresult.Item2 > 0)
                        {
                            string CurrentLogLocation = LogControl.getLogLocation();
                            DBControl newDB = new DBControl(model.Username, logGenerationresult.Item2);
                            LogControl OldLog = new LogControl(newDB);
                            string OldLogLocation = BundleConfig.OldLog;
                            LogControl.UpdateLogLocation(OldLogLocation);
                            UserAccount OldUserAccount = new UserAccount(model.Username, logGenerationresult.Item2);
                            await OldUserAccount.Login();
                            newDB.deleteUser();
                            await OldUserAccount.DeleteLog();
                            LogControl.UpdateLogLocation(CurrentLogLocation);
                        }
                        */
                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account",
                           new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id,
                           "Confirm your account", "Please confirm your account by clicking <a href=\""
                           + callbackUrl + "\">here </a>");

                        string LoopBackUrl = "";
                        if (Request.Browser.IsMobileDevice)
                        {
                            LoopBackUrl = "/Account/Mobile";
                        }
                        else
                        {
                            LoopBackUrl = "/Account/Desktop";
                        }
                        
                        retPost = new PostBackData(LoopBackUrl, 0);
                        RetValue.Data = (retPost.getPostBack);
                        return RetValue;


                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    }
                    else
                    {
                        LogControlDirect LogToBedeleted = new LogControlDirect(user,"",true);
                        await LogToBedeleted.DeleteLog();
                    }
                }
                retPost = new PostBackData(string.Join("\n", result.Errors), 3);
                RetValue.Data= (retPost.getPostBack);
                return RetValue;
            }
            string AllErrors = string.Join("\n", ModelState.Values.SelectMany(obj=>obj.Errors.Select(obj1=>obj1.ErrorMessage)));
            retPost = new PostBackData(AllErrors, 3);
            RetValue.Data = (retPost.getPostBack);

            return RetValue;
        }



        async Task<Tuple<IdentityResult,int>> generateLog(ApplicationUser model)
        {
            Tuple<bool, int> ExistInOldDB = DBControl.doesUserExistInOldDB(model.UserName);
            string CurrentLogLocation = LogControl.getLogLocation();
            bool NewLogCreationSuccess=false;
            Tuple<IdentityResult, int> retValue;
            int OldID = -1;
            if (ExistInOldDB.Item1)
            {
                /*
                OldID = ExistInOldDB.Item2;
                DBControl newDB = new DBControl(model.UserName, model.PasswordHash);
                LogControl OldLog = new LogControl(newDB);

                
                string OldLogLocation = BundleConfig.OldLog;
                LogControl.UpdateLogLocation(OldLogLocation);
                UserAccount OldUserAccount = new UserAccount(model.UserName, ExistInOldDB.Item2);
                await OldUserAccount.Login();
                ///*
                if (OldUserAccount.Status)
                {
                    Task<Tuple<Dictionary<string, TilerElements.CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location_Elements>>> Task_profileData = OldUserAccount.ScheduleData.getProfileInfo();
                    Tuple<Dictionary<string, TilerElements.CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location_Elements>> profileData = await Task_profileData;
                    //OldUserAccount.DeleteLog();

                    LogControlDirect newLog = new LogControlDirect(model, CurrentLogLocation);
                    newLog.genereateNewLogFile(model.Id);
                    newLog.UpdateReferenceDay(profileData.Item2);
                    NewLogCreationSuccess = await newLog.WriteToLog(profileData.Item1.Values, OldUserAccount.LastEventTopNodeID.ToString());
                }
                
                //*/
            }
            else
            {

                LogControlDirect newLog = new LogControlDirect(model, CurrentLogLocation);
                UserAccountDirect newUser = new UserAccountDirect(model);
                List<string> NameDist = model.FullName.Split().ToList();
                Task< TilerElements.CustomErrors> registerStatus =  newUser.Register(model);
                NewLogCreationSuccess =! (await registerStatus).Status;
            }

            if (NewLogCreationSuccess)
            {
                retValue = new Tuple<IdentityResult, int>(IdentityResult.Success, OldID);
                return retValue;
            }

            retValue = new Tuple<IdentityResult, int>(IdentityResult.Failed("Registration is currently unavailable"), OldID);

            return retValue;
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }
        

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [Authorize]
        public ActionResult Desktop()
        {
            ViewBag.Message = "Welcome To Tiler";
            return View();
        }


        [Authorize]
        public ActionResult Mobile()
        {
            ViewBag.Message = "Welcome To Tiler";
            return View();
        }



        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }



                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here </a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
                
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("Desktop", "Account");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    if (string.IsNullOrEmpty( loginInfo.Email))
                    {
                        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
                    }
                    else
                    {
                        return await ExternalLoginConfirmation(new ExternalLoginConfirmationViewModel { Email = loginInfo.Email }, returnUrl);
                    }
                    
                    //return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Desktop", "Account");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = info.ExternalIdentity.Name, LastChange = DateTime.Now };


                var result = await UserManager.CreateAsync(user);
                
                
                if (result.Succeeded)
                {
                    var logGenerationresult = await generateLog(user);
                    var LogCreationresult = logGenerationresult.Item1;
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded && LogCreationresult.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        var callbackUrl = Url.Action("ConfirmEmail", "Account",
                           new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id,
                           "Confirm your account", "Please confirm your account by clicking <a href=\""
                           + callbackUrl + "\">here </a>");
                        
                        
                        return RedirectToAction("Desktop", "Account");
                    }
                    else
                    {
                        LogControlDirect LogToBedeleted = new LogControlDirect(user);
                        await LogToBedeleted.DeleteLog();
                    }
                }

                AddErrors(result);
                
                
                
                
                /*
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToAction("Desktop", "Account");
                    }
                }
                AddErrors(result);*/
            }

            ViewBag.ReturnUrl = returnUrl;
            //return View("ExternalLoginFailure");
            return View("ExternalLoginConfirmation", model);
        }


        [AllowAnonymous]
        public ActionResult LogOff(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return RedirectToAction("Index", "Home");
        }
        
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut();

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie); 

            Session.Abandon();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}