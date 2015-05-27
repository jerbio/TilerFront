#define UseDefaultLocation

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
#if ForceReadFromXml
#else
using CassandraUserLog;
using TilerSearch;
#endif



namespace TilerFront
{
    public class LogControl
    {
        protected string ID;
        protected string UserName;
        string NameOfUser;
        protected static string WagTapLogLocation = "WagTapCalLogs\\";
        protected DBControl LogDBDataAccess;
        protected long LastIDNumber;
        protected int LastLocationID;
        protected string CurrentLog;
        protected bool LogStatus;
        protected Dictionary<string, Location_Elements> CachedLocation;
        protected Location_Elements DefaultLocation= new Location_Elements();

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
            LastLocationID= 0;
            CurrentLog="";
            LogStatus=false;
#if ForceReadFromXml
#else
            NameSearcher = new EventNameSearchHandler();
            LocationSearcher = new LocationSearchHandler();
#endif
            Dictionary<string, Location_Elements> CachedLocation = new Dictionary<string, Location_Elements>();
        }


        public LogControl(DBControl DBAccess, string logLocation = "")
        {
            if (!string.IsNullOrEmpty(logLocation))
            {
                WagTapLogLocation = logLocation;
            }
            LogDBDataAccess = DBAccess;
            LogStatus = false;
            CachedLocation = new Dictionary<string, Location_Elements>();
        }
        #region Functions
        virtual async public Task Initialize()
        {
            Tuple<bool, string, string> VerifiedUser = LogDBDataAccess.LogIn();
            CurrentLog = "";
            if (VerifiedUser.Item1)
            {
                Tuple<bool, string, DateTimeOffset, long> resultofLatestChange = await LogDBDataAccess.getLatestChanges(VerifiedUser.Item2).ConfigureAwait(false);
                ID = VerifiedUser.Item2;
#if ForceReadFromXml
#else
                myCassandraAccess = new CassandraUserLog.CassandraLog(ID);
#endif

                if (!resultofLatestChange.Item1)
                {
                    CurrentLog = ID.ToString() + ".xml";
                    string LogDir = (WagTapLogLocation + CurrentLog);
                    string myCurrDir = Directory.GetCurrentDirectory();
                    Console.WriteLine("Log DIR is:" + LogDir);
                    LogStatus = File.Exists(LogDir);
#if ForceReadFromXml

#else
                    Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>> tempProfileData = await getProfileInfo();
                    await LogDBDataAccess.CreateLatestChange(this.ID, new DateTimeOffset(), Convert.ToInt64(LastIDNumber));
                    resultofLatestChange = await LogDBDataAccess.getLatestChanges(VerifiedUser.Item2);
                    myCassandraAccess.BatchMigrateXMLToCassandra(this);
#endif
                }
                else
                {
                    
#if ForceReadFromXml
                    CurrentLog = ID.ToString() + ".xml";
                    string LogDir = (WagTapLogLocation + CurrentLog);
                    string myCurrDir = Directory.GetCurrentDirectory();
                    Console.WriteLine("Log DIR is:" + LogDir);
                    LogStatus = File.Exists(LogDir);
#else
                    LogStatus = true;
                    LastIDNumber = resultofLatestChange.Item4;
                    useCassandra = true;
#endif

                    ScheduleMetadata = resultofLatestChange;
                }

                NameOfUser = VerifiedUser.Item3;
            }
        }


        public static void UpdateLogLocation(string LogLocation)
        {
            WagTapLogLocation = LogLocation;
        }
        public static string getLogLocation()
        {
            return WagTapLogLocation;
        }

        
#if ForceReadFromXml
#else
        async internal Task BatchMigrateXML()
        {
            bool oldCassandraValue = useCassandra;
            useCassandra=false;

            getAllCalendarFromXml(new TimeLine());//empty TImeline, just doing this to force initialization of LastIDNumber
            Task<bool> createLatestChange = LogDBDataAccess.CreateLatestChange(ID, DateTimeOffset.Now, LastIDNumber);
            
            Task AddAllEvents = NameSearcher.Add(getAllCalendarFromXml(new TimeLine(new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan()), new DateTimeOffset(2970, 1, 1, 0, 0, 0, new TimeSpan()))), ID);
            Task<Dictionary<string, Location_Elements>> LocationCache = (getLocationCache());
            Task AddAllLocations = LocationSearcher.Add((await LocationCache).Values, ID);
            myCassandraAccess.BatchMigrateXMLToCassandra(this);

