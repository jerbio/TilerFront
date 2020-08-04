using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using System.Data.Entity;
using DBTilerElement;
using TilerElements;
using System.Configuration;

namespace TilerFront.Models
{
    public class ThirdPartyCalendarAuthenticationModel
    {
        public string ID { get; set; }
        [Key]
        [Column(Order=1)]
        public string TilerID { get; set; }
        public bool isLongLived { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        protected string _ProviderID { get; set; }
        [Key]
        [Column(Order = 3)]
        public string ProviderID {
            get {
                return _ProviderID;
            }
            set {
                ThirdPartyControl.CalendarTool calendarType = Utility.ParseEnum<ThirdPartyControl.CalendarTool>(value.ToLower());
                _ProviderID = calendarType.ToString();
            }
        }
        public DateTimeOffset Deadline { get; set; }

        public string DefaultLocationId { get; set; }
        [ForeignKey("DefaultLocationId")]
        public TilerElements.Location DefaultLocation { get; set; }
        /// <summary>
        /// Converts "this" object to "ThirdPartyAuthenticationForView" object. This is to be used to generate the more secure information on the user for the management of third party accounts on the front end
        /// </summary>
        /// <returns></returns>
        public ThirdPartyAuthenticationForView getThirdPartyOut()
        {
            ThirdPartyAuthenticationForView RetValue = new ThirdPartyAuthenticationForView() { Email = this.Email, ProviderName =this.ProviderID, ID=this.ID, DefaultLocation=this.DefaultLocation };
            return RetValue;
        }


        public DB_TilerUser getTilerUser()
        {
            DB_TilerUser retValue = new DB_TilerUser()
            {
                CalendarType = this.ProviderID,
                Id = TilerID,
                isNull = false,
                ThirdPartyId = Email
            };
            return retValue;
        }
        /// <summary>
        /// function generates the google oauth credentials
        /// </summary>
        /// <returns></returns>
        public Google.Apis.Auth.OAuth2.UserCredential getGoogleOauthCredentials()
        {
            Google.Apis.Auth.OAuth2.Responses.TokenResponse responseData = new Google.Apis.Auth.OAuth2.Responses.TokenResponse();
            responseData.AccessToken = this.Token;
            responseData.RefreshToken = this.RefreshToken;
            GoogleAuthorizationCodeFlow.Initializer myInit = new GoogleAuthorizationCodeFlow.Initializer();
            AuthorizationCodeFlow.Initializer codeFlowIntial = myInit;
            codeFlowIntial.ClientSecrets = new ClientSecrets();
            string googleClientId = ConfigurationManager.AppSettings["googleClientId"];
            string googleClientSecret = ConfigurationManager.AppSettings["googleClientSecret"];

            codeFlowIntial.ClientSecrets.ClientId = googleClientId;
            codeFlowIntial.ClientSecrets.ClientSecret = googleClientSecret;
            IAuthorizationCodeFlow myFlow = AppFlowMetadata.getFlow();
            Google.Apis.Auth.OAuth2.UserCredential RetValue = new Google.Apis.Auth.OAuth2.UserCredential(myFlow, this.ID, responseData);
            return RetValue;
        }

        /// <summary>
        /// Function refreshes the token and update "this" object. Note it does not try to persist the authentication to permanent storage on azure. If you want it to persist call function refreshAndCommitToken;
        /// </summary>
        /// <returns></returns>
        public async Task<bool> refreshAuthenticationToken()
        {
            Google.Apis.Auth.OAuth2.UserCredential OldCredentials = getGoogleOauthCredentials();
            System.Threading.CancellationToken CancelToken = new System.Threading.CancellationToken();
            bool refreshSuccess =await  OldCredentials.RefreshTokenAsync(CancelToken).ConfigureAwait(false);
            if(refreshSuccess )
            {
                this.ID = OldCredentials.UserId;
                this.Token = OldCredentials.Token.AccessToken;
                this.RefreshToken = OldCredentials.Token.RefreshToken;
                double totalSeconds = OldCredentials.Token.ExpiresInSeconds == null ? 0 : (double)OldCredentials.Token.ExpiresInSeconds;
                DateTime myDate = OldCredentials.Token.Issued.AddSeconds(totalSeconds);
                this.Deadline = new DateTimeOffset(myDate);
            }

            return refreshSuccess;
        }


        //refreshes the token credentials and updates tiler DB with the data
        async public Task<bool> refreshAndCommitToken(ApplicationDbContext db)
        {
            bool RetValue =await refreshAuthenticationToken().ConfigureAwait(false);
            if (RetValue)
            {
                Object[] Param = { this.TilerID, this.Email, this.ProviderID };
                ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = await db.ThirdPartyAuthentication.FindAsync(Param);
                if (checkingThirdPartyCalendarAuthentication != null)
                {
                    checkingThirdPartyCalendarAuthentication.ID = this.ID;
                    checkingThirdPartyCalendarAuthentication.Token = this.Token;
                    checkingThirdPartyCalendarAuthentication.RefreshToken = this.RefreshToken;
                    checkingThirdPartyCalendarAuthentication.Deadline = this.Deadline;
                    db.Entry(checkingThirdPartyCalendarAuthentication).State = EntityState.Modified;
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
                else
                {
                    db.ThirdPartyAuthentication.Add(this);
                    await db.SaveChangesAsync();
                }
            }
            return RetValue;
        }


        /// <summary>
        /// This deletes this authentication tokens from persistent storage. Note this uses the data members from "this" object. It revokes the credentials then commits them.
        /// </summary>
        /// <returns></returns>
        async public Task<bool> unCommitAuthentication()
        {
            bool RetValue = false;    

            UserCredential googleCredential = getGoogleOauthCredentials();
            Object[] Param = { this.TilerID, this.Email, this.ProviderID };
            try
            { 
                await googleCredential.RefreshTokenAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
                await googleCredential.RevokeTokenAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
                
                RetValue = true;
            }
            catch(Exception e)
            {
                ;
            }
            finally
            {
                ApplicationDbContext db = new ApplicationDbContext();
                ThirdPartyCalendarAuthenticationModel checkingThirdPartyCalendarAuthentication = db.ThirdPartyAuthentication.Find(Param);
                if (checkingThirdPartyCalendarAuthentication != null)
                {
                    db.ThirdPartyAuthentication.Remove(checkingThirdPartyCalendarAuthentication);
                    db.SaveChanges();
                }
            }
            return RetValue;
        }

        public async Task revokeAccess ()
        {
            UserCredential googleCredential = getGoogleOauthCredentials();
            await googleCredential.RevokeTokenAsync(System.Threading.CancellationToken.None).ConfigureAwait(false);
        }

        public GoogleNotificationRequestModel getGoogleNotificationCredentials(string NotificationEndPoint)
        {
            GoogleNotificationRequestModel RetValue = new GoogleNotificationRequestModel();
            RetValue.id = this.ID;
            RetValue.type = "web_hook";
            RetValue.address = "https://" + NotificationEndPoint + "/api/GoogleNotification/Trigger";
            //RetValue.token = "";
            //RetValue.expiration= "";
            return RetValue;
        }

        

        
    }

}