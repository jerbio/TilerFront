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
            {40001000,"Publication Error."}//Just testing
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
            Data = Error.Message;
            Status = Error.Code;
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
                TilerFront.Models.PostError retPostError = new TilerFront.Models.PostError() { code = this.Status.ToString(), Message = getErrorMessage(Status) };
                TilerFront.Models.PostBackStruct PostBackData = new TilerFront.Models.PostBackStruct { Error = retPostError, Content = this.Data };
                return PostBackData;
            }
        }

        public string getPostBackData
        {
            get
            {

                string retValue = "{\"Error\":{\"code\":\"" + Status + "\",\"Message\":\"" + getErrorMessage(Status) + "\"},\"Content\":" + Data + "}";
                return retValue;
            }
        }



        

        

    }
}