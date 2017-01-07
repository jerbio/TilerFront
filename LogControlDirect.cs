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
using System.Web;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using TilerElements;
using TilerFront.Models;
#if ForceReadFromXml
#else
using CassandraUserLog;
#endif



namespace TilerFront
{
    public class LogControlDirect:LogControl
    {
        Tuple<bool, string, DateTimeOffset, long> ScheduleMetadata;
        bool forcedLogin = false;
        public LogControlDirect()
        {
            ScheduleMetadata = new Tuple<bool, string, DateTimeOffset, long>(false, "", new DateTimeOffset(), 0);
            //useCassandra=false;
//            SessionUser= new TilerUser();
        }
        public LogControlDirect(TilerUser User, ApplicationDbContext database, string logLocation="")
        {
            if (!string.IsNullOrEmpty(logLocation))
            {
                WagTapLogLocation = logLocation;
            }
            _TilerUser = User;
            LogStatus = false;
            CachedLocation = new Dictionary<string, Location_Elements>();
            Database = database;
            CurrentLog = _TilerUser.Id.ToString() + ".xml";
            LogStatus = true;
            ID = _TilerUser.Id;
            UserName = _TilerUser.UserName;
                
            
        }

        public async Task<TilerUser> forceLogin()
        {
            TilerUser retValue = null;
            
            
            if (_TilerUser != null)
            {
                HttpContext myContext = HttpContext.Current;
                ApplicationUserManager UserManager = myContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                try
                {
                    retValue = await UserManager.FindByIdAsync(_TilerUser.Id).ConfigureAwait(false);
                    forcedLogin = true;
                }
                catch (Exception e)
                {
                    ;
                }
                
            }
            return retValue;
        }



