using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements;

namespace TilerFront
{
    public class PostBackData
    {
        /*
         * Class formats data to be sent back to client.
         * Error Codes
         * 0: No issues
         * 1: Failed To Login with User
         * 2: Unknown error Tiler Error
         * 3: User registration error
         */
        


        static Dictionary<int, string> errorDictionary = new Dictionary<int, string>()
        {
            {0,""},
            {1,"Failed To Login with User"},
            {2,"Unknown Tiler Error"},
            {3,"Error Registering user, Invalid Credentials"},
            {4,"User cannot be validated"},
            {5,"Invalid Event ID"},
            {6,"Passwords do not match"},
            {7,"User Has Been Locked out"},
            {8,"User needs to be Verified"},
            {10001000,"User Already exists"},
            {30001000,"Registration exception. Check DB control with user credentials"},
            {40001000,"Publication Error."},//Just testing
            {50005000,"Pause/Resume Bug."},//Just testing
            {100,"I have no idea"}
        };
        dynamic Data;
        int Status=0;
        string Message;

        public PostBackData(int StatusEntry)
        {
            Message = "";
            Status = StatusEntry;
        }


        public PostBackData(string PostMessage,  int StatusEntry)
        {
            Message = PostMessage;
            Status = StatusEntry;
        }

        public PostBackData(CustomErrors Error)
        {
            Status = 0;
            Data = "Success";
            if (Error != null)
            {
                Data = Error.Message;
                Status = Error.Code;
            }
            
        }

        public PostBackData(dynamic DataEntry, int StatusEntry)
        {
            Data = DataEntry;
            Status = StatusEntry;
        }

        string getErrorMessage(int errorCode)
        {
            string retValue;
            if (errorCode >= 20000000)
            {
                retValue = "Tiler is having some issues please try again later";
                return retValue;
            }

            if (string.IsNullOrEmpty(Message))
            {
                retValue = errorDictionary[errorCode];
            }
            else
            {
                retValue = Message;
            }

            return retValue;
        }

        public TilerFront.Models.PostBackStruct getPostBack
        { 
            get
            {
                
                TilerFront.Models.PostError retPostError = new TilerFront.Models.PostError() { code = this.Status, message = getErrorMessage(Status) };
                TilerFront.Models.PostBackStruct PostBackData = new TilerFront.Models.PostBackStruct { Error = retPostError, content = this.Data };

                return PostBackData;
            }
        }

        public string getPostBackData
        {
            get
            {
                JObject retValueJson = new JObject();
                JObject error = new JObject();
                error.Add("code", Status);
                error.Add("message", getErrorMessage(Status));
                retValueJson.Add("content", Data);
                string retValue = "{\"Error\":{\"code\":" + Status + ",\"message\":\"" + getErrorMessage(Status) + "\"},\"content\":" + Data + "}";
                retValue = retValueJson.ToString();
                return retValue;
            }
        }



        

        

    }
}