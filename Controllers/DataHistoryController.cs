using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
using BigDataTiler;
using TilerFront.Models;

namespace TilerFront.Controllers
{
    [Authorize]
    public class DataHistoryController : TilerApiController
    {
        // GET: DataHistory
        [HttpGet]
        [Route("api/DataHistory/User")]
        public async Task<HttpResponseMessage> Index([FromUri]DataHistoryModel UserData)
        {
            BigDataLogControl bigDataControl = new BigDataLogControl();
            var timeline = UserData.TimeLine;
            var logChanges = await bigDataControl.getLogChangesByType(UserData.UserID, timeline, UserData.ActivityType).ConfigureAwait(false);
            byte[] returnedData = null;

            using (var returnedMemoryStream = new MemoryStream())
            {
                using (var returnedArchive = new ZipArchive(returnedMemoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (LogChange logChange in logChanges)
                    {
                        byte[] zippedLog = logChange.ZippedLog;

                        string entryFileName = logChange.JsTimeOfCreation + "_" + logChange.TypeOfEvent + "_" + logChange.JsTimeOfCreation;


                        MemoryStream zippedXmlMemoryStream = new MemoryStream(logChange.ZippedLog);

                        using (var zip = new ZipArchive(zippedXmlMemoryStream, ZipArchiveMode.Read))
                        {
                            foreach (var entry in zip.Entries)
                            {
                                using (var unzippedXmlStream = entry.Open())
                                {
                                    var copiedLogEntry = returnedArchive.CreateEntry(entryFileName + ".xml", CompressionLevel.Optimal);
                                    using (var entryStream = copiedLogEntry.Open())
                                    {
                                        unzippedXmlStream.CopyTo(entryStream);
                                        entryStream.Write(zippedLog, 0, zippedLog.Length);
                                        entryStream.Flush();
                                    }


                                }
                            }
                        }
                    }
                }
                returnedData = returnedMemoryStream.ToArray();
            }
            


            HttpResponseMessage retValue;
            if (returnedData!=null && returnedData.Length > 0)
            {
                string fileName = UserData.UserName + "_" + UserData.UserActivityType +"_"+ DateTimeOffset.Now.ToUnixTimeMilliseconds() + ".zip";
                retValue = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(returnedData)
                };
                retValue.Content.Headers.ContentDisposition =
                    new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                retValue.Content.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                return retValue;
            } else
            {
                retValue = new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return retValue;

        }
    }
}