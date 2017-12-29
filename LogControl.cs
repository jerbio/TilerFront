//#define UseDefaultLocation

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using DBTilerElement;
using TilerElements;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Data.Entity;
using TilerFront.Models;
using BigDataTiler;
#if ForceReadFromXml
#else
using CassandraUserLog;
using TilerSearch;
#endif



namespace TilerFront
{
    public class LogControl
    {
        protected ApplicationDbContext Database;
        protected string ID;
        protected string UserName;
        string NameOfUser;
        protected static string WagTapLogLocation = "WagTapCalLogs\\";
        protected static string BigDataLogLocation = "BigDataLogs\\";
        //protected DBControl LogDBDataAccess;
        protected long LastIDNumber;
        protected string CurrentLog;
        protected bool LogStatus;
        protected bool UpdateLocaitionCache = false;
        protected Dictionary<string, TilerElements.Location> CachedLocation;
        protected TilerElements.Location DefaultLocation= new TilerElements.Location();
        protected TilerElements.Location NewLocation;
        protected DB_UserActivity activity;
        Dictionary<string, Func<XmlNode, Reason>> createDictionaryOfOPtionToFunction;
        protected TilerUser _TilerUser;
        protected BigDataTiler.BigDataLogControl bigdataControl = new BigDataLogControl();

#if ForceReadFromXml
#else
        protected CassandraUserLog.CassandraLog myCassandraAccess;
        protected TilerSearch.EventNameSearchHandler NameSearcher;
        protected LocationSearchHandler LocationSearcher;
#endif
        Tuple<bool, string, DateTimeOffset, long> ScheduleMetadata;
        
#if ForceReadFromXml
#else
        public static bool useCassandra=true;
#endif

        protected LogControl()
        { 
            ID="";
            UserName="";
            NameOfUser="";
            LastIDNumber = 0;
            CurrentLog="";
            LogStatus=false;
#if ForceReadFromXml
#else
            NameSearcher = new EventNameSearchHandler();
            LocationSearcher = new LocationSearchHandler();
#endif
            Dictionary<string, TilerElements.Location> CachedLocation = new Dictionary<string, TilerElements.Location>();

            createDictionaryOfOPtionToFunction = new Dictionary<string, Func<XmlNode, Reason>>
            {
                {Reason.Options.BestFit.ToString(),(XmlNode node)=> { return  getBestFitReason(node); } },
                {Reason.Options.PreservedOrder.ToString(),(XmlNode node)=> { return  getPreservedOrderReason(node); } },
                {Reason.Options.Weather.ToString(), (XmlNode node)=> { return  getWeatherReason(node); } },
                {Reason.Options.CloseToCluster.ToString(), (XmlNode node)=> { return  getLocationReason(node); } },
                {Reason.Options.Occupancy.ToString(), (XmlNode node)=> { return  getOccupancyReason(node); } },
                {Reason.Options.RestrictedEvent.ToString(), (XmlNode node)=> { return  getRestrictedEventReason(node); } }
            };
        }

        public LogControl(TilerUser user, ApplicationDbContext database, string logLocation = "", DB_UserActivity useractivity = null)
        {
            if (!string.IsNullOrEmpty(logLocation))
            {
                WagTapLogLocation = logLocation;
            }
            //LogDBDataAccess = DBAccess;
            LogStatus = false;
            CachedLocation = new Dictionary<string, TilerElements.Location>();
            _TilerUser = user;
            Database = database;
        }
        #region Functions
        virtual async public Task Initialize()
        {
            CurrentLog = "";

            CurrentLog = ID.ToString() + ".xml";
            string LogDir = (WagTapLogLocation + CurrentLog);
            string myCurrDir = Directory.GetCurrentDirectory();
            Console.WriteLine("Log DIR is:" + LogDir);
            LogStatus = File.Exists(LogDir);

            _TilerUser = Database.Users.Find(ID);
            UserName = _TilerUser.UserName;
            NameOfUser = _TilerUser.FullName;
        }


        public static void UpdateLogLocation(string LogLocation)
        {
            WagTapLogLocation = LogLocation;
        }

        public static void UpdateBigDataLogLocation(string bigLogLocation)
        {
            BigDataLogLocation = bigLogLocation;
        }
        public static string getLogLocation()
        {
            return WagTapLogLocation;
        }

        /// <summary>
        /// Function returns the tiler User retrieved from the db
        /// </summary>
        /// <returns></returns>
        public TilerUser getTilerRetrievedUser()
        {
            return _TilerUser;
                 
        }

        #region Write Data

        public async Task Undo(string LogFile = "")
        {
            Task<bool> retValue;

#if ForceReadFromXml
#else
            if (useCassandra)
            {
                retValue =  myCassandraAccess.Commit(AllEvents);
                LogDBDataAccess.WriteLatestData(DateTime.Now,Convert.ToInt64( LatestID), ID);
                bool boolRetValue = await retValue;
                return boolRetValue;
            }
#endif



            retValue = new Task<bool>(() => { return true; });
            retValue.Start();
            string LogFileCopy = "";
            if (LogFile == "")
            {
                LogFile = WagTapLogLocation + CurrentLog;
                LogFileCopy = WagTapLogLocation + "Copy_" + CurrentLog;
            }



            XmlDocument xmldoc = new XmlDocument();
            XmlDocument xmldocCopy = new XmlDocument();
            xmldoc.Load(LogFile);
            try
            {
                xmldocCopy.Load(LogFileCopy);
            }
            catch
            {
                try
                {
                    genereateNewLogFile("Copy_" + ID);
                    xmldocCopy.Load(LogFileCopy);
                }
                catch (Exception e)
                {
                    Console.Write(e.Message);
                }

            }

            xmldoc.InnerXml = xmldocCopy.InnerXml;
            int loopCounter = 0;
            while (true)
            {
                try
                {
                    Task dbLatestChange = null;
                    if (!string.IsNullOrEmpty(_TilerUser.PasswordHash))
                    {
                        dbLatestChange = TilerController.saveLatestChange(Database, _TilerUser);
                    }
                    xmldoc.Save(LogFile);
                    xmldocCopy.Save(LogFileCopy);
                    await updateBigData(xmldocCopy, xmldoc).ConfigureAwait(false);
                    if (dbLatestChange != null) {
                        await dbLatestChange;
                    }
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);

                    if (++loopCounter > 3)
                    {
                        throw new TimeoutException("Failed to open files for undo");
                    }
                }
            }

        }

        public CustomErrors genereateNewLogFile(string UserID)//creates a new xml log file. Uses the passed UserID
        {

            CustomErrors retValue = null;
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                return retValue;
            }
#endif
            try
            {

                string NameOfFile = WagTapLogLocation + UserID + ".xml";
                if (File.Exists(NameOfFile))
                {
                    File.Delete(NameOfFile);
                }

                FileStream myFileStream = File.Create(NameOfFile);
                myFileStream.Close();

                CurrentLog = UserID + ".xml";
                EmptyCalendarXMLFile(NameOfFile);
                //EmptyCalendarXMLFile();
            }
            catch (Exception e)
            {
                retValue = new CustomErrors("Error generating log\n" + e.ToString(), 20000000);
            }

            return retValue;

        }

        public async Task<CustomErrors> DeleteLog()
        {
            CustomErrors retValue = null;
            try
            {
                string NameOfFile = WagTapLogLocation + CurrentLog;
                File.Delete(NameOfFile);
            }
            catch (Exception e)
            {
                retValue = new CustomErrors(e.ToString(), 20002000);
            }

            return retValue;
        }

        public void updateUserActivty(UserActivity activity)
        {
            this.activity = new DB_UserActivity(activity);
        }

        /// <summary>
        /// Function is to be called for the generation of auto logs for a user interaction with the schedule
        /// </summary>
        /// <param name="oldData"></param>
        /// <param name="newData"></param>
        public async Task updateBigData(XmlDocument oldData, XmlDocument newData)
        {
            bool corruptZipFile = false;
            string zipFile = LoggedUserID + ".zip";
            string zipFolder = LoggedUserID;

            string fullZipPath = @BigDataLogLocation + zipFile;
            try
            {
                if (activity == null)
                {
                    activity = new DB_UserActivity(DateTimeOffset.UtcNow, UserActivity.ActivityType.None);
                }
                XmlDocument combinedDoc = new XmlDocument();

                XmlNode timeOfCreationNode = combinedDoc.CreateElement("TimeOfCreation");
                XmlNode bigDataLog = combinedDoc.CreateElement("BigDataLog");
                XmlNode miscDataLog = combinedDoc.CreateElement("MiscData");
                miscDataLog.InnerText = activity.getMiscdata();

                timeOfCreationNode.InnerXml = activity.ToXML();
                bigDataLog.AppendChild(timeOfCreationNode);
                bigDataLog.AppendChild(miscDataLog);
                XmlNode beforeProcessing = combinedDoc.CreateElement("BeforeProcessing");
                XmlNode importedNBeforeProcessingNode = combinedDoc.ImportNode(oldData.DocumentElement as XmlNode, true);
                beforeProcessing.PrependChild(importedNBeforeProcessingNode);

                XmlNode afterProcessing = combinedDoc.CreateElement("AfterProcessing");
                XmlNode importedNAfterProcessingNode = combinedDoc.ImportNode(newData.DocumentElement as XmlNode, true);
                afterProcessing.PrependChild(importedNAfterProcessingNode);

                bigDataLog.AppendChild(beforeProcessing);
                bigDataLog.AppendChild(afterProcessing);

                XmlDeclaration xmldecl;
                xmldecl = combinedDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                combinedDoc.AppendChild(bigDataLog);
                XmlElement root = combinedDoc.DocumentElement;
                combinedDoc.InsertBefore(xmldecl, root);
                DateTimeOffset timeOfCreation = DateTimeOffset.UtcNow;
                LogChange log = new LogChange()
                {
                    Id = Guid.NewGuid().ToString(),
                    TimeOfCreation = timeOfCreation,
                    JsTimeOfCreation = timeOfCreation.toJSMilliseconds(),
                    TypeOfEvent = activity.TriggerType.ToString(),
                    UserId = _TilerUser.Id
                };
                log.loadXmlFile(combinedDoc);
                await bigdataControl.AddLogDocument(log).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if(corruptZipFile)
                {
                    File.Delete(fullZipPath);
                }
                CustomErrors retValue = new CustomErrors("Error generating bigdata log\n" + e.ToString(), 20000000);
            }
        }

        virtual public void UpdateReferenceDayInXMLLog(DateTimeOffset referenceDay, string LogFile = "")
        {
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                return;
            }