        #region Functions
        override async public Task Initialize()
        {
            {

                await base.Initialize().ConfigureAwait(false);
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
            await Initialize().ConfigureAwait(false);
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

        async public override Task<DateTimeOffset> getDayReferenceTime(string NameOfFile = "")
        {
            if(!forcedLogin)
            {
                _TilerUser = await forceLogin().ConfigureAwait(false);
            }
            DateTimeOffset retValue = _TilerUser.LastChange;
            //retValue = new DateTimeOffset(2015, 4, 5, 22, 0, 0, new TimeSpan());
            return retValue;
        }

        async public  Task<DateTimeOffset> getDayReferenceTimeFromXml(string NameOfFile = "")
        {
            return await base.getDayReferenceTime(NameOfFile);
        }

        /// <summary>
        /// Function does nothing because updating start of day gets triggered in the manage controller
        /// </summary>
        /// <param name="referenceDay"></param>
        /// <param name="LogFile"></param>
        override public void UpdateReferenceDayInXMLLog(DateTimeOffset referenceDay, string LogFile = "")
        {
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                return;
            }
#endif
            /*
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
            xmldoc.Save(LogFile);*/
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
//        async public Task<bool> WriteToLog(IEnumerable<CalendarEvent> AllEvents, string LatestID, string LogFile = "")
//        {
//            Task<bool>  retValue;
            
//#if ForceReadFromXml
//#else
//            if (useCassandra)
//            {
//                retValue =  myCassandraAccess.Commit(AllEvents);
//                Task<bool> WritingLatestData =LogDBDataAccess.WriteLatestData(DateTime.Now,Convert.ToInt64( LatestID), ID);
                
//                bool LatestDataSuccess = await WritingLatestData;
//                bool boolRetValue =await retValue;
//                return boolRetValue;
//            }
//#endif



//            retValue = new Task<bool>(() => { return true; });
//            retValue.Start();
//            if (LogFile == "")
//            { LogFile = WagTapLogLocation + CurrentLog; }
//            XmlDocument xmldoc = new XmlDocument();
//            xmldoc.Load(LogFile);
//            CachedLocation = await getLocationCache();//populates with current location info
//            Dictionary<string, Location_Elements> OldLocationCache = new Dictionary<string, Location_Elements>(CachedLocation);
//            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = LatestID;
//            XmlNodeList EventSchedulesNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules");

//            XmlNode EventSchedulesNodesNode = xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
//            EventSchedulesNodesNode.RemoveAll();
//            XmlNodeList EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");


//            EventScheduleNodes = xmldoc.DocumentElement.SelectNodes("/ScheduleLog/EventSchedules/EventSchedule");

//            foreach (CalendarEvent MyEvent in AllEvents)
//            {
//                XmlElement EventScheduleNode = CreateEventScheduleNode(MyEvent);
//                //EventSchedulesNodes[0].PrependChild(xmldoc.CreateElement("EventSchedule"));
//                //EventSchedulesNodes[0].ChildNodes[0].InnerXml = CreateEventScheduleNode(MyEvent).InnerXml;
//                XmlNode MyImportedNode = xmldoc.ImportNode(EventScheduleNode as XmlNode, true);
//                //(EventScheduleNode, true);
//                if (!UpdateInnerXml(ref EventScheduleNodes, "ID", MyEvent.ID.ToString(), EventScheduleNode))
//                {
//                    xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules").AppendChild(MyImportedNode);
//                }
//                else
//                {
//                    ;
//                }
//            }

//            UpdateCacheLocation(xmldoc, OldLocationCache, NewLocation);

//            while (true)
//            {
//                try
//                {
//                    xmldoc.Save(LogFile);
//                    break;
//                }
//                catch (Exception e)
//                {
//                    Thread.Sleep(160);
//                }
//            }
//            return await retValue;
//        }

        



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
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.End.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PrepTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Preparation.ToString();
            //MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadlineFlag"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Pre.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("PreDeadline"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.PreDeadline.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("StartTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Start.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Name.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getId;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEnabled.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.myLocation, "EventScheduleLocation").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MyEvent.UIParam, "UIParams").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MyEvent.Notes, "MiscData").InnerXml;



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

            File.WriteAllText(dirString, "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>0</LastIDCounter><referenceDay>12:00 AM</referenceDay><EventSchedules></EventSchedules></ScheduleLog>");
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

        /*
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

        override public DateTimeOffset getDayReferenceTime(string NameOfFile = "")
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
            DateTimeOffset retValue = DateTimeOffset.Parse(node.InnerText);

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
        
        async override private Task<Dictionary<string, Location_Elements>> getLocationCache(string NameOfFile = "")
        {
            Dictionary<string, Location_Elements> retValue = new Dictionary<string, Location_Elements>();
#if ForceReadFromXml
#else
            if (useCassandra)
            {
                retValue = await myCassandraAccess.getAllCachedLocations();
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
            LastIDNumber = Convert.ToInt64(LastUsedIndex);
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

            foreach (XmlNode EventScheduleNode in EventSchedulesNodes.ChildNodes)
            {
                CalendarEvent RetrievedEvent;
                RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode, RangeOfLookUP);
                if (RetrievedEvent != null)
                { MyCalendarEventDictionary.Add(RetrievedEvent.Calendar_EventID.getCalendarEventComponent(), RetrievedEvent); }
            }

            return MyCalendarEventDictionary;
        }
         */
        
        

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

                    RetValue = new Repetition(true, new TimeLine(DateTimeOffset.Parse(RepeatStart), DateTimeOffset.Parse(RepeatEnd)), RepeatFrequency, repetitionNodes.ToArray(), repetitionDay);
                    return RetValue;
                }

            }



            RetValue = new Repetition(true, new TimeLine(DateTimeOffset.Parse(RepeatStart), DateTimeOffset.Parse(RepeatEnd)), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList, RangeOfLookUP), repetitionDay);

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
                retValue = AllScheduleData.Values.Where(obj => obj.Name.NameValue.ToLower().Contains(Name)).ToList();
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
            CachedLocation =await getLocationCache();
            

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
            MyDateTime = DateTimeOffset.Parse(MyDateTimeString);


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



        /*
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
                DateTimeOffset ReferenceTime = getDayReferenceTime();
                Dictionary<string, Location_Elements>  LocationCache = await TaskLocationCache;
                populateDefaultLocation(LocationCache);
                retValue = new Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, Location_Elements>>(AllScheduleData, ReferenceTime, LocationCache);
            }
            else
            {
                retValue = null;
            }
            return retValue;
        }
        */

        


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

        public override bool Status
        {
            get
            {
                return LogStatus && _TilerUser != null;
            }
        }

        public override string  LoggedUserID
        {
            get
            {
                return _TilerUser.Id;
            }
        }


        override public string getFullLogDir
        {
            get
            {
                return WagTapLogLocation + CurrentLog;
            }

        }

        override public Location_Elements defaultLocation
        {
            get
            {
                return DefaultLocation;
            }
        }

        override public string Usersname
        {
            get
            {
                return _TilerUser.FullName;
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
