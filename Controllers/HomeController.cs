using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;


namespace TilerFront.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return View();
            ///*
            if (User.Identity.IsAuthenticated)
            {
                if (Request.Browser.IsMobileDevice)
                {
                    return RedirectToAction("Mobile", "Account");
                }
                else
                {
                    return RedirectToAction("Desktop", "Account");
                }

            }
            else
            {
                return View();
            }
            
            //*/
            
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new AccountController.ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Claims()
        {
            ViewBag.Message = "Your claims page.";

            ViewBag.ClaimsIdentity = Thread.CurrentPrincipal.Identity;

            return View();
        }
    }
}