#endif
            if (LogFile == "")
            { LogFile = WagTapLogLocation + CurrentLog; }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(LogFile);
            XmlElement refDayNode = xmldoc.CreateElement("referenceDay");
            refDayNode.InnerText = referenceDay.ToString();
            XmlNode refNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/referenceDay");
            if (refNode == null)
            {
                XmlNode myNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog");
                myNode.AppendChild(refDayNode);
            }
            else
            {
                refNode.InnerText = refDayNode.InnerText;
            }
            xmldoc.Save(LogFile);
            return;
        }

        async public Task<bool> WriteToLogOld(IEnumerable<CalendarEvent> AllEvents, string LatestID, string LogFile = "")
        {
            Task<bool>  retValue;



            retValue = new Task<bool>(() => { return true; });
            retValue.Start();
            string LogFileCopy = "";
            if (LogFile == "")
            { 
                LogFile = WagTapLogLocation + CurrentLog;
                LogFileCopy = WagTapLogLocation + "Copy_" + CurrentLog;
            }

            

            XmlDocument xmldoc = new XmlDocument();
            XmlDocument xmldocCopy = new XmlDocument();
            xmldoc.Load(LogFile);
            try
            {
                xmldocCopy.Load(LogFileCopy);
            }
            catch
            {
                try
                {
                    genereateNewLogFile("Copy_" + ID);
                    xmldocCopy.Load(LogFileCopy);
                }
                catch(Exception e)
                {
                    Console.Write(e.Message);
                }
            }


            xmldocCopy.InnerXml = xmldoc.InnerXml;
            CachedLocation = await getLocationCache().ConfigureAwait(false); ;//populates with current location info
            Dictionary<string, TilerElements.Location> OldLocationCache = new Dictionary<string, TilerElements.Location>(CachedLocation);
            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = LatestID;
            XmlNodeList EventSchedulesNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules");
            
            XmlNode EventSchedulesNodesNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            XmlNode EventSchedulesNodesNodeCpy = xmldoc.CreateElement("NodeCopy");
            EventSchedulesNodesNodeCpy.InnerXml = EventSchedulesNodesNode.InnerXml;
            EventSchedulesNodesNode.RemoveAll();
            XmlNodeList EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");
            bool errorWritingFile = false;
            CalendarEvent ErrorEvent = new CalendarEvent();
            EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");
            try
            {
                foreach (CalendarEvent MyEvent in AllEvents)
                {
                    {
                        XmlElement EventScheduleNode;
                        ErrorEvent = MyEvent;
                        EventScheduleNode = CreateEventScheduleNode(MyEvent);
                
                        XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
                        //(EventScheduleNode, true);
                        if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.getId, EventScheduleNode))
                        {
                            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules").AppendChild(MyImportedNode);
                        }
                        else
                        {
                            ;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                EventSchedulesNodesNode.InnerXml = EventSchedulesNodesNodeCpy.InnerXml;
                errorWritingFile = true;
            }

            UpdateCacheLocation(xmldoc, OldLocationCache,NewLocation);
            int loopCounter = 0;
            while (true)
            {
                try
                {
                    Task dbLatestChange = null;
                    if (!string.IsNullOrEmpty(_TilerUser.PasswordHash))
                    {
                        dbLatestChange = TilerController.saveLatestChange(Database, _TilerUser);
                    }
                    xmldoc.Save(LogFile);
                    xmldocCopy.Save(LogFileCopy);
                    await updateBigData(xmldocCopy, xmldoc).ConfigureAwait(false);
                    if (dbLatestChange != null)
                    {
                        await dbLatestChange;
                    }
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);

                    if (++loopCounter > 3)
                    {
                        throw new TimeoutException("Failed to update schedule log");
                    }
                }
            }

            if(errorWritingFile)
            {
                throw new Exception("Error wrtiting file" + ErrorEvent.getName);
            }

            return await retValue.ConfigureAwait(false); ;
        }
        /// <summary>
        /// updates the logcontrol with a possible new location
        /// </summary>
        /// <param name="NewLocation"></param>
        public void updateNewLocation(TilerElements.Location NewLocation)
        {
            this.NewLocation = NewLocation;
        }

        public void UpdateCacheLocation(XmlDocument xmldoc, Dictionary<string, TilerElements.Location> currentCache, TilerElements.Location NewLocation )
        {
            XmlNode LocationCacheNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache");
            if (LocationCacheNode == null)
            {
                XmlNode ScheduleLogNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog");
                LocationCacheNode = CreateLocationCacheNode();

                XmlNode MyImportedNode = xmldoc.ImportNode(LocationCacheNode as XmlNode, true);
                xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog").AppendChild(MyImportedNode);
                LocationCacheNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache");
            }

            XmlNodeList AllCachedLocations = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/LocationCache/Locations/Location");


           
            foreach (KeyValuePair<string, TilerElements.Location> eachKeyValuePair in CachedLocation)
            {
                if (!currentCache.ContainsKey(eachKeyValuePair.Key))
                {
                    //if (!eachKeyValuePair.Value.isNull)
                    {
                        string LocationID = eachKeyValuePair.Value.ID;
                        XmlElement myCacheLocationNode = CreateLocationNode(eachKeyValuePair.Value, "Location");

                        XmlNode MyImportedNode = xmldoc.ImportNode(myCacheLocationNode as XmlNode, true);
                        myCacheLocationNode = MyImportedNode as XmlElement;


                        XmlNode LocationIDNode = xmldoc.CreateElement("LocationID");
                        XmlNode CacheNameNode = xmldoc.CreateElement("CachedName");
                        CacheNameNode.InnerText = eachKeyValuePair.Value.Description.ToLower();
                        LocationIDNode.InnerText = LocationID.ToString();
                        MyImportedNode.PrependChild(LocationIDNode);
                        MyImportedNode.PrependChild(CacheNameNode);
                        MyImportedNode = xmldoc.ImportNode(myCacheLocationNode as XmlNode, true);

                        if (!UpdateInnerXml(ref AllCachedLocations, "LocationID", LocationID.ToString(), myCacheLocationNode))
                        {
                            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache/Locations").AppendChild(MyImportedNode);
                        }
                    }
                }
                else
                {
                    if (NewLocation != null)
                    {
                        if (NewLocation.Description.ToLower() == eachKeyValuePair.Key)
                        {
                            updateLocationNode(NewLocation, xmldoc);
                        }
                    }
                }
            }
        }

        public bool isLocationIsDifferent(TilerElements.Location OldLocation, TilerElements.Location NewLocation)
        {
            double newDistance = TilerElements.Location.calculateDistance(OldLocation, NewLocation);
            bool retValue = newDistance >= 0.5;
            return retValue;
        }

        virtual protected XmlNode getLocationNodeByTagName(string TagName, XmlDocument DocNode = null)
        {
            TagName = TagName.Trim().ToLower();
            XmlNode retValue = null;
            XmlDocument doc = DocNode;
            if (DocNode == null)
            {
                doc = getLogDataStore();
            }
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache");
            if (node == null)
            {
                return retValue;
            }
            XmlNodeList AllLocationNodes = node.SelectNodes("Locations/Location");
            foreach (XmlNode eachXmlNode in AllLocationNodes)
            {
                string desciption = eachXmlNode.SelectSingleNode("Description").InnerText.ToLower(); 
                string CachedName = eachXmlNode.SelectSingleNode("CachedName").InnerText.ToLower();
                if ((desciption == TagName)|| (CachedName == TagName))
                {
                    retValue = eachXmlNode;
                    break;
                }
            }
            return retValue;
        }

        virtual public XmlNode updateLocationNode(TilerElements.Location Location, XmlDocument DocNode = null)
        {
            XmlNode OldNode = getLocationNodeByTagName(Location.Description, DocNode);
            TilerElements.Location OldLocation;
            if(OldNode != null)
            {
                OldLocation = getLocation(OldNode);
            }
            else
            {
                OldLocation = new TilerElements.Location();
            }
            

            XmlElement newNode = CreateLocationNode(Location);
            if(isLocationIsDifferent(OldLocation, Location))
            {
                OldNode.InnerXml = newNode.InnerXml;
                XmlNode LocationIDNode = DocNode.CreateElement("LocationID");
                XmlNode CacheNameNode = DocNode.CreateElement("CachedName");
                CacheNameNode.InnerText = Location.Description.ToLower();
                LocationIDNode.InnerText = Location.ID;
                OldNode.PrependChild(LocationIDNode);
                OldNode.PrependChild(CacheNameNode);
            }

            return OldNode;
        }

        public XmlElement generateNowProfileNode(NowProfile myNowProfile)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement NowProfileNode = xmldoc.CreateElement("NowProfile");
            XmlElement InitializedNode = xmldoc.CreateElement("Initialized");
            XmlElement PreferredStart = xmldoc.CreateElement("PreferredStart");
            PreferredStart.InnerText = myNowProfile.PreferredTime.ToString();
            InitializedNode.InnerText = myNowProfile.isInitialized.ToString();
            NowProfileNode.AppendChild(InitializedNode);
            NowProfileNode.AppendChild(PreferredStart);
            return NowProfileNode;
        }

        /// <summary>
        /// This function generates a tiler user node that simply stores the id and user name for a given tiler user
        /// </summary>
        /// <param name="user">tiler user</param>
        /// <returns></returns>
        public XmlElement generateTilerUserNode(TilerUser user)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement UserNode = xmlDoc.CreateElement("UserNode");
            XmlElement idNode = xmlDoc.CreateElement("Id");
            XmlElement userNameNode = xmlDoc.CreateElement("UserName");
            XmlElement CalendarType = xmlDoc.CreateElement("CalendarType");
            idNode.InnerText = user.Id;
            userNameNode.InnerText = user.UserName;
            CalendarType.InnerText = user.CalendarType;
            UserNode.AppendChild(idNode);
            UserNode.AppendChild(userNameNode);
            return UserNode;
        }

        public XmlElement generateTimeZone(string timeZone)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement TimeZoneNode = xmlDoc.CreateElement("TimeZone");
            TimeZoneNode.InnerText = timeZone;
            return TimeZoneNode;
        }

        public XmlElement generateTilerUserGroup(TilerUserGroup usergroup)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement userGroupNode = xmlDoc.CreateElement("UserGroup");
            XmlElement idNode = xmlDoc.CreateElement("Id");
            XmlElement usersNode = xmlDoc.CreateElement("Users");
            foreach(TilerUser user in usergroup.getUsers)
            {
                XmlElement userNode = generateTilerUserNode(user);
                usersNode.AppendChild(userNode);
            }

            return userGroupNode;
        }

        public XmlElement CreateLocationCacheNode()
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement LocationCacheNode = xmldoc.CreateElement("LocationCache");
            XmlElement LocationsNode = xmldoc.CreateElement("Locations");
            LocationCacheNode.PrependChild(LocationsNode);
            return LocationCacheNode;
        }



        public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent)
        {
            XmlDocument xmldoc = new XmlDocument();


            XmlElement MyEventScheduleNode = xmldoc.CreateElement("EventSchedule");
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Completed"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getIsComplete.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RepetitionFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.RepetitionStatus.ToString();

            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("EventSubSchedules"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Repetition.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RigidFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getRigid.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getActiveDuration.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Split"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.NumberOfSplit.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Deadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.End.UtcDateTime.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getPreparation.ToString();
            
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("CompletionCount"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.CompletionCount.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("DeletionCount"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.DeletionCount.ToString();

            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getPreDeadline.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Start.UtcDateTime.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getName.NameValue.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("NameId"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getName.NameId.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getId;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEnabled.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.Location, "EventScheduleLocation").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MyEvent.getUIParam, "UIParams").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MyEvent.Notes, "MiscData").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Restricted"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getIsEventRestricted.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("NowProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateNowProfileNode(MyEvent.getNowInfo).InnerXml);
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ProcrastinationProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateProcrastinationNode(MyEvent.getProcrastinationInfo).InnerXml);
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("isProcrastinateEvent"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getIsProcrastinateCalendarEvent.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("TimeZone"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getTimeZone;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("TimeCreaated"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.TimeCreated.ToString();

            if (MyEvent.getIsEventRestricted)
            {
                CalendarEventRestricted restrictedMyEvent = (CalendarEventRestricted)MyEvent;
                XmlElement restrictionProfileData = generateXMLRestrictionProfile(restrictedMyEvent.RetrictionInfo);
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RestrictionProfile"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = restrictionProfileData.InnerXml;
            }


            if (MyEvent.RepetitionStatus)
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Recurrence"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = CreateRepetitionNode(MyEvent.Repeat).InnerXml;
                return MyEventScheduleNode;
            }
            else
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Recurrence"));

            }
            XmlNode SubScheduleNodes = MyEventScheduleNode.SelectSingleNode("EventSubSchedules");
            foreach (SubCalendarEvent MySubEvent in MyEvent.AllSubEvents)
            {
                SubScheduleNodes.PrependChild(xmldoc.CreateElement("EventSubSchedule"));
                SubScheduleNodes.ChildNodes[0].InnerXml = CreateSubScheduleNode(MySubEvent).InnerXml;
            }


            return MyEventScheduleNode;
        }

        public XmlElement CreateRepetitionNode(Repetition RepetitionObjEntry)//This takes a repetition object, and creates a Repetition XmlNode
        {
            int Layer = 0;

            List<XmlNode> lowerLayers = new List<XmlNode>();
            XmlDocument xmldoc = new XmlDocument();
            if (RepetitionObjEntry.isExtraLayers())
            {
                foreach (Repetition eachRepetition in RepetitionObjEntry.getDayRepetitions())
                {
                    XmlElement LayerData = CreateRepetitionNode(eachRepetition);
                    XmlNode MyImportedNode = xmldoc.ImportNode(LayerData as XmlNode, true);
                    lowerLayers.Add(MyImportedNode);
                }
            }


            int i = 0;

            XmlElement RepeatCalendarEventsNode = xmldoc.CreateElement("Recurrence");//Defines umbrella Repeat XmlNode 
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatStartDate"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Range.Start.UtcDateTime.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatEndDate"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Range.End.UtcDateTime.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatFrequency"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.getFrequency.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatDay"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.weekDay.ToString();
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatDays"));

            foreach (XmlNode eachXmlNode in lowerLayers)
            {
                RepeatCalendarEventsNode.ChildNodes[0].PrependChild(eachXmlNode);
            }
            RepeatCalendarEventsNode.PrependChild(xmldoc.CreateElement("RepeatCountOfLowerLayer"));
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = Layer.ToString();
            XmlNode XmlNodeForRepeatListOfEvents = xmldoc.CreateElement("RepeatCalendarEvents");


            XmlElement RepeatCalendarEventNode;//Declares Repeat XmlNode 
            CalendarEvent[] allRecurringEVents = RepetitionObjEntry.RecurringCalendarEvents();
            for (; i < allRecurringEVents.Length; i++)//For loop goes through each classEvent in repeat object and generates an xmlnode
            {
                RepeatCalendarEventNode = xmldoc.CreateElement("RepeatCalendarEvent");
                RepeatCalendarEventNode.InnerXml = CreateEventScheduleNode(allRecurringEVents[i]).InnerXml;
                XmlNodeForRepeatListOfEvents.PrependChild(RepeatCalendarEventNode);
            }
            RepeatCalendarEventsNode.PrependChild(XmlNodeForRepeatListOfEvents);
            return RepeatCalendarEventsNode;
        }

        public XmlElement generateProcrastinationNode(Procrastination ProcrastinationData)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement ProcrastinationProfileNode = xmldoc.CreateElement("ProcrastinationProfile");
            XmlElement ProcrastinationPreferredStart = xmldoc.CreateElement("ProcrastinationPreferredStart");
            ProcrastinationPreferredStart.InnerText = ProcrastinationData.PreferredStartTime.ToString();
            XmlElement ProcrastinationDislikedStartNode = xmldoc.CreateElement("ProcrastinationDislikedStart");
            ProcrastinationDislikedStartNode.InnerText = ProcrastinationData.DislikedStartTime.ToString();
            XmlElement ProcrastinationDislikedDaySectionNode = xmldoc.CreateElement("ProcrastinationDislikedDaySection");
            ProcrastinationDislikedDaySectionNode.InnerText = ProcrastinationData.DislikedDaySection.ToString();
            ProcrastinationProfileNode.AppendChild(ProcrastinationPreferredStart);
            ProcrastinationProfileNode.AppendChild(ProcrastinationDislikedStartNode);
            ProcrastinationProfileNode.AppendChild(ProcrastinationDislikedDaySectionNode);
            return ProcrastinationProfileNode;
        }

        public XmlElement generateXMLRestrictionProfile(RestrictionProfile RestrictionProfileData)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement RestrictionProfileNode = xmldoc.CreateElement("RestrictionProfile");
            List<RestrictionDay> ActiveRestrictions = RestrictionProfileData.getActiveDays();
            foreach (RestrictionDay eachTuple in ActiveRestrictions)
            {
                XmlElement RestrictionNode = xmldoc.CreateElement("RestrictionNode");
                XmlElement RestrictionDayOfWeekNode = xmldoc.CreateElement("RestrictionDayOfWeek");
                RestrictionDayOfWeekNode.InnerText = ((int)eachTuple.DayOfWeek).ToString();
                XmlElement RestrictionTimeLineNode = xmldoc.CreateElement("RestrictionTimeLineData");
                RestrictionTimeLineNode.InnerXml=generateRestrictionTimeLineNode(eachTuple.RestrictionTimeLine).InnerXml;
                RestrictionNode.AppendChild(RestrictionTimeLineNode);
                RestrictionNode.AppendChild(RestrictionDayOfWeekNode);
                RestrictionProfileNode.AppendChild(RestrictionNode);
            }
            return RestrictionProfileNode;
        }

        public XmlElement generateRestrictionTimeLineNode(RestrictionTimeLine RestrictionTimeLineData)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement RestrictionTimeLineNode = xmldoc.CreateElement("RestrictionTimeLineData");
            RestrictionTimeLineNode.PrependChild(xmldoc.CreateElement("StartTime"));
            RestrictionTimeLineNode.ChildNodes[0].InnerText = RestrictionTimeLineData.Start.UtcDateTime.ToString();
            RestrictionTimeLineNode.PrependChild(xmldoc.CreateElement("EndTime"));
            RestrictionTimeLineNode.ChildNodes[0].InnerText = RestrictionTimeLineData.End.UtcDateTime.ToString();
            RestrictionTimeLineNode.PrependChild(xmldoc.CreateElement("RangeSpan"));
            RestrictionTimeLineNode.ChildNodes[0].InnerText = RestrictionTimeLineData.Span.Ticks.ToString();
            return RestrictionTimeLineNode;
        }


        public XmlElement CreateSubScheduleNode(SubCalendarEvent MySubEvent)
        {

            SubCalendarEventRestricted restrictedMySub;
            XmlDocument xmldoc = new XmlDocument();
            


            
            
            XmlElement MyEventSubScheduleNode = xmldoc.CreateElement("EventSubSchedule");
            DateTimeOffset StartTime = MySubEvent.Start;
            StartTime = StartTime.removeSecondsAndMilliseconds();
            DateTimeOffset EndTime = MySubEvent.End;
            EndTime = EndTime.removeSecondsAndMilliseconds();
            TimeSpan EventTimeSpan = MySubEvent.getActiveDuration;

            if ((!string.IsNullOrEmpty(MySubEvent.Location.Description)) || (!MySubEvent.Location.isNull))
            {
                string TaggedLocation = MySubEvent.Location.Description;
                TaggedLocation = TaggedLocation.ToLower();
                if (!CachedLocation.ContainsKey(TaggedLocation))
                {
                    CachedLocation.Add(TaggedLocation, MySubEvent.Location);
                }

            }





            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("EndTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = EndTime.UtcDateTime.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = StartTime.UtcDateTime.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = EventTimeSpan.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveEndTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = EndTime.UtcDateTime.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ActiveStartTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = StartTime.UtcDateTime.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getPreparation.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ThirdPartyID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ThirdPartyID;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Rigid"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getRigid.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getId;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isEnabled.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Complete"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getIsComplete.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getName.NameValue.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("NameId"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getName.NameId.ToString();

            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.Location, "EventSubScheduleLocation").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MySubEvent.getUIParam, "UIParams").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MySubEvent.Notes, "MiscData").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ConflictProfile"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateConflictProfile(MySubEvent.Conflicts, "ConflictProfile").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TimePositionReasons"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = ReasonForPosition(MySubEvent.ReasonsForPosiition.SelectMany(obj => obj.Value).ToList(), "TimePositionReasons").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Restricted"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getIsEventRestricted.ToString();
            MyEventSubScheduleNode.PrependChild(CreatePauseUsedUpNode(MySubEvent, xmldoc));
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isProcrastinateEvent"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getIsProcrastinateCalendarEvent.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TimeZone"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getTimeZone;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TimeCreaated"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TimeCreated.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TravelTimeAfter"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TravelTimeAfter.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TravelTimeBefore"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TravelTimeBefore.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isWake"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isWake.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isSleep"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isSleep.ToString();

            if (MySubEvent.getIsEventRestricted)
            {
                restrictedMySub = (SubCalendarEventRestricted)MySubEvent;
                XmlElement restrictionProfileData =  generateXMLRestrictionProfile(restrictedMySub.RetrictionInfo);
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("RestrictionProfile"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = restrictionProfileData.InnerXml;
            }

            return MyEventSubScheduleNode;
        }

        /// <summary>
        /// Class generates the pause xml mode for a given schedule
        /// </summary>
        /// <param name="SubEvent"></param>
        /// <returns></returns>
        public XmlElement CreatePauseUsedUpNode(SubCalendarEvent SubEvent, XmlDocument xmldoc)
        {


            XmlElement UsedUpTime = xmldoc.CreateElement("UsedUpTime");
            XmlElement PauseTime = xmldoc.CreateElement("PauseTime");
            XmlElement RetValue = xmldoc.CreateElement("PauseInformation");
            UsedUpTime.InnerText = SubEvent.UsedTime.ToString();
            PauseTime.InnerText = SubEvent.getPauseTime().ToString();
            RetValue.AppendChild(UsedUpTime);
            RetValue.AppendChild(PauseTime);

            return RetValue;
        }
        public XmlElement ReasonForPosition(List<Reason> reasons, string ElementIdentifier= "TimePositionReasons") {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Reason>));
            XmlDocument xmldoc = new XmlDocument();
            XmlElement retValue = xmldoc.CreateElement(ElementIdentifier);
            using (XmlWriter writer = xmldoc.CreateNavigator().AppendChild())
            {
                serializer.Serialize(writer, reasons);
            }
            retValue.InnerXml = xmldoc.FirstChild.InnerXml;
            return retValue;
        }

        public XmlElement CreateLocationNode(TilerElements.Location Arg1, string Identifier = "Location")
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(Identifier);
            string XCoordinate = "";
            string YCoordinate = "";
            string Descripion = "";
            string MappedAddress = "";
            string IsNull = true.ToString(); ;
            string CheckCalendarEvent = 0.ToString();
            if ((Arg1 != null) )//&& (!Arg1.isNull))
            {
                XCoordinate = Arg1.XCoordinate.ToString();
                YCoordinate = Arg1.YCoordinate.ToString();
                Descripion = Arg1.Description;
                MappedAddress = Arg1.Address;
                IsNull = Arg1.isNull.ToString();
                CheckCalendarEvent = Arg1.isDefault.ToString();
            }
            var1.PrependChild(xmldoc.CreateElement("XCoordinate"));
            var1.ChildNodes[0].InnerText = XCoordinate;
            var1.PrependChild(xmldoc.CreateElement("YCoordinate"));
            var1.ChildNodes[0].InnerText = YCoordinate;
            var1.PrependChild(xmldoc.CreateElement("Address"));
            var1.ChildNodes[0].InnerText = MappedAddress;
            var1.PrependChild(xmldoc.CreateElement("Description"));
            var1.ChildNodes[0].InnerText = Descripion;
            var1.PrependChild(xmldoc.CreateElement("isNull"));
            var1.ChildNodes[0].InnerText = IsNull;
            var1.PrependChild(xmldoc.CreateElement("CheckCalendarEvent"));
            var1.ChildNodes[0].InnerText = CheckCalendarEvent;
            return var1;
        }


        public XmlElement CreateConflictProfile(ConflictProfile Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);

            int typeOfConflict = Arg1.conflictType;
            bool conflictFlag = Arg1.isConflicting();

            var1.PrependChild(xmldoc.CreateElement("Type"));
            var1.ChildNodes[0].InnerText = typeOfConflict.ToString();
            var1.PrependChild(xmldoc.CreateElement("Flag"));
            var1.ChildNodes[0].InnerText = conflictFlag.ToString();
            var1.PrependChild(xmldoc.CreateElement("ConflictIDs"));
            var1.ChildNodes[0].InnerXml = CreateConflictingIDS(Arg1.getConflictingEventIDs(), "ConflictIDs").InnerXml;

            return var1;
        }



        public XmlElement CreateConflictingIDS(IEnumerable<string> Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            foreach (string eachXmlNode in Arg1)
            {
                var1.PrependChild(xmldoc.CreateElement("ConflictID"));
                var1.ChildNodes[0].InnerText = eachXmlNode;
            }
            return var1;
        }



        public bool UpdateInnerXml(ref XmlNodeList MyLogList, string NodeName, string IdentifierData, XmlElement UpdatedData)
        {
            for (int i = 0; i < MyLogList.Count; i++)
            {
                //XmlNode XmlTempHolder = MyLogList[i];
                //string TempHolder = XmlTempHolder.SelectSingleNode("ID").InnerText;
                if (MyLogList[i].SelectSingleNode(NodeName).InnerText == IdentifierData)
                {
                    MyLogList[i].InnerXml = UpdatedData.InnerXml;
                    return true;
                }
            }
            return false;
        }

        public bool UpdateXMLInnerText(ref XmlNodeList MyLogList, string NodeName, string IdentifierData, string UpdatedData)
        {
            foreach (XmlNode MyNode in MyLogList)
            {
                if (MyNode.SelectSingleNode("/" + NodeName).InnerText == IdentifierData)
                {
                    MyNode.SelectSingleNode("/" + NodeName).InnerText = UpdatedData;

                    return true;
                }
            }
            return false;
        }

        public XmlElement createDisplayUINode(EventDisplay Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            var1.PrependChild(xmldoc.CreateElement("Visible"));
            var1.ChildNodes[0].InnerText = Arg1.isVisible.ToString();
            var1.PrependChild(xmldoc.CreateElement("Color"));
            var1.ChildNodes[0].InnerXml = createColorNode(Arg1.UIColor, "Color").InnerXml;
            var1.PrependChild(xmldoc.CreateElement("Type"));
            var1.ChildNodes[0].InnerText = Arg1.isDefault.ToString();
            return var1;
        }

        public XmlElement createColorNode(TilerColor Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            var1.PrependChild(xmldoc.CreateElement("Opacity"));
            var1.ChildNodes[0].InnerText = Arg1.O.ToString();
            var1.PrependChild(xmldoc.CreateElement("Red"));
            var1.ChildNodes[0].InnerText = Arg1.R.ToString();
            var1.PrependChild(xmldoc.CreateElement("Green"));
            var1.ChildNodes[0].InnerText = Arg1.G.ToString();
            var1.PrependChild(xmldoc.CreateElement("Blue"));
            var1.ChildNodes[0].InnerText = Arg1.B.ToString();
            var1.PrependChild(xmldoc.CreateElement("UserSelection"));
            var1.ChildNodes[0].InnerText = Arg1.User.ToString();

            return var1;
        }

        public XmlElement createMiscDataNode(MiscData Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            var1.PrependChild(xmldoc.CreateElement("UserNote"));
            var1.ChildNodes[0].InnerText = Arg1.UserNote.ToString();
            var1.PrependChild(xmldoc.CreateElement("TypeSelection"));
            var1.ChildNodes[0].InnerText = Arg1.TypeSelection.ToString();
            return var1;
        }
        public void EmptyCalendarXMLFile(string dirString = "")
        {
            if (string.IsNullOrEmpty(dirString))
            {
                dirString = WagTapLogLocation + CurrentLog;
            }

            File.WriteAllText(dirString, "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>1024</LastIDCounter><referenceDay>8:00 AM</referenceDay><EventSchedules></EventSchedules></ScheduleLog>");
        }

        public void deleteAllCalendarEvets(string dirString = "")
        {
#if ForceReadFromXml
#else
            if(useCassandra)
            {
                return;
            }
#endif
            if (string.IsNullOrEmpty(dirString))
            {
                dirString = WagTapLogLocation + CurrentLog;
            }

            XmlDocument doc = new XmlDocument();
            int loopCounter = 0;
            while (true)
            {
                if (!File.Exists(dirString))
                {
                    break;
                }
                try
                {
                    doc.Load(dirString);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);

                    if (++loopCounter > 3)
                    {
                        throw new TimeoutException("Failed to create empty log for deletion");
                    }
                }
            }

            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            EventSchedulesNodes.InnerText = "";

            loopCounter = 0;
            while (true)
            {
                try
                {
                    doc.Save(dirString);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);

                    if (++loopCounter > 3)
                    {
                        throw new TimeoutException("Failed to save empty log in deletion of log");
                    }
                }
            }

        }
        #endregion

        #region Read Data
        
        /// <summary>
        /// Function retrieves a db_tiler user from an xmlnode.  Note if there is no user node it returns an object in which the isNull property is set to true
        /// </summary>
        /// <param name="userNode">The xml node with the tiler user information</param>
        /// <returns></returns>
        protected DB_TilerUser getTilerLoggedUser(XmlNode userNode)
        {
            DB_TilerUser retValue = new DB_TilerUser();
            if(userNode!= null)
            {
                XmlNode idNode = userNode.SelectSingleNode("Id");
                string id = idNode.InnerText;
                XmlNode userNameNode = userNode.SelectSingleNode("UserName");
                XmlNode CalendarType = userNode.SelectSingleNode("CalendarType");
                string userName = userNameNode.InnerText;
                retValue.Id = id;
                retValue.UserName = UserName;
                retValue.CalendarType = CalendarType.InnerText;
                retValue.isNull = false;
            }
            return retValue;
        }
        /// <summary>
        /// Function retrieves a tiler user group form an xmlnode. Npte if there isn't a node a new object will be created with the isNull flag set
        /// </summary>
        /// <param name="userGroupNode">xml node with formated user group</param>
        /// <returns></returns>
        public DB_TilerUserGroup getTilerUserGroup(XmlNode userGroupNode)
        {
            DB_TilerUserGroup retValue = new DB_TilerUserGroup();
            if (userGroupNode != null)
            {
                XmlNode idNode = userGroupNode.SelectSingleNode("Id");
                string id = idNode.InnerText;
                List<TilerUser> users = new List<TilerUser>();
                XmlNode userNodes = userGroupNode.SelectSingleNode("Users");
                if (userNodes.ChildNodes != null)
                {
                    foreach (XmlNode userNode in userNodes.ChildNodes)
                    {
                        TilerUser user = getTilerLoggedUser(userNode);
                        users.Add(user);
                    }

                }
                retValue.Id = id;
                retValue.Users = users;
                retValue.isNull = false;
            }
            return retValue;
        }



        public string GetShortcutTarget(string file)
        {
            try
            {
                if (System.IO.Path.GetExtension(file).ToLower() != ".lnk")
                {
                    throw new Exception("Supplied file must be a .LNK file");
                }

                FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read);
                using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                {
                    fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                    uint flags = fileReader.ReadUInt32();        // Read flags
                    if ((flags & 1) == 1)
                    {                      // Bit 1 set means we have to
                        // skip the shell item ID list
                        fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                        uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                        fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                    }

                    long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                    // structure begins
                    uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                    fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                    uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                    // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                    fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                    // base pathname (target)
                    long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                    // the base pathname. I don't need the 2 terminating nulls.
                    char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                    var link = new string(linkTarget);

                    int begin = link.IndexOf("\0\0");
                    if (begin > -1)
                    {
                        int end = link.IndexOf("\\\\", begin + 2) + 2;
                        end = link.IndexOf('\0', end) + 1;

                        string firstPart = link.Substring(0, begin);
                        string secondPart = link.Substring(end);

                        return firstPart + secondPart;
                    }
                    else
                    {
                        return link;
                    }
                }
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Function gets the end of the day of the specified user.
        /// </summary>
        /// <param name="NameOfFile"></param>
        /// <returns></returns>
        virtual public async Task<DateTimeOffset> getDayReferenceTime(string NameOfFile = "")
        {
            XmlDocument doc = getLogDataStore(NameOfFile);
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/referenceDay");
            DateTimeOffset retValue = _TilerUser.EndfOfDay;

            return retValue;
        }

        protected XmlDocument getLogDataStore(string NameOfFile = "")
        {

            XmlDocument doc = new XmlDocument();
            if (string.IsNullOrEmpty(NameOfFile))
            {
                //NameOfFile = "MyEventLog.xml";
                NameOfFile = WagTapLogLocation + CurrentLog;
            }
#if readfromBeforeInsertionFixingStiticRestricted
                NameOfFile = WagTapLogLocation + "BeforeInsertionFixingStiticRestricted.xml.lnk";
                NameOfFile = GetShortcutTarget(NameOfFile);
#endif
            int loopCounter = 0;
            while (true)
            {
                if (!File.Exists(NameOfFile))
                {
                    break;
                }
                try
                {
                    doc.Load(NameOfFile);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);

                    if (++loopCounter > 3)
                    {
                        throw new TimeoutException("Failed to load day reference");
                    }
                }
            }

            return doc;
        }

        async protected Task<Dictionary<string, TilerElements.Location>> getLocationCache(string NameOfFile = "")
        {
            Dictionary<string, TilerElements.Location> retValue = new Dictionary<string, TilerElements.Location>();
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                Task<Tuple<bool, string, DateTimeOffset, long>> gettingLatesData = LogDBDataAccess.getLatestChanges(ID);
                Task<Dictionary<string, Location_Elements>> gettingLocationCache = myCassandraAccess.getAllCachedLocations();
                Tuple<bool, string, DateTimeOffset, long> LatesData =await gettingLatesData;
                LastIDNumber = LatesData.Item4;
                retValue = await gettingLocationCache;
                return retValue;
            }
