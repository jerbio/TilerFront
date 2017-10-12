using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TilerFront.Models;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

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

namespace TilerFront.Controllers
{
    public static class ThirdPartyCalendarAuthenticationModelsController// : Controller
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static string CurrentURI = null;
        private static bool isCurrentURIUpdated = false;

        static public async Task<bool> CreateGoogle(ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthentication)
        {
            bool RetValue = false;
            //if (ModelState.IsValid)
            {
                bool makeAdditionCall = false;
                //if (!deletGoogleCalendarFromLog)
                {
                        bool saveFailed;
                    do
                    {
                        saveFailed = false; 
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
                                RetValue = true;
                            }
                            else
                            {
                                db.ThirdPartyAuthentication.Add(thirdPartyCalendarAuthentication);
                                await db.SaveChangesAsync();
                                RetValue = true;
                                //if (!(await SendRequestForGoogleNotification(thirdPartyCalendarAuthentication).ConfigureAwait(false)))
                                //{
                                //    await deleteGoogleAccount(thirdPartyCalendarAuthentication.getThirdPartyOut()).ConfigureAwait(false);
                                //    RetValue=false;
                                //}
                            }
                        }

                        catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException  dbEx)
                        {
                            saveFailed = true;
                            dbEx.Entries.Single().Reload();

                            RetValue = false;
                            /*
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            {
                                foreach (var validationError in validationErrors.ValidationErrors)
                                {
                                    System.Console.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                                }
                            }*/
                        }
                    } while (saveFailed); 
                }
            }

            return RetValue;
        }

        public static void initializeCurrentURI(string URIEntry)
        {
            if(string.IsNullOrEmpty( CurrentURI))
            {
                CurrentURI = URIEntry;
                isCurrentURIUpdated = true;
                return;
            }
            //throw new Exception("You already initialized current URI");
        }


        static public async Task<bool> SendRequestForGoogleNotification(ThirdPartyCalendarAuthenticationModel AuthenticationData)
        {
            bool RetValue = false;
            try
            {

                var url = string.Format
                (
                    "https://www.googleapis.com/calendar/v3/calendars/{0}/events/watch",
                    AuthenticationData.Email
                );
                //url = "https://mytilerkid.azurewebsites.net/api/GoogleNotification/Trigger";
                var httpWebRequest = HttpWebRequest.Create(url) as HttpWebRequest;
                httpWebRequest.Headers["Authorization"] = string.Format("Bearer {0}", AuthenticationData.Token);
                httpWebRequest.Method = "POST";
                // added the character set to the content-type as per David's suggestion
                httpWebRequest.ContentType = "application/json; charset=UTF-8";
                httpWebRequest.CookieContainer = new CookieContainer();

                // replaced Environment.Newline by CRLF as per David's suggestion
                GoogleNotificationRequestModel NotificationRequest = AuthenticationData.getGoogleNotificationCredentials(CurrentURI);

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




                /*
                string url = "https://www.googleapis.com/calendar/v3/calendars/" + AuthenticationData.Email + "/events/watch";
                //url = "https://mytilerkid.azurewebsites.net/api/GoogleNotification/Trigger";
                var httpWebRequest = WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                string AuthorizationString = "Bearer " + AuthenticationData.Token;
                httpWebRequest.Headers.Add("Authorization", AuthorizationString);
                httpWebRequest.Method = "POST";

                GoogleNotificationRequestModel NotificationRequest = AuthenticationData.getGoogleNotificationCredentials(Ctx.Request.Url.Authority);

                
                //GoogleNotificationWatchResponseModel testGoogleResponse = new GoogleNotificationWatchResponseModel();
                //testGoogleResponse.expiration = "98989";
                //testGoogleResponse.id = "jkhj2hkjhkjh";
                //testGoogleResponse.kind = "99898989";
                //testGoogleResponse.resourceUri= "hjhj98878";
                //string JsonString = JsonConvert.SerializeObject(testGoogleResponse);
                
                
                
                string JsonString = JsonConvert.SerializeObject(NotificationRequest);
                
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = JsonString;
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                }
                RetValue = true;
                */
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }

            return RetValue;
        }

        static public async Task<bool> deleteGoogleAccount(ThirdPartyAuthenticationForView modelData)
        {
            bool RetValue = false;
            ThirdPartyCalendarAuthenticationModel ThirdPartyAuth = db.ThirdPartyAuthentication.Where(obj => obj.ID == modelData.ID).Single();

            if (ThirdPartyAuth != null)
            {
                UserCredential googleCredential = ThirdPartyAuth.getGoogleOauthCredentials();
                try
                {
                    await googleCredential.RefreshTokenAsync(CancellationToken.None).ConfigureAwait(false);
                    await googleCredential.RevokeTokenAsync(CancellationToken.None).ConfigureAwait(false);
                    db.ThirdPartyAuthentication.Remove(ThirdPartyAuth);
                    RetValue = true;
                }
                catch (Exception e)
                {

                }
                //await ThirdPartyAuth.getGoogleOauthCredentials().RevokeTokenAsync(CancellationToken.None).ConfigureAwait(false);

                await db.SaveChangesAsync().ConfigureAwait(false);
            }

            return RetValue;
        }

        /*
        // GET: ThirdPartyCalendarAuthenticationModels
        public async Task<ActionResult> Index()
        {
            return View(await db.ThirdPartyAuthentication.ToListAsync());
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TilerID,Email,ProviderID,ID,isLongLived,Token,RefreshToken,Deadline")] ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel)
        {
            if (ModelState.IsValid)
            {
                db.ThirdPartyAuthentication.Add(thirdPartyCalendarAuthenticationModel);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TilerID,Email,ProviderID,ID,isLongLived,Token,RefreshToken,Deadline")] ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thirdPartyCalendarAuthenticationModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // GET: ThirdPartyCalendarAuthenticationModels/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }

        // POST: ThirdPartyCalendarAuthenticationModels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(id);
            db.ThirdPartyAuthentication.Remove(thirdPartyCalendarAuthenticationModel);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Authenticate(string TilerID, string Email, string Provider)
        {
            Object[] ParamS = { TilerID, Email, Convert.ToInt32( Provider )};
            ThirdPartyCalendarAuthenticationModel thirdPartyCalendarAuthenticationModel = await db.ThirdPartyAuthentication.FindAsync(ParamS);
            if (thirdPartyCalendarAuthenticationModel == null)
            {
                return HttpNotFound();
            }
            return View(thirdPartyCalendarAuthenticationModel);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        */
    }
}