            bool status = await createLatestChange;
            await AddAllEvents;
            await AddAllLocations;
            useCassandra = oldCassandraValue;
            return;

            
        }
        public Task<bool> CommitToCassadra(IEnumerable<CalendarEvent> AllCalEvents)
        {
            return myCassandraAccess.Commit(AllCalEvents);
        }

        /// <summary>
        /// Adds a new event to the cassandra db. Do not use with the old XML system.
        /// </summary>
        /// <param name="myEvent"></param>
        async public Task AddNewEventToCassandra(CalendarEvent newEvent)
        {
            await myCassandraAccess.AddNewEventToTiler(newEvent);
        }
#endif


        #region Write Data

        public void Undo(string LogFile = "")
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

            while (true)
            {
                try
                {
                    xmldoc.Save(LogFile);
                    xmldocCopy.Save(LogFileCopy);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);
                }
            }

        }

        public CustomErrors genereateNewLogFile(string UserID)//creates a new xml log file. Uses the passed UserID
        {
            
            CustomErrors retValue = new CustomErrors(false, "success");
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
                retValue = new CustomErrors(true, "Error generating log\n" + e.ToString(), 20000000);
            }

            return retValue;

        }

        public async Task<CustomErrors> DeleteLog()
        {
            CustomErrors retValue = new CustomErrors(false, "Success");
            try
            {
                string NameOfFile = WagTapLogLocation + CurrentLog;
                File.Delete(NameOfFile);
            }
            catch (Exception e)
            {
                retValue = new CustomErrors(true, e.ToString(), 20002000);
            }

            return retValue;
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

        /*
        public void WriteToLog(CalendarEvent MyEvent, string LogFile = "")//writes to an XML Log file. Takes calendar event as an argument
        {
            if (LogFile == "")
            { LogFile = WagTapLogLocation + CurrentLog; }
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(LogFile);
            CachedLocation = getLocationCache();//populates with current location info

            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = MyEvent.ID;
            XmlNodeList EventSchedulesNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules");
            XmlNodeList EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");

            XmlElement EventScheduleNode = CreateEventScheduleNode(MyEvent);
            //EventSchedulesNodes[0].PrependChild(xmldoc.CreateElement("EventSchedule"));
            //EventSchedulesNodes[0].ChildNodes[0].InnerXml = CreateEventScheduleNode(MyEvent).InnerXml;
            XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
            //(EventScheduleNode, true);
            if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.ID.ToString(), EventScheduleNode))
            {
                xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules").AppendChild(MyImportedNode);
            }

            UpdateCacheLocation(xmldoc);
            while (true)
            {
                try
                {
                    xmldoc.Save(LogFile);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);
                }
            }
        }
        */
        async public Task<bool> WriteToLogOld(IEnumerable<CalendarEvent> AllEvents, string LatestID, string LogFile = "")
        {
            Task<bool>  retValue;
            
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
                catch(Exception e)
                {
                    Console.Write(e.Message);
                }
                
            }


            xmldocCopy.InnerXml = xmldoc.InnerXml;
            CachedLocation = await getLocationCache().ConfigureAwait(false); ;//populates with current location info
            Dictionary<string, Location_Elements> OldLocationCache = new Dictionary<string, Location_Elements>(CachedLocation);
            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = LatestID;
            XmlNodeList EventSchedulesNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules");
            
            XmlNode EventSchedulesNodesNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            XmlNode EventSchedulesNodesNodeCpy = xmldoc.CreateElement("NodeCopy");// .DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            EventSchedulesNodesNodeCpy.InnerXml = EventSchedulesNodesNode.InnerXml;
            EventSchedulesNodesNode.RemoveAll();
            XmlNodeList EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");
            bool errorWritingFile = false;
            CalendarEvent ErrorEvent = new CalendarEvent();
            EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");
            DateTimeOffset purgeLimit = DateTimeOffset.UtcNow.AddMonths(-3);
            try
            {
                foreach (CalendarEvent MyEvent in AllEvents)
                {
                    //break;
                    if (MyEvent.End > purgeLimit)
                    {
                        XmlElement EventScheduleNode;
                        //EventScheduleNode = CreateEventScheduleNode(MyEvent);
                        //*
                        ErrorEvent = MyEvent;
                        EventScheduleNode = CreateEventScheduleNode(MyEvent);
                
                
                        //*/
                
                        //EventSchedulesNodes[0].PrependChild(xmldoc.CreateElement("EventSchedule"));
                        //EventSchedulesNodes[0].ChildNodes[0].InnerXml = CreateEventScheduleNode(MyEvent).InnerXml;
                        XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
                        //(EventScheduleNode, true);
                        if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.ID.ToString(), EventScheduleNode))
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
            catch
            {
                EventSchedulesNodesNode.InnerXml = EventSchedulesNodesNodeCpy.InnerXml;
                errorWritingFile = true;
            }

            UpdateCacheLocation(xmldoc, OldLocationCache);

            while (true)
            {
                try
                {
                    xmldoc.Save(LogFile);
                    xmldocCopy.Save(LogFileCopy);
                    break;
                }
                catch (Exception e)
                {
                    Thread.Sleep(160);
                }
            }

            if(errorWritingFile)
            {
                throw new Exception("Error wrtiting file" + ErrorEvent.Name);
            }

            return await retValue.ConfigureAwait(false); ;
        }

        public void UpdateCacheLocation(XmlDocument xmldoc, Dictionary<string, Location_Elements> currentCache)
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

            LastLocationID = Convert.ToInt32(LocationCacheNode.SelectSingleNode("LastID").InnerText);//gets the last Location ID from xmldoc

            XmlNode AllLocationsCacheContainer = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache/Locations");

            XmlNodeList AllCachedLocations = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/LocationCache/Locations/Location");

            int LocationID = LastLocationID;
            foreach (KeyValuePair<string, Location_Elements> eachKeyValuePair in CachedLocation)
            {
                if (!currentCache.ContainsKey(eachKeyValuePair.Key))
                {
                    if (eachKeyValuePair.Value.ID == 0)//new cached location detected
                    {
                        LocationID = ++LastLocationID;
                    }
                    else //Location already in cache
                    {
                        LocationID = eachKeyValuePair.Value.ID;
                    }

                    XmlElement myCacheLocationNode = CreateLocationNode(eachKeyValuePair.Value, "Location");

                    XmlNode MyImportedNode = xmldoc.ImportNode(myCacheLocationNode as XmlNode, true);
                    myCacheLocationNode = MyImportedNode as XmlElement;


                    XmlNode LocationIDNOde = xmldoc.CreateElement("LocationID");
                    XmlNode CacheNameNode = xmldoc.CreateElement("CachedName");
                    CacheNameNode.InnerText = eachKeyValuePair.Value.Description.ToLower();
                    LocationIDNOde.InnerText = LocationID.ToString();
                    MyImportedNode.PrependChild(LocationIDNOde);
                    MyImportedNode.PrependChild(CacheNameNode);
                    MyImportedNode = xmldoc.ImportNode(myCacheLocationNode as XmlNode, true);

                    if (!UpdateInnerXml(ref AllCachedLocations, "LocationID", LocationID.ToString(), myCacheLocationNode))
                    {
                        xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LocationCache/Locations").AppendChild(MyImportedNode);
                    }
                }
            }

            LocationCacheNode.SelectSingleNode("LastID").InnerXml = LastLocationID.ToString();
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


        public XmlElement CreateLocationCacheNode()
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement LocationCacheNode = xmldoc.CreateElement("LocationCache");
            XmlElement LastIDNode = xmldoc.CreateElement("LastID");
            XmlElement LocationsNode = xmldoc.CreateElement("Locations");
            LastLocationID = 1;
            LastIDNode.InnerText = LastLocationID.ToString();
            LocationCacheNode.PrependChild(LastIDNode);
            LocationCacheNode.PrependChild(LocationsNode);
            return LocationCacheNode;
        }



        public XmlElement CreateEventScheduleNode(CalendarEvent MyEvent)
        {
            XmlDocument xmldoc = new XmlDocument();


            XmlElement MyEventScheduleNode = xmldoc.CreateElement("EventSchedule");
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Completed"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isComplete.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RepetitionFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.RepetitionStatus.ToString();

            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("EventSubSchedules"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Repetition.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RigidFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Rigid.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Duration"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ActiveDuration.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Split"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.NumberOfSplit.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Deadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.End.UtcDateTime.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Preparation.ToString();
            //MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadlineFlag"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Pre.ToString();
            
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("CompletionCount"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.CompletionCount.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("DeletionCount"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.DeletionCount.ToString();

            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.PreDeadline.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Start.UtcDateTime.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Name.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEnabled.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.myLocation, "EventScheduleLocation").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MyEvent.UIParam, "UIParams").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MyEvent.Notes, "MiscData").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Restricted"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEventRestricted.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("NowProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateNowProfileNode(MyEvent.NowInfo).InnerXml);
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ProcrastinationProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateProcrastinationNode(MyEvent.ProcrastinationInfo).InnerXml);
            
            if (MyEvent.isEventRestricted)
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
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ID;


            return MyEventScheduleNode;
        }

        public DateTimeOffset Truncate(DateTimeOffset dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
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
            RepeatCalendarEventsNode.ChildNodes[0].InnerText = RepetitionObjEntry.Frequency;
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
            List<Tuple<DayOfWeek, RestrictionTimeLine>> ActiveRestrictions = RestrictionProfileData.getActiveDays();
            foreach (Tuple<DayOfWeek, RestrictionTimeLine> eachTuple in ActiveRestrictions)
            {
                XmlElement RestrictionNode = xmldoc.CreateElement("RestrictionNode");
                XmlElement RestrictionDayOfWeekNode = xmldoc.CreateElement("RestrictionDayOfWeek");
                RestrictionDayOfWeekNode.InnerText = ((int)eachTuple.Item1).ToString();
                XmlElement RestrictionTimeLineNode = xmldoc.CreateElement("RestrictionTimeLineData");
                RestrictionTimeLineNode.InnerXml=generateRestrictionTimeLineNode(eachTuple.Item2).InnerXml;
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
            StartTime = Truncate(StartTime, TimeSpan.FromSeconds(1));
            DateTimeOffset EndTime = MySubEvent.End;
            EndTime = Truncate(EndTime, TimeSpan.FromSeconds(1));
            TimeSpan EventTimeSpan = MySubEvent.ActiveDuration;
            long AllSecs = (long)EventTimeSpan.TotalSeconds;
            long AllTicks = (long)EventTimeSpan.TotalMilliseconds;
            long DiffSecs = (long)(EndTime - StartTime).TotalSeconds;
            long DiffTicks = (long)(EndTime - StartTime).TotalMilliseconds;
            EventTimeSpan = TimeSpan.FromSeconds(AllSecs);
            if ((EndTime - StartTime) != EventTimeSpan)
            {
                EndTime = StartTime.Add(EventTimeSpan);
            }

            if ((!string.IsNullOrEmpty(MySubEvent.myLocation.Description)) || (MySubEvent.myLocation.ID != 0))
            {
                string TaggedLocation = MySubEvent.myLocation.Description;
                TaggedLocation = TaggedLocation.ToLower();
                if (!CachedLocation.ContainsKey(TaggedLocation))
                {
                    CachedLocation.Add(TaggedLocation, MySubEvent.myLocation);
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
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Preparation.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ThirdPartyID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ThirdPartyID;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Rigid"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.Rigid.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ID.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isEnabled.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Complete"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isComplete.ToString();
            
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.myLocation, "EventSubScheduleLocation").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MySubEvent.UIParam, "UIParams").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MySubEvent.Notes, "MiscData").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ConflictProfile"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateConflictProfile(MySubEvent.Conflicts, "ConflictProfile").InnerXml;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Restricted"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isEventRestricted.ToString();
            if (MySubEvent.isEventRestricted)
            {
                restrictedMySub = (SubCalendarEventRestricted)MySubEvent;
                XmlElement restrictionProfileData =  generateXMLRestrictionProfile(restrictedMySub.RetrictionInfo);
                MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("RestrictionProfile"));
                MyEventSubScheduleNode.ChildNodes[0].InnerXml = restrictionProfileData.InnerXml;
            }


            return MyEventSubScheduleNode;
        }



        public XmlElement CreateLocationNode(Location_Elements Arg1, string ElementIdentifier)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement var1 = xmldoc.CreateElement(ElementIdentifier);
            string XCoordinate = "";
            string YCoordinate = "";
            string Descripion = "";
            string MappedAddress = "";
            string IsNull = true.ToString(); ;
            string CheckCalendarEvent = 0.ToString();
            if ((Arg1 != null) && (Arg1.ID != -1))
            {
                XCoordinate = Arg1.XCoordinate.ToString();
                YCoordinate = Arg1.YCoordinate.ToString();
                Descripion = Arg1.Description;
                MappedAddress = Arg1.Address;
                IsNull = Arg1.isNull.ToString();
                CheckCalendarEvent = Arg1.DefaultCheck.ToString();
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
                }
            }

            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            EventSchedulesNodes.InnerText = "";
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
                }
            }

        }
        #endregion

        #region Read Data


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

        virtual public async Task<DateTimeOffset> getDayReferenceTime(string NameOfFile = "")
        {
#if ForceReadFromXml
#else
            if (useCassandra)
            {
#if LocalDebug
                return new DateTimeOffset(1970, 1, 1, 16, 0, 0, new TimeSpan());
#else
                return SessionUser.LastChange;
#endif
            }
#endif       
            XmlDocument doc = getLogDataStore(NameOfFile);
            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/referenceDay");
            DateTimeOffset retValue = DateTimeOffset.Parse(node.InnerText).UtcDateTime;

            return retValue;
        }

        private XmlDocument getLogDataStore(string NameOfFile = "")
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
                    ;

                }
            }

            return doc;
        }

        async protected Task<Dictionary<string, Location_Elements>> getLocationCache(string NameOfFile = "")
        {
            Dictionary<string, Location_Elements> retValue = new Dictionary<string, Location_Elements>();
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
                LastLocationID = 1;
                return retValue;
            }
            LastLocationID = Convert.ToInt32(node.SelectSingleNode("LastID").InnerText);
            XmlNodeList AllLocationNodes = node.SelectNodes("Locations/Location");
            foreach (XmlNode eachXmlNode in AllLocationNodes)
            {
                Location_Elements myLocation = generateLocationObjectFromNode(eachXmlNode);

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



        public Dictionary<string, CalendarEvent> getAllCalendarFromXml(TimeLine RangeOfLookUP)
        {
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                return myCassandraAccess.getAllCalendarEvent();
            }
#endif

            XmlDocument doc = getLogDataStore();
            Dictionary<string, CalendarEvent> MyCalendarEventDictionary = new Dictionary<string, CalendarEvent>();


            XmlNode node = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");
            string LastUsedIndex = node.InnerText;
            LastIDNumber =Convert.ToInt64( LastUsedIndex);
            DateTimeOffset userReferenceDay;
            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");

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

            if (EventSchedulesNodes.ChildNodes!=null)
            {
                foreach (XmlNode EventScheduleNode in EventSchedulesNodes.ChildNodes)
                {
                    CalendarEvent RetrievedEvent;

                    //RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode, RangeOfLookUP);
                    ///*
                    try
                    {
                        RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode, RangeOfLookUP);
                    }
                    catch
                    {
                        RetrievedEvent = new CalendarEvent();
                    }
                    //*/

                    if (RetrievedEvent != null)
                    { MyCalendarEventDictionary.Add(RetrievedEvent.Calendar_EventID.getCalendarEventComponent(), RetrievedEvent); }
                }

            }
            

            return MyCalendarEventDictionary;
        }
        public CalendarEvent getCalendarEventObjFromNode(XmlNode EventScheduleNode, TimeLine RangeOfLookUP)
        {
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
            string RepeatStart;
            string RepeatEnd;
            string RepeatFrequency;
            string LocationData;
            string EnableFlag;

            
            
            
            Name = EventScheduleNode.SelectSingleNode("Name").InnerText;
            ID = EventScheduleNode.SelectSingleNode("ID").InnerText;
            //EventScheduleNode.SelectSingleNode("ID").InnerXml = "<wetin></wetin>";
            Deadline = EventScheduleNode.SelectSingleNode("Deadline").InnerText;
            Rigid = EventScheduleNode.SelectSingleNode("RigidFlag").InnerText;
            XmlNode RecurrenceXmlNode = EventScheduleNode.SelectSingleNode("Recurrence");
            EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;


            DateTimeOffset StartDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("StartTime").InnerText).UtcDateTime;
            DateTimeOffset EndDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("Deadline").InnerText).UtcDateTime;
            StartDateTime = EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');

            StartDate = StartDateTime[0];
            StartTime = StartDateTime[1] + StartDateTime[2];
            EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
            EndDate = EndDateTime[0];
            EndTime = EndDateTime[1] + EndDateTime[2];
            DateTimeOffset StartTimeConverted = DateTimeOffset.Parse(StartDate).UtcDateTime;// new DateTimeOffset(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
            DateTimeOffset EndTimeConverted = DateTimeOffset.Parse(EndDate).UtcDateTime; //new DateTimeOffset(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));
            /*
            if (RangeOfLookUP.InterferringTimeLine(new TimeLine(StartDateTimeStruct, EndDateTimeStruct)) == null)
            {
                return null;
            }
            */

            Repetition Recurrence;
            if (Convert.ToBoolean(EventRepetitionflag))
            {
                /*
                RepeatStart = RecurrenceXmlNode.SelectSingleNode("RepeatStartDate").InnerText;
                RepeatEnd = RecurrenceXmlNode.SelectSingleNode("RepeatEndDate").InnerText;
                RepeatFrequency = RecurrenceXmlNode.SelectSingleNode("RepeatFrequency").InnerText;
                XmlNode XmlNodeWithList = RecurrenceXmlNode.SelectSingleNode("RepeatCalendarEvents");
                Recurrence = new Repetition(true, new TimeLine(DateTimeOffset.Parse(RepeatStart), DateTimeOffset.Parse(RepeatEnd)), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList, RangeOfLookUP));*/
                Recurrence = getRepetitionObject(RecurrenceXmlNode, RangeOfLookUP); ;

                StartTimeConverted = (Recurrence.Range.Start);
                EndTimeConverted = (Recurrence.Range.End);
            }
            else
            {
                Recurrence = new Repetition();
            }
            Split = EventScheduleNode.SelectSingleNode("Split").InnerText;
            PreDeadline = EventScheduleNode.SelectSingleNode("PreDeadline").InnerText;
            //PreDeadlineFlag = EventScheduleNode.SelectSingleNode("PreDeadlineFlag").InnerText;
            CalendarEventDuration = EventScheduleNode.SelectSingleNode("Duration").InnerText;
            //EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
            //PrepTimeFlag = EventScheduleNode.SelectSingleNode("PrepTimeFlag").InnerText;
            PrepTime = EventScheduleNode.SelectSingleNode("PrepTime").InnerText;
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

            Location_Elements var3 = getLocation(EventScheduleNode);
            MiscData noteData = getMiscData(EventScheduleNode);
            EventDisplay UiData = getDisplayUINode(EventScheduleNode);
            Procrastination procrastinationData = generateProcrastinationObject(EventScheduleNode);
            NowProfile NowProfileData = generateNowProfile(EventScheduleNode);

            CalendarEvent RetrievedEvent = new CalendarEvent(ID, Name, StartTime, StartTimeConverted, EndTime, EndTimeConverted, Split, PreDeadline, CalendarEventDuration, Recurrence, false, Convert.ToBoolean(Rigid), PrepTime, false, var3, EVentEnableFlag, UiData, noteData, completedFlag);
            RetrievedEvent = new DB_CalendarEventExtra(RetrievedEvent, procrastinationData, NowProfileData);

            SubCalendarEvent[] AllSubCalEvents = ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent, RangeOfLookUP).ToArray();
            //AllSubCalEvents = AllSubCalEvents.Select(obj => new DB_SubCalendarEvent(obj, NowProfileData, procrastinationData)).ToArray();
            
            //AllSubCalEvents
            XmlNode restrictedNode = EventScheduleNode.SelectSingleNode("Restricted");
            
            
            /*if (AllSubCalEvents.Length < 1)
            {
                return null;
            }*/
            RetrievedEvent = new CalendarEvent(RetrievedEvent, AllSubCalEvents);
            
            
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
                CompleteCount = RetrievedEvent.AllSubEvents.Where(obj => obj.isComplete).Count();
            }


            if ((deleteNode != null))
            {
                DeleteCount = Convert.ToInt32(deleteNode.InnerText);
            }
            else
            {
                DeleteCount = RetrievedEvent.AllSubEvents.Where(obj => !obj.isEnabled).Count();
            }
            RetrievedEvent.InitializeCounts(DeleteCount, CompleteCount);

            return RetrievedEvent;
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
        
        List<SubCalendarEvent> ReadSubSchedulesFromXMLNode(XmlNode MyXmlNode, CalendarEvent MyParent, TimeLine RangeOfLookUP)
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
                BusyTimeLine SubEventActivePeriod = new BusyTimeLine(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText, stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText), stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText));
                ID = EventID.convertToSubcalendarEventID(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText).ToString();
                Start = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText).UtcDateTime;
                End = DateTimeOffset.Parse(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText).UtcDateTime;

                bool rigidFlag =MyParent.Rigid;
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
                Location_Elements var1 = getLocation(MyXmlNode.ChildNodes[i]);
                MiscData noteData = getMiscData(MyXmlNode.ChildNodes[i]);
                EventDisplay UiData = getDisplayUINode(MyXmlNode.ChildNodes[i]);
                ConflictProfile conflictProfile = getConflctProfile(MyXmlNode.ChildNodes[i]);

                SubCalendarEvent retrievedSubEvent = new SubCalendarEvent(ID, BusySlot, Start, End, PrepTime, MyParent.ID, rigidFlag, Enabled, UiData, noteData, CompleteFlag, var1, MyParent.RangeTimeLine, conflictProfile);
                retrievedSubEvent.ThirdPartyID = MyXmlNode.ChildNodes[i].SelectSingleNode("ThirdPartyID").InnerText;//this is a hack to just update the Third partyID
                XmlNode restrictedNode = MyXmlNode.ChildNodes[i].SelectSingleNode("Restricted");
                retrievedSubEvent = new DB_SubCalendarEvent(retrievedSubEvent, MyParent.NowInfo, MyParent.ProcrastinationInfo);


                if (restrictedNode != null)
                {
                    if (Convert.ToBoolean(restrictedNode.InnerText))
                    { 
                        XmlNode RestrictionProfileNode =MyXmlNode.ChildNodes[i].SelectSingleNode("RestrictionProfile");
                        DB_RestrictionProfile myRestrictionProfile = (DB_RestrictionProfile)getRestrictionProfile(RestrictionProfileNode);
                        retrievedSubEvent = new DB_SubCalendarEventRestricted(retrievedSubEvent, myRestrictionProfile);
                    }
                }
                MyArrayOfNodes.Add(retrievedSubEvent);
            }

            return MyArrayOfNodes;
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
            List<Tuple<DayOfWeek, RestrictionTimeLine>> RestrictionTimeLines = new List<Tuple<DayOfWeek, RestrictionTimeLine>>();
            foreach (XmlNode eachXmlNode in RestrictionNode.SelectNodes("RestrictionNode"))
            {
                RestrictionTimeLines.Add(getgetRestrictionTuples(eachXmlNode));
            }

            DB_RestrictionProfile retValue = new DB_RestrictionProfile(RestrictionTimeLines);
            return retValue;
        }

        Tuple<DayOfWeek, RestrictionTimeLine> getgetRestrictionTuples(XmlNode RestrictionTupleNode)
        {
            DayOfWeek myDayOfWeek = getRestrictionDayOfWeek(RestrictionTupleNode);
            RestrictionTimeLine myRestrictionTimeLine = getRestrictionTimeLine(RestrictionTupleNode);
            Tuple<DayOfWeek, RestrictionTimeLine> retValue = new Tuple<DayOfWeek, RestrictionTimeLine>(myDayOfWeek, myRestrictionTimeLine);
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
        Location_Elements getLocation(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("Location");
            bool UninitializedLocation = false;
            return generateLocationObjectFromNode(var1);
        }

        Location_Elements generateLocationObjectFromNode(XmlNode var1)
        {
            bool UninitializedLocation = false;
            if (var1 == null)
            {
#if UseDefaultLocation
                return DefaultLocation;
#else
                return new Location_Elements();
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
                    return new Location_Elements(Address);
                }
                else
                {
                    double xCoOrdinate = double.MaxValue;
                    double yCoOrdinate = double.MaxValue;


                    int CheckDefault = Convert.ToInt32(CheckDefault_Str);

                    if (!(double.TryParse(XCoordinate_Str, out xCoOrdinate)))
                    {
                        xCoOrdinate = double.MaxValue;
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

                    return new Location_Elements(xCoOrdinate, yCoOrdinate, Address, Descripion, UninitializedLocation, CheckDefault);
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
            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.Now.AddYears(-1000), DateTimeOffset.Now.AddYears(1000));
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

            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.Now.AddYears(-1000), DateTimeOffset.Now.AddYears(1000));
            Dictionary<string, CalendarEvent> AllScheduleData = getAllCalendarFromXml(RangeOfLookup);
            
            Name = Name.ToLower();
            if (AllScheduleData.Count > 0)
            {
                retValue = AllScheduleData.Values.Where(obj => obj.Name.ToLower().Contains(Name)).ToList();
            }

            return retValue;
        }


        async public Task<IList<Location_Elements>> getCachedLocationByName(string Name)
        {
            
            IList<Location_Elements> retValue = new List<Location_Elements>();
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                retValue = myCassandraAccess.SearchLocationByName(Name);
                return retValue;
            }
#endif
            TimeLine RangeOfLookup = new TimeLine(DateTimeOffset.Now.AddYears(-1000), DateTimeOffset.Now.AddYears(1000));
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



        async public Task<Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>>> getProfileInfo(TimeLine RangeOfLookup = null)
        {
            //getLocationCache
            if (RangeOfLookup == null)
            {
                RangeOfLookup = new TimeLine(DateTimeOffset.Now.AddYears(-10), DateTimeOffset.Now.AddYears(10));
            }

            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>> retValue;
            if (this.Status)
            {
                Task<Dictionary<string, Location_Elements>> TaskLocationCache = getLocationCache();
                
                Dictionary<string, CalendarEvent> AllScheduleData = this.getAllCalendarFromXml(RangeOfLookup);
                Task<DateTimeOffset> getReferenceTime = getDayReferenceTime();


                Dictionary<string, Location_Elements> LocationCache = await TaskLocationCache.ConfigureAwait(false);
                await populateDefaultLocation(LocationCache).ConfigureAwait(false);
                DateTimeOffset ReferenceTime = await getReferenceTime.ConfigureAwait(false);
                
                retValue = new Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>>(AllScheduleData, ReferenceTime, LocationCache);
            }
            else
            {
                retValue = null;
            }
            return retValue;
        }


        

        async Task populateDefaultLocation(Dictionary<string, Location_Elements> locations = null)
        {
            double xLocation = 40.083319;
            double yLocation = -105.3505482;
            Location_Elements retValue;
            if ((locations == null))
            {
                
                retValue = new Location_Elements(xLocation, yLocation, -1);
                DefaultLocation = retValue;
                locations = await getLocationCache().ConfigureAwait(false);
                return;
            }

            if ((locations.Count < 1))
            {
                retValue = new Location_Elements(xLocation, yLocation, -1);
                DefaultLocation = retValue;
                locations = await getLocationCache().ConfigureAwait(false);
                return;
            }

            xLocation = locations.Values.Average(obj => obj.XCoordinate);
            yLocation = locations.Values.Average(obj => obj.YCoordinate);

            retValue = new Location_Elements(xLocation, yLocation, -1);
            Location_Elements.InitializeDefaultLongLat(xLocation, yLocation);
            DefaultLocation = retValue;
            
        }

        #region Cassandra Functions

        void TransferXmlFileToCassandra()
        { 
        
        }
        
        #endregion



        #endregion

        #region Properties

        public int LastUserID
        {
            get
            {
                return Convert.ToInt32(LastIDNumber);
            }
        }

        public bool Status
        {
            get
            {
                return LogStatus;
            }
        }

        public string LoggedUserID
        {
            get
            {
                return ID;
            }
        }


        public string getFullLogDir
        {
            get
            {
                return WagTapLogLocation + CurrentLog;
            }

        }

        public Location_Elements defaultLocation
        {
            get
            {
                return DefaultLocation;
            }
        }

        public string Usersname
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