#endif


            XmlDocument doc = getLogDataStore(NameOfFile);
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache");
            if (node == null)
            {
                return retValue;
            }
            XmlNodeList AllLocationNodes = node.SelectNodes("Locations/Location");
            foreach (XmlNode eachXmlNode in AllLocationNodes)
            {
                TilerElements.Location myLocation = generateLocationObjectFromNode(eachXmlNode);

                if (myLocation != null)
                {
                    if (!string.IsNullOrEmpty(myLocation.Description))
                    {
                        string Description = myLocation.Description.ToLower();
                        if (!retValue.ContainsKey(Description))
                        {
                            retValue.Add(Description, myLocation);
                        }
                    }
                }
            }
            return retValue;
        }



        protected virtual Dictionary<string, CalendarEvent> getAllCalendarFromXml(TimeLine RangeOfLookUP, XmlNode IdNode, XmlNode EventSchedulesNodes)
        {
            Dictionary<string, CalendarEvent> MyCalendarEventDictionary = new Dictionary<string, CalendarEvent>();
            if (IdNode != null)
            {
                string LastUsedIndex = IdNode.InnerText;
                LastIDNumber = Convert.ToInt64(LastUsedIndex);
            }
            
            DateTimeOffset userReferenceDay;
            string ID;
            string Deadline;
            string Split;
            string Completed;
            string Rigid;
            string Name;
            string[] StartDateTime;
            string StartDate;
            string StartTime;
            string[] EndDateTime;
            string EndDate;
            string EndTime;
            string PreDeadline;
            string CalendarEventDuration;
            string PreDeadlineFlag;
            string EventRepetitionflag;
            string PrepTimeFlag;
            string PrepTime;

            if (EventSchedulesNodes.ChildNodes != null)
            {
                foreach (XmlNode EventScheduleNode in EventSchedulesNodes.ChildNodes)
                {
                    CalendarEvent RetrievedEvent;

                    //RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode, RangeOfLookUP);
                    ///*
                    RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode, RangeOfLookUP);
                    //*/

                    if (RetrievedEvent != null)
                    { MyCalendarEventDictionary.Add(RetrievedEvent.Calendar_EventID.getCalendarEventComponent(), RetrievedEvent); }
                }

            }


            return MyCalendarEventDictionary;
        }

        public virtual Dictionary<string, CalendarEvent> getAllCalendarFromXml(TimeLine RangeOfLookUP)
        {
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                return myCassandraAccess.getAllCalendarEvent();
            }
#endif

            XmlDocument doc = getLogDataStore();
            XmlNode IdNode = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");
            
            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");

            return getAllCalendarFromXml(RangeOfLookUP, IdNode, EventSchedulesNodes);



        }
        public virtual CalendarEvent getCalendarEventObjFromNode(XmlNode EventScheduleNode, TimeLine RangeOfLookUP)
        {
            string ID;
            string Deadline;
            int Split;
            string Completed;
            string Rigid;
            EventName Name;
            string[] StartDateTime;
            string StartDate;
            string StartTime;
            string[] EndDateTime;
            string EndDate;
            string EndTime;
            TimeSpan PreDeadline;
            TimeSpan CalendarEventDuration;
            string PreDeadlineFlag;
            string EventRepetitionflag;
            string PrepTimeFlag;
            TimeSpan PrepTime;
            string RepeatStart;
            string RepeatEnd;
            string RepeatFrequency;
            string LocationData;
            string EnableFlag;


            string NameId = EventScheduleNode.SelectSingleNode("NameId")?.InnerText ?? Guid.NewGuid().ToString();
            Name = new DB_EventName( EventScheduleNode.SelectSingleNode("Name").InnerText, NameId);
            ID = EventScheduleNode.SelectSingleNode("ID").InnerText;
            //EventScheduleNode.SelectSingleNode("ID").InnerXml = "<wetin></wetin>";
            Deadline = EventScheduleNode.SelectSingleNode("Deadline").InnerText;
            Rigid = EventScheduleNode.SelectSingleNode("RigidFlag").InnerText;
            XmlNode RecurrenceXmlNode = EventScheduleNode.SelectSingleNode("Recurrence");
            EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;


            DateTimeOffset StartDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("StartTime").InnerText).UtcDateTime;
            DateTimeOffset EndDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("Deadline").InnerText).UtcDateTime;
            DateTimeOffset start = DateTimeOffset.Parse( EventScheduleNode.SelectSingleNode("StartTime").InnerText);
            StartDateTime = EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');

            StartDate = StartDateTime[0];
            StartTime = StartDateTime[1] + StartDateTime[2];
            EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
            DateTimeOffset end= DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("Deadline").InnerText);
            EndDate = EndDateTime[0];
            EndTime = EndDateTime[1] + EndDateTime[2];
            DateTimeOffset StartTimeConverted = DateTimeOffset.Parse(StartDate).UtcDateTime;// new DateTimeOffset(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
            DateTimeOffset EndTimeConverted = DateTimeOffset.Parse(EndDate).UtcDateTime; //new DateTimeOffset(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));

            Repetition Recurrence;
            if (Convert.ToBoolean(EventRepetitionflag))
            {
                Recurrence = getRepetitionObject(RecurrenceXmlNode, RangeOfLookUP); ;

                StartTimeConverted = (Recurrence.Range.Start);
                EndTimeConverted = (Recurrence.Range.End);
            }
            else
            {
                Recurrence = new Repetition();
            }
            Split = Convert.ToInt32( EventScheduleNode.SelectSingleNode("Split").InnerText);
            PreDeadline = TimeSpan.Parse( EventScheduleNode.SelectSingleNode("PreDeadline").InnerText);
            CalendarEventDuration = TimeSpan.Parse( EventScheduleNode.SelectSingleNode("Duration").InnerText);
            PrepTime = TimeSpan.Parse(EventScheduleNode.SelectSingleNode("PrepTime").InnerText);
            Completed = EventScheduleNode.SelectSingleNode("Completed").InnerText;
            EnableFlag = EventScheduleNode.SelectSingleNode("Enabled").InnerText;
            bool EVentEnableFlag = Convert.ToBoolean(EnableFlag);
            bool completedFlag = Convert.ToBoolean(Completed);

            XmlNode completeNode = EventScheduleNode.SelectSingleNode("CompletionCount");
            XmlNode deleteNode = EventScheduleNode.SelectSingleNode("DeletionCount");
            int CompleteCount = 0;
            int DeleteCount = 0;

            //string Name, string StartTime, DateTimeOffset StartDate, string EndTime, DateTimeOffset EventEndDate, string eventSplit, string PreDeadlineTime, string EventDuration, bool EventRepetitionflag, bool DefaultPrepTimeflag, bool RigidScheduleFlag, string eventPrepTime, bool PreDeadlineFlag

            //MainWindow.CreateSchedule("","",new DateTimeOffset(),"",new DateTimeOffset(),"","","",true,true,true,"",false);

            TilerElements.Location var3 = getLocation(EventScheduleNode);
            MiscData noteData = getMiscData(EventScheduleNode);
            EventDisplay UiData = getDisplayUINode(EventScheduleNode);
            Procrastination procrastinationData = generateProcrastinationObject(EventScheduleNode);
            NowProfile NowProfileData = generateNowProfile(EventScheduleNode);
            bool procrastinationEventFlag = Convert.ToBoolean(EventScheduleNode.SelectSingleNode("isProcrastinateEvent")?.InnerText?? "False");
            bool rigidFlag = Convert.ToBoolean(Rigid);
            string timeZone = EventScheduleNode.SelectSingleNode("TimeZone")?.InnerText ?? "UTC";
            TilerUser creator = getTilerLoggedUser(EventScheduleNode.SelectSingleNode("UserNode"));
            if ((creator as DB_TilerUser).isNull)
            {
                creator = new DB_TilerUser()
                {
                    Id = this.LoggedUserID,
                    UserName = this.UserName,
                    isNull = false
                };
                if(creator.Id == this._TilerUser.Id)
                {
                    creator = this._TilerUser;
                }
            }
            
            TilerUserGroup userGroup = getTilerUserGroup(EventScheduleNode.SelectSingleNode("UserGroup"));
            CalendarEvent RetrievedEvent = null;
            if (rigidFlag)
            {
                if (!procrastinationEventFlag)
                {
                    RetrievedEvent = new RigidCalendarEvent(//new EventID(ID), 
                    Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Recurrence, var3, UiData, noteData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, new EventID(ID));
                }
                
                    


            } else
            {
                RetrievedEvent = new CalendarEvent(Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Split, Recurrence, var3, UiData, noteData, procrastinationData, NowProfileData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, new EventID(ID));
            }

            if (rigidFlag && procrastinationEventFlag)
            {
                RetrievedEvent = new DB_ProcrastinateCalendarEvent(new EventID(creator.getClearAllEventsId()), Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Recurrence, var3, UiData, noteData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, Split);
            }
            else {
                RetrievedEvent = new DB_CalendarEventExtra(RetrievedEvent, procrastinationData, NowProfileData);
            }

            SubCalendarEvent[] AllSubCalEvents = ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent, RangeOfLookUP).ToArray();
            XmlNode restrictedNode = EventScheduleNode.SelectSingleNode("Restricted");


            /*if (AllSubCalEvents.Length < 1)
            {
                return null;
            }*/

            if (rigidFlag && procrastinationEventFlag)
            {
                RetrievedEvent = new DB_ProcrastinateCalendarEvent(RetrievedEvent as DB_ProcrastinateCalendarEvent, AllSubCalEvents);
            }
            else
            {
                RetrievedEvent = new CalendarEvent(RetrievedEvent, AllSubCalEvents);
            }
                
            
            
            if (restrictedNode != null)
            {
                if (Convert.ToBoolean(restrictedNode.InnerText))
                {
                    XmlNode RestrictionProfileNode = EventScheduleNode.SelectSingleNode("RestrictionProfile");
                    DB_RestrictionProfile myRestrictionProfile = (DB_RestrictionProfile)getRestrictionProfile(RestrictionProfileNode);
                    RetrievedEvent = new DB_CalendarEventRestricted(RetrievedEvent,myRestrictionProfile);
                }
            }

            if ((completeNode != null))
            {
                CompleteCount = Convert.ToInt32(completeNode.InnerText);
            }
            else
            {
                CompleteCount = RetrievedEvent.AllSubEvents.Where(obj => obj.getIsComplete).Count();
            }


            if ((deleteNode != null))
            {
                DeleteCount = Convert.ToInt32(deleteNode.InnerText);
            }
            else
            {
                DeleteCount = RetrievedEvent.AllSubEvents.Where(obj => !obj.isEnabled).Count();
            }

            DateTimeOffset timeCreated;
            XmlNode timeCreatednode = EventScheduleNode.SelectSingleNode("TimeCreated");
            if (timeCreatednode != null)
            {
                timeCreated = DateTimeOffset.Parse(timeCreatednode.ToString());
                RetrievedEvent.TimeCreated = timeCreated;
            }


            RetrievedEvent.InitializeCounts(DeleteCount, CompleteCount);

            return RetrievedEvent;
        }
        
        public WeatherReason getWeatherReason(XmlNode ReasonNode)
        {
            //DBWeatherReason retValue = new DBWeatherReason();
            MemoryStream stm = new MemoryStream();

            StreamWriter stw = new StreamWriter(stm);
            stw.Write(ReasonNode.OuterXml);
            stw.Flush();

            stm.Position = 0;
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = ReasonNode.Name;
            xRoot.IsNullable = true;
            XmlSerializer ser = new XmlSerializer(typeof(WeatherReason), xRoot);
            WeatherReason result = (ser.Deserialize(stm) as WeatherReason);

            return result;
        }

        public BestFitReason getBestFitReason(XmlNode ReasonNode)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(ReasonNode.OuterXml);
            stw.Flush();

            stm.Position = 0;
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = ReasonNode.Name;
            xRoot.IsNullable = true;
            XmlSerializer ser = new XmlSerializer(typeof(BestFitReason), xRoot);
            BestFitReason result = (ser.Deserialize(stm) as BestFitReason);

            return result;
        }

        public PreservedOrder getPreservedOrderReason(XmlNode ReasonNode)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(ReasonNode.OuterXml);
            stw.Flush();

            stm.Position = 0;
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = ReasonNode.Name;
            xRoot.IsNullable = true;
            XmlSerializer ser = new XmlSerializer(typeof(PreservedOrder), xRoot);
            PreservedOrder result = (ser.Deserialize(stm) as PreservedOrder);

            return result;
        }


        public Reason getLocationReason(XmlNode ReasonNode)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(ReasonNode.OuterXml);
            stw.Flush();
            LocationReason result = new LocationReason();
            return result;
        }

        public Reason getOccupancyReason(XmlNode ReasonNode)
        {
            DurationReason result = new DurationReason(new TimeSpan());
            return result;
        }


        public Reason getRestrictedEventReason(XmlNode ReasonNode)
        {
            RestrictedEventReason result = new RestrictedEventReason(new RestrictionProfile(new DateTimeOffset(), new TimeSpan()));
            return result;
        }
        


        Procrastination generateProcrastinationObject(XmlNode ReferenceNode)
        {
            XmlNode ProcrastinationProfileNode = ReferenceNode.SelectSingleNode("ProcrastinationProfile");
            if(ProcrastinationProfileNode ==null)
            {
                Procrastination retValueEmpty= new Procrastination(new DateTimeOffset(), new TimeSpan());
                return retValueEmpty;
            }

            DateTimeOffset ProcrastinationPreferredStart = DateTimeOffset.Parse(ProcrastinationProfileNode.SelectSingleNode("ProcrastinationPreferredStart").InnerText).UtcDateTime; ;
            DateTimeOffset ProcrastinationDislikedStart = DateTimeOffset.Parse(ProcrastinationProfileNode.SelectSingleNode("ProcrastinationDislikedStart").InnerText).UtcDateTime;
            int DaySection = Convert.ToInt32(ProcrastinationProfileNode.SelectSingleNode("ProcrastinationDislikedDaySection").InnerText);
            DB_Procrastination retValue = new DB_Procrastination(ProcrastinationDislikedStart, ProcrastinationPreferredStart, DaySection);
            return retValue;
        }
        
        protected virtual List<SubCalendarEvent> ReadSubSchedulesFromXMLNode(XmlNode MyXmlNode, CalendarEvent MyParent, TimeLine RangeOfLookUP)
        {
            List<SubCalendarEvent> MyArrayOfNodes = new List<SubCalendarEvent>();
            string ID = "";
            DateTimeOffset Start = new DateTimeOffset();
            DateTimeOffset End = new DateTimeOffset();
            TimeSpan SubScheduleDuration = new TimeSpan();
            TimeSpan PrepTime = new TimeSpan();
            BusyTimeLine BusySlot = new BusyTimeLine();
            bool Enabled;
            for (int i = 0; i < MyXmlNode.ChildNodes.Count; i++)
            {

                XmlNode SubEventNode = MyXmlNode.ChildNodes[i];
                BusyTimeLine SubEventActivePeriod = new BusyTimeLine(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText, stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText), stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText));
                ID = EventID.convertToSubcalendarEventID(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText).ToString();
                Start = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText).UtcDateTime;
                End = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText).UtcDateTime;

                bool rigidFlag =MyParent.getRigid;
                XmlNode rigidNode = MyXmlNode.ChildNodes[i].SelectSingleNode("Rigid");
                if (rigidNode!=null)
                {
                    rigidFlag=Convert.ToBoolean(rigidNode.InnerText);
                }

                if (RangeOfLookUP.InterferringTimeLine(new TimeLine(Start, End)) == null)
                {
                    continue;
                }

                BusySlot = new BusyTimeLine(ID, Start, End);
                PrepTime = new TimeSpan(ConvertToMinutes(MyXmlNode.ChildNodes[i].SelectSingleNode("PrepTime").InnerText) * 60 * 10000000);
                //stringToDateTime();
                Start = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("StartTime").InnerText).UtcDateTime;
                End = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("EndTime").InnerText).UtcDateTime;
                Enabled = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Enabled").InnerText);
                bool CompleteFlag = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Complete").InnerText);
                TilerElements.Location var1 = getLocation(MyXmlNode.ChildNodes[i]);
                MiscData noteData = getMiscData(MyXmlNode.ChildNodes[i]);
                EventDisplay UiData = getDisplayUINode(MyXmlNode.ChildNodes[i]);
                ConflictProfile conflictProfile = getConflctProfile(MyXmlNode.ChildNodes[i]);
                string nameString = MyXmlNode.ChildNodes[i].SelectSingleNode("Name")?.InnerText??MyParent.getName.NameValue;
                string id = MyXmlNode.ChildNodes[i].SelectSingleNode("NameId")?.InnerText ?? MyParent.getName.NameId;
                EventName name = new DB_EventName(nameString, id);
                TilerUser creator = getTilerLoggedUser(MyXmlNode.ChildNodes[i].SelectSingleNode("UserNode"));
                if ((creator as DB_TilerUser).isNull)
                {
                    creator = MyParent.getCreator;
                }
                string timeZone = MyXmlNode.ChildNodes[i].SelectSingleNode("TimeZone")?.InnerText ?? "UTC";
                DB_TilerUserGroup userGroup = getTilerUserGroup(MyXmlNode.ChildNodes[i].SelectSingleNode("UserGroup"));
                Tuple<TimeSpan, DateTimeOffset> PauseData = getPauseData(SubEventNode);

                SubCalendarEvent retrievedSubEvent;
                bool procrastinationEventFlag = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("isProcrastinateEvent")?.InnerText??"False");
                if (!procrastinationEventFlag)
                {
                    retrievedSubEvent = new SubCalendarEvent(creator, userGroup, timeZone, ID, name, BusySlot, Start, End, PrepTime, ID, rigidFlag, Enabled, UiData, noteData, CompleteFlag, var1, MyParent.RangeTimeLine, conflictProfile);
                    retrievedSubEvent = new DB_SubCalendarEvent(retrievedSubEvent, MyParent.getNowInfo, MyParent.getProcrastinationInfo);
                    (retrievedSubEvent as DB_SubCalendarEvent).UseTime = PauseData.Item1;
                    (retrievedSubEvent as DB_SubCalendarEvent).PauseTime = PauseData.Item2;
                }
                else
                {
                    DB_ProcrastinateAllSubCalendarEvent procrastinateSubEvent = new DB_ProcrastinateAllSubCalendarEvent(creator, userGroup, timeZone, new TimeLine(Start, End), new EventID(ID), MyParent.Location, MyParent as ProcrastinateCalendarEvent, Enabled, CompleteFlag);
                    procrastinateSubEvent.UseTime = PauseData.Item1;
                    procrastinateSubEvent.PauseTime = PauseData.Item2;
                    retrievedSubEvent = procrastinateSubEvent;
                }

                retrievedSubEvent.ThirdPartyID = MyXmlNode.ChildNodes[i].SelectSingleNode("ThirdPartyID").InnerText;//this is a hack to just update the Third partyID
                XmlNode restrictedNode = MyXmlNode.ChildNodes[i].SelectSingleNode("Restricted");
                


                
                
                if (restrictedNode != null)
                {
                    if (Convert.ToBoolean(restrictedNode.InnerText))
                    { 
                        XmlNode RestrictionProfileNode =MyXmlNode.ChildNodes[i].SelectSingleNode("RestrictionProfile");
                        DB_RestrictionProfile myRestrictionProfile = (DB_RestrictionProfile)getRestrictionProfile(RestrictionProfileNode);
                        retrievedSubEvent = new DB_SubCalendarEventRestricted(retrievedSubEvent, myRestrictionProfile);
                        (retrievedSubEvent as DB_SubCalendarEventRestricted).UsedTime = PauseData.Item1;
                        (retrievedSubEvent as DB_SubCalendarEventRestricted).PauseTime = PauseData.Item2;
                    }
                }

                DateTimeOffset timeCreated;
                XmlNode timeCreatednode = MyXmlNode.ChildNodes[i].SelectSingleNode("TimeCreated");
                if (timeCreatednode != null)
                {
                    timeCreated = DateTimeOffset.Parse(timeCreatednode.ToString());
                    retrievedSubEvent.TimeCreated = timeCreated;
                }

                TimeSpan TravelTimeAfter;
                TimeSpan TravelTimeBefore;
                TimeSpan.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("TravelTimeAfter")?.InnerText ?? "", out TravelTimeAfter);
                TimeSpan.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("TravelTimeBefore")?.InnerText ?? "", out TravelTimeBefore);

                bool isSleep, isWake;
                bool.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("isSleep")?.InnerText ?? "", out isSleep);
                bool.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("isWake")?.InnerText ?? "", out isWake);

                retrievedSubEvent.isSleep = isSleep;
                retrievedSubEvent.isWake = isWake;

                retrievedSubEvent.TravelTimeAfter = TravelTimeAfter;
                retrievedSubEvent.TravelTimeBefore = TravelTimeBefore;
                MyArrayOfNodes.Add(retrievedSubEvent);
                createReasonObjects(retrievedSubEvent, MyXmlNode.ChildNodes[i]);
            }
            
            return MyArrayOfNodes;
        }
        
        void createReasonObjects(dynamic subCalevent, XmlNode node)
        {
            node = node.SelectSingleNode("TimePositionReasons");
            List<Reason> reasons = new List<Reason>();
            if (node != null)
            {
                foreach(XmlNode eachNode in node.ChildNodes)
                {
                    string OPtionName = eachNode.SelectSingleNode("Option").InnerText;
                    Reason generatedReason = createDictionaryOfOPtionToFunction[OPtionName](eachNode);
                    reasons.Add(generatedReason);
                }
            }

            subCalevent.updateReasons(reasons);
        }


        Tuple<TimeSpan, DateTimeOffset> getPauseData (XmlNode ReferenceNode)
        {
            Tuple<TimeSpan, DateTimeOffset> RetValue = new Tuple<TimeSpan, DateTimeOffset>(new TimeSpan(), new DateTimeOffset());
            XmlNode PauseInformation = ReferenceNode.SelectSingleNode("PauseInformation");
            if(PauseInformation!= null)
            {
                TimeSpan UsedUpTime = TimeSpan.Parse(PauseInformation.SelectSingleNode("UsedUpTime").InnerText);
                DateTimeOffset PauseTime = DateTimeOffset.Parse(PauseInformation.SelectSingleNode("PauseTime").InnerText);
                RetValue = new Tuple<TimeSpan, DateTimeOffset>(UsedUpTime, PauseTime);
            }
            
            return RetValue;

        }

        NowProfile generateNowProfile(XmlNode ReferenceNode)
        {
            XmlNode NowProfileNode = ReferenceNode.SelectSingleNode("NowProfile");
            if (NowProfileNode == null)
            {
                return new NowProfile();
            }
            bool initializedFlag = Convert.ToBoolean(NowProfileNode.SelectSingleNode("Initialized").InnerText);
            DateTimeOffset preferredTime = DateTimeOffset.Parse(NowProfileNode.SelectSingleNode("PreferredStart").InnerText).UtcDateTime;
            DB_NowProfile retValue = new DB_NowProfile(preferredTime, initializedFlag);
            return retValue;
        }

        RestrictionProfile getRestrictionProfile(XmlNode RestrictionNode)
        {
            List<RestrictionDay> RestrictionTimeLines = new List<RestrictionDay>();
            foreach (XmlNode eachXmlNode in RestrictionNode.SelectNodes("RestrictionNode"))
            {
                RestrictionTimeLines.Add(getgetRestrictionTuples(eachXmlNode));
            }

            DB_RestrictionProfile retValue = new DB_RestrictionProfile(RestrictionTimeLines);
            return retValue;
        }

        RestrictionDay getgetRestrictionTuples(XmlNode RestrictionTupleNode)
        {
            DayOfWeek myDayOfWeek = getRestrictionDayOfWeek(RestrictionTupleNode);
            RestrictionTimeLine myRestrictionTimeLine = getRestrictionTimeLine(RestrictionTupleNode);
            RestrictionDay retValue = new RestrictionDay(myDayOfWeek, myRestrictionTimeLine);
            return retValue;
        }

        DayOfWeek getRestrictionDayOfWeek(XmlNode RestrictionDayOfWeek)
        {
            DayOfWeek retValue = RestrictionProfile.AllDaysOfWeek[Convert.ToInt32(RestrictionDayOfWeek.SelectSingleNode("RestrictionDayOfWeek").InnerText)];
            return retValue;
        }

        /// <summary>
        /// Funciton returns aall the possible active restriction timelines for the restriciton profile
        /// </summary>
        /// <param name="RestrictionTimeLineNode"></param>
        /// <returns></returns>
        RestrictionTimeLine getRestrictionTimeLine(XmlNode RestrictionTimeLineNode)
        {
            RestrictionTimeLineNode = RestrictionTimeLineNode.SelectSingleNode("RestrictionTimeLineData");
            DateTimeOffset Start = DateTimeOffset.Parse(RestrictionTimeLineNode.SelectSingleNode("StartTime").InnerText).UtcDateTime;
            DateTimeOffset End = DateTimeOffset.Parse(RestrictionTimeLineNode.SelectSingleNode("EndTime").InnerText).UtcDateTime;
            TimeSpan rangeSpan = TimeSpan.FromTicks(Convert.ToInt64(RestrictionTimeLineNode.SelectSingleNode("RangeSpan").InnerText));
            DB_RestrictionTimeLine retValue = new DB_RestrictionTimeLine(Start, End, rangeSpan);
            return retValue;
        }

        Repetition getRepetitionObject(XmlNode RecurrenceXmlNode, TimeLine RangeOfLookUP)
        {
            Repetition RetValue = new Repetition();
            string RepeatStart = RecurrenceXmlNode.SelectSingleNode("RepeatStartDate").InnerText;
            string RepeatEnd = RecurrenceXmlNode.SelectSingleNode("RepeatEndDate").InnerText;
            string RepeatFrequency = RecurrenceXmlNode.SelectSingleNode("RepeatFrequency").InnerText;
            XmlNode XmlNodeWithList = RecurrenceXmlNode.SelectSingleNode("RepeatCalendarEvents");
            int repetitionDay = 7;
            XmlNode RepetitionDayNode = RecurrenceXmlNode.SelectSingleNode("RepeatDay");
            if (RepetitionDayNode != null)
            {
                repetitionDay = Convert.ToInt32(RepetitionDayNode.InnerText);
            }
            XmlNode RepetitionDayNodes = RecurrenceXmlNode.SelectSingleNode("RepeatDays");
            if (RepetitionDayNodes != null)
            {
                XmlNodeList AllRepeatingDays = RecurrenceXmlNode.SelectNodes("Recurrence");
                if (AllRepeatingDays.Count > 0)
                {
                    List<Repetition> repetitionNodes = new List<Repetition>();
                    foreach (XmlNode eacXmlNode in AllRepeatingDays)
                    {
                        repetitionNodes.Add(getRepetitionObject(eacXmlNode, RangeOfLookUP));
                    }

                    RetValue = new Repetition(true, new TimeLine(DateTimeOffset.Parse(RepeatStart).UtcDateTime, DateTimeOffset.Parse(RepeatEnd).UtcDateTime), RepeatFrequency, repetitionNodes.ToArray(), repetitionDay);
                    return RetValue;
                }

            }



            RetValue = new Repetition(true, new TimeLine(DateTimeOffset.Parse(RepeatStart).UtcDateTime, DateTimeOffset.Parse(RepeatEnd).UtcDateTime), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList, RangeOfLookUP), repetitionDay);

            return RetValue;
        }

        CalendarEvent[] getAllRepeatCalendarEvents(XmlNode RepeatEventSchedulesNode, TimeLine RangeOfLookUP)
        {
            XmlNodeList ListOfRepeatEventScheduleNode = RepeatEventSchedulesNode.ChildNodes;
            List<CalendarEvent> ListOfRepeatCalendarNodes = new List<CalendarEvent>();

            foreach (XmlNode MyNode in ListOfRepeatEventScheduleNode)
            {
                CalendarEvent myCalendarEvent = getCalendarEventObjFromNode(MyNode, RangeOfLookUP);
                if (myCalendarEvent != null)
                {
                    ListOfRepeatCalendarNodes.Add(myCalendarEvent);
                }
            }
            return ListOfRepeatCalendarNodes.ToArray();
        }
        TilerElements.Location getLocation(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("Location");
            bool UninitializedLocation = false;
            return generateLocationObjectFromNode(var1);
        }

        TilerElements.Location generateLocationObjectFromNode(XmlNode var1)
        {
            bool UninitializedLocation = false;
            if (var1 == null)
            {
#if UseDefaultLocation
                return DefaultLocation;
#else
                return new TilerElements.Location();
#endif
            }
            else
            {
                string XCoordinate_Str = var1.SelectSingleNode("XCoordinate").InnerText;
                string YCoordinate_Str = var1.SelectSingleNode("YCoordinate").InnerText;
                string Descripion = var1.SelectSingleNode("Description").InnerText;
                Descripion = string.IsNullOrEmpty(Descripion) ? "" : Descripion;
                string Address = var1.SelectSingleNode("Address").InnerText;
                Address = string.IsNullOrEmpty(Address) ? "" : Address;
                UninitializedLocation = Convert.ToBoolean(var1.SelectSingleNode("isNull").InnerText);
                string CheckDefault_Str = var1.SelectSingleNode("CheckCalendarEvent") == null ? 0.ToString() : var1.SelectSingleNode("CheckCalendarEvent").InnerText;
#if UseDefaultLocation
                if (UninitializedLocation)
                { return DefaultLocation; }
#endif

                if (string.IsNullOrEmpty(XCoordinate_Str) || string.IsNullOrEmpty(YCoordinate_Str))
                {
                    return new TilerElements.Location(Address);
                }
                else
                {
                    double xCoOrdinate = double.MaxValue;
                    double yCoOrdinate = double.MaxValue;

                    bool isDefault;
                    if(CheckDefault_Str == "0")
                    {
                        isDefault = false;
                    }
                    else
                    {
                        isDefault = Convert.ToBoolean(CheckDefault_Str);
                    }
                        

                    if (!(double.TryParse(XCoordinate_Str, out xCoOrdinate)))
                    {
                        xCoOrdinate = TilerElements.Location.MaxLatitude;
                        UninitializedLocation = true;
#if UseDefaultLocation
                        return DefaultLocation;
#endif
                    }

                    if (!(double.TryParse(YCoordinate_Str, out yCoOrdinate)))
                    {
                        yCoOrdinate = double.MaxValue;
                        UninitializedLocation = true;
#if UseDefaultLocation
                        return DefaultLocation;
#endif
                    }

                    return new TilerElements.Location(xCoOrdinate, yCoOrdinate, Address, Descripion, UninitializedLocation, isDefault);
                }
            }
        }
        MiscData getMiscData(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("MiscData");
            string stringData = (var1.SelectSingleNode("UserNote").InnerText);
            int NoteData = Convert.ToInt32(var1.SelectSingleNode("TypeSelection").InnerText);
            MiscData retValue = new MiscData(stringData, NoteData);
            return retValue;
        }
        public EventDisplay getDisplayUINode(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("UIParams");
            int DefaultFlag = Convert.ToInt32(var1.SelectSingleNode("Type").InnerText);
            bool DisplayFlag = Convert.ToBoolean(var1.SelectSingleNode("Visible").InnerText);
            EventDisplay retValue;
            TilerColor colorNode = getColorNode(var1);
            if (DefaultFlag == 0)
            {
                retValue = new EventDisplay();
            }
            else
            {

                retValue = new EventDisplay(DisplayFlag, colorNode, DefaultFlag);
            }
            return retValue;
        }

        public ConflictProfile getConflctProfile(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("ConflictProfile");
            ConflictProfile retValue = new ConflictProfile();


            if (var1 != null)
            {
                int typeOfConflict = Convert.ToInt32(var1.SelectSingleNode("Type").InnerText);
                bool conflictFlag = Convert.ToBoolean(var1.SelectSingleNode("Flag").InnerText);
                retValue = new ConflictProfile(typeOfConflict, conflictFlag);
                IEnumerable<string> conflictinIDs = getConflictingIDS(var1);
                retValue.LoadConflictingIDs(conflictinIDs);
            }
            return retValue;
        }

        public HashSet<string> getConflictingIDS(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("ConflictIDs");
            HashSet<string> retValue = new HashSet<string>();
            XmlNodeList EventIDs = var1.SelectNodes("ConflictID");
            foreach (XmlNode eachXmlNode in EventIDs)
            {
                retValue.Add(eachXmlNode.InnerText);
            }
            return retValue;
        }

        public TilerColor getColorNode(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("Color");
            int b = Convert.ToInt32(var1.SelectSingleNode("Blue").InnerText);
            int g = Convert.ToInt32(var1.SelectSingleNode("Green").InnerText);
            int r = Convert.ToInt32(var1.SelectSingleNode("Red").InnerText);
            double o = Convert.ToDouble(var1.SelectSingleNode("Opacity").InnerText);
            XmlNode userSelectionNode = var1.SelectSingleNode("UserSelection");

            string userSelection_String = "";
            if (userSelectionNode != null)
            {
                userSelection_String = userSelectionNode.InnerText;
            }
            if (string.IsNullOrEmpty(userSelection_String))
            {
                userSelection_String = "-1";
            }
            int UserSelection = Convert.ToInt32(userSelection_String);

            TilerColor retValue = new TilerColor(r, g, b, o, UserSelection);
            return retValue;
        }

        public CalendarEvent getCalendarEventWithID(string ID)
        {
            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.UtcNow.AddYears(-1000), DateTimeOffset.UtcNow.AddYears(1000));
            Dictionary<string, CalendarEvent> AllScheduleData = getAllCalendarFromXml(RangeOfLookup);
            CalendarEvent retValue = null;
            if (AllScheduleData.ContainsKey(ID))
            {
                retValue = AllScheduleData[ID];
            }
            return retValue;
        }

        public IList<CalendarEvent> getCalendarEventWithName(string Name)
        {
            IList<CalendarEvent> retValue = new CalendarEvent[0];
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                retValue = myCassandraAccess.SearchEventsByName(Name);
                return retValue;
            }
#endif

            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.UtcNow.AddYears(-1000), DateTimeOffset.UtcNow.AddYears(1000));
            Dictionary<string, CalendarEvent> AllScheduleData = getAllCalendarFromXml(RangeOfLookup);
            
            Name = Name.ToLower();
            if (AllScheduleData.Count > 0)
            {
                retValue = AllScheduleData.Values.Where(obj => obj.getName.NameValue.ToLower().Contains(Name)).ToList();
            }

            return retValue;
        }


        async public Task<IList<TilerElements.Location>> getCachedLocationByName(string Name)
        {

            IList<TilerElements.Location> retValue = new List<TilerElements.Location>();
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                retValue = myCassandraAccess.SearchLocationByName(Name);
                return retValue;
            }
#endif
            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.UtcNow.AddYears(-1000), DateTimeOffset.UtcNow.AddYears(1000));
            CachedLocation = await getLocationCache().ConfigureAwait(false); ;
            

            Name = Name.ToLower();
            if (CachedLocation.Count > 0)
            {
                retValue = CachedLocation.Select(obj => obj.Value).Where(obj1 => obj1.Description.ToLower().Contains(Name) || obj1.Address.ToLower().Contains(Name)).ToList();
            }
            return retValue;
        }
        #endregion

        private DateTimeOffset stringToDateTime(string MyDateTimeString)//String should be in format "MM/DD/YY HH:MM:SSA"
        {

            DateTimeOffset MyDateTime, MyNow;
            MyDateTime = DateTimeOffset.Parse(MyDateTimeString).UtcDateTime;


            return MyDateTime;
        }

        public static DateTimeOffset ConvertToDateTime(string StringOfDateTime)
        {
            string[] strArray = StringOfDateTime.Split(new char[] { '|' });
            string[] strArray2 = strArray[0].Split(new char[] { ' ' });
            string[] strArray3 = strArray[1].Split(new char[] { ' ' });
            return new DateTimeOffset(Convert.ToInt16(strArray2[0]), Convert.ToInt16(strArray2[1]), Convert.ToInt16(strArray2[2]), Convert.ToInt16(strArray3[0]), Convert.ToInt16(strArray3[1]), Convert.ToInt16(strArray3[2]), 0, new TimeSpan());
        }
        static public uint ConvertToMinutes(string TimeEntry)
        {
            int MaxTimeIndexCounter = 5;
            string[] ArrayOfTimeComponent = TimeEntry.Split(':');
            Array.Reverse(ArrayOfTimeComponent);
            uint TotalMinutes = 0;
            for (int x = 0; x < ArrayOfTimeComponent.Length; x++)
            {
                int Multiplier = 0;
                switch (x)
                {
                    case 0:
                        Multiplier = 0;
                        break;
                    case 1:
                        Multiplier = 1;
                        break;
                    case 2:
                        Multiplier = 60;
                        break;
                    case 3:
                        Multiplier = 36 * 24;
                        break;
                    case 4:
                        Multiplier = 36 * 24 * 365;
                        break;
                }
                string JustHold = ArrayOfTimeComponent[x];
                Int64 MyNumber = (Int64)Convert.ToDouble(JustHold);
                TotalMinutes = (uint)(TotalMinutes + (Multiplier * MyNumber));

            }

            return TotalMinutes;

        }



        async public Task<Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>>> getProfileInfo(TimeLine RangeOfLookup = null)
        {
            //getLocationCache
            if (RangeOfLookup == null)
            {
                RangeOfLookup = new TimeLine(DateTimeOffset.UtcNow.AddYears(-10), DateTimeOffset.UtcNow.AddYears(10));
            }

            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>> retValue;
            if (this.Status)
            {
                Task<Dictionary<string, TilerElements.Location>> TaskLocationCache = getLocationCache();
                
                Dictionary<string, CalendarEvent> AllScheduleData = this.getAllCalendarFromXml(RangeOfLookup);
                Task<DateTimeOffset> getReferenceTime = getDayReferenceTime();


                Dictionary<string, TilerElements.Location> LocationCache = await TaskLocationCache.ConfigureAwait(false);
                await populateDefaultLocation(LocationCache).ConfigureAwait(false);
                DateTimeOffset ReferenceTime = await getReferenceTime.ConfigureAwait(false);
                
                retValue = new Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>>(AllScheduleData, ReferenceTime, LocationCache);
            }
            else
            {
                retValue = null;
            }
            return retValue;
        }


        

        async protected Task populateDefaultLocation(Dictionary<string, TilerElements.Location> locations = null)
        {
            double xLocation = 40.083319;
            double yLocation = -105.3505482;
            TilerElements.Location retValue;
            if ((locations == null))
            {

                retValue = TilerElements.Location.getDefaultLocation();
                DefaultLocation = retValue;
                locations = await getLocationCache().ConfigureAwait(false);
                return;
            }

            if ((locations.Count < 1))
            {
                retValue = TilerElements.Location.getDefaultLocation();
                DefaultLocation = retValue;
                locations = await getLocationCache().ConfigureAwait(false);
                return;
            }

            TilerElements.Location AverageLocations = TilerElements.Location.AverageGPSLocation(locations.Values);

            xLocation = AverageLocations.XCoordinate;
            yLocation = AverageLocations.YCoordinate;
            TilerElements.Location.InitializeDefaultLongLat(xLocation, yLocation);
            DefaultLocation = AverageLocations;
            
        }
        #endregion

        #region Properties

        public int LastUserID
        {
            get
            {
                return Convert.ToInt32(LastIDNumber);
            }
        }

        virtual public bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        virtual public string LoggedUserID
        {
            get
            {
                return ID;
            }
        }


        virtual public string getFullLogDir
        {
            get
            {
                return WagTapLogLocation + CurrentLog;
            }

        }

        virtual public TilerElements.Location defaultLocation
        {
            get
            {
                return DefaultLocation;
            }
        }

        virtual public string Usersname
        {
            get
            {
                return NameOfUser;
            }
        }

        public static string LogLocation
        {
            get
            {
                return WagTapLogLocation;
            }
        }

        #endregion
    }

}
