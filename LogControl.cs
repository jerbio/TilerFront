//#define UseDefaultLocation
#define liveDebugging

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
using System.Data.Entity.Core.Objects;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TilerFront
{
    public class LogControl
    {
        protected TilerDbContext _Context;
        protected string ID;
        protected string UserName;
        string NameOfUser;
        protected static string BigDataLogLocation = "BigDataLogs\\";
        protected static string LogLocation = "BigDataLogs\\";
        protected bool LogStatus;
        protected bool UpdateLocaitionCache = false;
        protected Dictionary<string, TilerElements.Location> CachedLocation;
        protected TilerElements.Location DefaultLocation = new TilerElements.Location();
        protected TilerElements.Location NewLocation;
        protected DB_UserActivity activity;
        Dictionary<string, Func<XmlNode, Reason>> createDictionaryOfOPtionToFunction;
        Dictionary<string, CalendarEvent> _AllScheduleData;
        protected ScheduleDump _TempDump;
        protected TilerUser _TilerUser;
        protected bool _UpdateBigData = true;
        Tuple<bool, string, DateTimeOffset, long> ScheduleMetadata;

        protected LogControl()
        {
            ID = "";
            UserName = "";
            NameOfUser = "";
            LogStatus = false;
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

        public LogControl(TilerUser user, ApplicationDbContext database, DB_UserActivity useractivity = null):this()
        {
            LogStatus = false;
            CachedLocation = new Dictionary<string, TilerElements.Location>();
            _TilerUser = user;
            _Context = database;
        }
        #region Functions
        public static void UpdateBigDataLogLocation(string bigLogLocation)
        {
            BigDataLogLocation = bigLogLocation;
        }

        public static void UpdateLogLocation(string logLocation)
        {
            LogLocation = logLocation;
        }

        public static string getLogLocation()
        {
            return LogLocation;
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
            throw new NotImplementedException("Undo has not been fully implemented for RDBMS");
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
        public virtual async Task updateBigData(XmlDocument oldData, XmlDocument newData)
        {
            if(_UpdateBigData)
            {
                bool corruptZipFile = false;
                string zipFile = LoggedUserID + ".zip";
                string zipFolder = LoggedUserID;
                string fullZipPath = @BigDataLogLocation + zipFile;
                try
                {
                    BigDataLogControl bigdataControl = new BigDataLogControl();
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
                    if (corruptZipFile)
                    {
                        File.Delete(fullZipPath);
                    }
                    CustomErrors retValue = new CustomErrors("Error generating bigdata log\n" + e.ToString(), 20000000);
                }
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

        async public Task<ScheduleDump> CreateScheduleDump(IEnumerable<CalendarEvent> AllEvents, TilerUser user, ReferenceNow now, string notes, Dictionary<string, TilerElements.Location> cachedLocation = null)
        {
            Task<ScheduleDump> retValue;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.InnerXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ScheduleLog><LastIDCounter>1024</LastIDCounter><referenceDay>" 
                                    + now.EndOfDay.DateTime + 
                                "</referenceDay>"
                                +"<scheduleNotes>" 
                                    + notes 
                                + "</scheduleNotes>"
                                +"<lastUpdated>" 
                                    + user.LastScheduleModification.DateTime
                                + "</lastUpdated>"
                                +"<constNowTime>"
                                    + now?.constNow.ToString()
                                + "</constNowTime>"
                                + "<dateTimeOffsetUtcNow>"
                                    + DateTimeOffset.UtcNow.ToString().ToString()
                                + "</dateTimeOffsetUtcNow>"
                                + "<EventSchedules></EventSchedules></ScheduleLog>";

            CachedLocation = cachedLocation ?? await getAllLocationsByUser().ConfigureAwait(false); ;//populates with current location info
            Dictionary<string, TilerElements.Location> OldLocationCache = new Dictionary<string, TilerElements.Location>(CachedLocation);
            xmldoc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter").InnerText = "false";
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
            catch (Exception e)
            {
                EventSchedulesNodesNode.InnerXml = EventSchedulesNodesNodeCpy.InnerXml;
                errorWritingFile = true;
            }

            UpdateCacheLocation(xmldoc, OldLocationCache, NewLocation);
            ScheduleDump scheduleDump = new ScheduleDump()
            {
                UserId = user.Id,
                ScheduleXmlString = xmldoc.InnerXml,
                ReferenceNow = now.constNow,
                StartOfDay = now.EndOfDay
            };
            retValue = new Task<ScheduleDump>(() => { return scheduleDump; });
            retValue.Start();
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


        public void UpdateCacheLocation(XmlDocument xmldoc, Dictionary<string, TilerElements.Location> currentCache, TilerElements.Location NewLocation)
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

                string LocationID = eachKeyValuePair.Value.Id;
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


        protected virtual async Task Commit(IEnumerable<CalendarEvent> calendarEvents, TilerUser tilerUser, TravelCache travelCache)
        {
#if liveDebugging
            Debug.WriteLine("********************************Check directive of #liveDebugging if test is failing********************************");
            return;
#else
            IEnumerable<SubCalendarEvent> subevents = calendarEvents.SelectMany(calEVent => calEVent.RemoveSubEventFromEntity);
            foreach (SubCalendarEvent subEvent in subevents)
            {
                _Context.Entry(subEvent).State = EntityState.Deleted;
            }

            if (travelCache!=null)
            {
                foreach (var cacheEntry in travelCache.purgedLocations)
                {
                    _Context.Entry(cacheEntry).State = EntityState.Deleted;
                }
            }


            Task saveDbChangesTask = SaveDBChanges();
            if (_TempDump != null && _UpdateBigData)
            {
                ReferenceNow now = new ReferenceNow(_TempDump.ReferenceNow, _TempDump.StartOfDay, _TilerUser.TimeZoneDifference);
                Task<ScheduleDump> scheduleDumpCreationTask = CreateScheduleDump(calendarEvents, _TilerUser, now, "", CachedLocation);
                await scheduleDumpCreationTask.ConfigureAwait(false);
                ScheduleDump scheduleDump = scheduleDumpCreationTask.Result;

                Task updateBigDataTask = updateBigData(_TempDump.XmlDoc, scheduleDump.XmlDoc);
                await updateBigDataTask.ConfigureAwait(false);
            }

            await saveDbChangesTask.ConfigureAwait(false);
#endif
        }

        public async Task SaveDBChanges ()
        {
            Task saveDbChangesTask = _Context.SaveChangesAsync();
            await saveDbChangesTask.ConfigureAwait(false);
        }

        public async Task Commit(IEnumerable<CalendarEvent> calendarEvents, CalendarEvent calendarEvent, String LatestId, ReferenceNow now, TravelCache travelCache)
        {
            _TilerUser.LatestId = LatestId;
            _TilerUser.LastScheduleModification = now.constNow;
            if (calendarEvent != null)
            {
                _Context.CalEvents.Add(calendarEvent);
            }


            await Commit(calendarEvents, _TilerUser, travelCache).ConfigureAwait(false);
        }

        public async Task DiscardChanges()
        {
            var changedEntries = _Context.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }

        public async Task UpdateLocationCache(XmlDocument xmldoc, Dictionary<string, TilerElements.Location> currentCache, TilerElements.Location NewLocation)
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
                        string LocationID = eachKeyValuePair.Value.Id;
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
                            await AddNewLocation(NewLocation).ConfigureAwait(false);
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

        virtual public async Task AddNewLocation(TilerElements.Location Location)
        {
            _Context.Locations.Add(Location);
        }

        virtual public async Task updateLocationNode(TilerElements.Location Location)
        {
            TilerElements.Location location = await _Context.Locations.FindAsync(Location.Id).ConfigureAwait(false);
            location.update(Location);
        }

        virtual protected XmlNode getLocationNodeByTagName(string TagName, XmlDocument DocNode = null)
        {
            TagName = TagName.Trim().ToLower();
            XmlNode retValue = null;
            XmlDocument doc = DocNode;

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
                if ((desciption == TagName) || (CachedName == TagName))
                {
                    retValue = eachXmlNode;
                    break;
                }
            }
            return retValue;
        }


        virtual public XmlNode updateScheduleLocationNode(TilerElements.Location Location, XmlDocument DocNode = null)
        {
            XmlNode OldNode = getLocationNodeByTagName(Location.Description, DocNode);
            TilerElements.Location OldLocation;
            if (OldNode != null)
            {
                OldLocation = getLocation(OldNode);
            }
            else
            {
                OldLocation = new TilerElements.Location();
            }


            XmlElement newNode = CreateLocationNode(Location);
            if (isLocationIsDifferent(OldLocation, Location))
            {
                OldNode.InnerXml = newNode.InnerXml;
                XmlNode LocationIDNode = DocNode.CreateElement("LocationID");
                XmlNode CacheNameNode = DocNode.CreateElement("CachedName");
                CacheNameNode.InnerText = Location.Description.ToLower();
                LocationIDNode.InnerText = Location.Id;
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
            PreferredStart.InnerText = myNowProfile?.PreferredTime.ToString();
            InitializedNode.InnerText = myNowProfile?.isInitialized.ToString();
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
            foreach (TilerUser user in usergroup.users)
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
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.IsFromRecurringAndNotChildRepeatCalEvent.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("IsRepeatsChildCalEvent"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.IsRepeatsChildCalEvent.ToString();

            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("EventSubSchedules"));
            //MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Repetition.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RigidFlag"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isRigid.ToString();
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
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getName?.NameValue.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("NameId"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getName?.NameId.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getId;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.isEnabled.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MyEvent.LocationObj, "EventScheduleLocation").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UIParams"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createDisplayUINode(MyEvent.getUIParam, "UIParams").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("MiscData"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = createMiscDataNode(MyEvent.Notes, "MiscData").InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Restricted"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getIsEventRestricted.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("NowProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateNowProfileNode(MyEvent.getNowInfo).InnerXml);
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ProcrastinationProfile"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = (generateProcrastinationNode(MyEvent.getProcrastinationInfo)?.InnerXml);
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("isProcrastinateEvent"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getIsProcrastinateCalendarEvent.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("TimeZone"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.getTimeZone;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("TimeCreated"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.TimeCreated.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("ThirdpartyType"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.ThirdpartyType.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("EventPreference"));
            MyEventScheduleNode.ChildNodes[0].InnerXml = CreateEventPreference(MyEvent.DayPreference)?.InnerXml;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("LastCompletionTimes"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.LastCompletionTime_DB;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("Access_DB"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.Access_DB;
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("IniStartTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.InitialStartTime_DB.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("IniEndTime"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.InitialEndTime_DB.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("DeadlineSuggestion"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.DeadlineSuggestion_DB.ToString();
            MyEventScheduleNode.PrependChild(xmldoc.CreateElement("LocationValidationId"));
            MyEventScheduleNode.ChildNodes[0].InnerText = MyEvent.LocationValidationId;
            if (!MyEvent.isThirdParty)
            {
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("UpdateHistory"));
                MyEventScheduleNode.ChildNodes[0].InnerText = CreateUpdateHistory(MyEvent.TimeLineHistory)?.InnerXml;
            }
            

            if (MyEvent.getIsEventRestricted)
            {
                CalendarEventRestricted restrictedMyEvent = (CalendarEventRestricted)MyEvent;
                XmlElement restrictionProfileData = generateXMLRestrictionProfile(restrictedMyEvent.RestrictionProfile);
                MyEventScheduleNode.PrependChild(xmldoc.CreateElement("RestrictionProfile"));
                MyEventScheduleNode.ChildNodes[0].InnerXml = restrictionProfileData.InnerXml;
            }


            if (MyEvent.IsFromRecurringAndNotChildRepeatCalEvent && MyEvent.isRepeatLoaded)
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
            if(ProcrastinationData!=null)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement ProcrastinationProfileNode = xmldoc.CreateElement("ProcrastinationProfile");
                XmlElement ProcrastinationPreferredStart = xmldoc.CreateElement("ProcrastinationPreferredStart");
                ProcrastinationPreferredStart.InnerText = ProcrastinationData?.PreferredStartTime.ToString();
                XmlElement ProcrastinationDislikedStartNode = xmldoc.CreateElement("ProcrastinationDislikedStart");
                ProcrastinationDislikedStartNode.InnerText = ProcrastinationData?.DislikedStartTime.ToString();
                XmlElement ProcrastinationDislikedDaySectionNode = xmldoc.CreateElement("ProcrastinationDislikedDaySection");
                ProcrastinationDislikedDaySectionNode.InnerText = ProcrastinationData?.DislikedDaySection.ToString();
                ProcrastinationProfileNode.AppendChild(ProcrastinationPreferredStart);
                ProcrastinationProfileNode.AppendChild(ProcrastinationDislikedStartNode);
                ProcrastinationProfileNode.AppendChild(ProcrastinationDislikedDaySectionNode);
                return ProcrastinationProfileNode;
            } else
            {
                return null;
            }
            
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
                RestrictionDayOfWeekNode.InnerText = eachTuple.DayOfWeekString;
                XmlElement RestrictionTimeLineNode = xmldoc.CreateElement("RestrictionTimeLineData");
                RestrictionTimeLineNode.InnerXml = generateRestrictionTimeLineNode(eachTuple.RestrictionTimeLine).InnerXml;
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
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isRigid.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ID"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getId;
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Enabled"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isEnabled.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Complete"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getIsComplete.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Name"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getName?.NameValue?.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("NameId"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.getName?.NameId.ToString();

            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("Location"));
            MyEventSubScheduleNode.ChildNodes[0].InnerXml = CreateLocationNode(MySubEvent.LocationObj, "EventSubScheduleLocation").InnerXml;
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
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TimeCreated"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TimeCreated.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TravelTimeAfter"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TravelTimeAfter.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("TravelTimeBefore"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.TravelTimeBefore.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isWake"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isWake.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isSleep"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isSleep.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("ThirdpartyType"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.ThirdpartyType.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("AutoDeleted"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.AutoDeleted_EventDB.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("AutoDeletionReason"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.AutoDeletion_ReasonDB.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("RepetitionLock"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.RepetitionLock_DB.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("NowLock"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.NowLock_DB.ToString();
            
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("isTardy"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.isTardy.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("IniStartTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.InitialStartTime_DB.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("IniEndTime"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.InitialEndTime_DB.ToString();
            MyEventSubScheduleNode.PrependChild(xmldoc.CreateElement("LocationValidationId"));
            MyEventSubScheduleNode.ChildNodes[0].InnerText = MySubEvent.LocationValidationId;


            if (MySubEvent.getIsEventRestricted)
            {
                restrictedMySub = (SubCalendarEventRestricted)MySubEvent;
                XmlElement restrictionProfileData = generateXMLRestrictionProfile(restrictedMySub.RestrictionProfile);
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
            //XmlElement PauseTime = xmldoc.CreateElement("PauseTime");
            XmlElement pausedTimeSlots = xmldoc.CreateElement("PausedTimeSlots");
            XmlElement RetValue = xmldoc.CreateElement("PauseInformation");
            UsedUpTime.InnerText = SubEvent.UsedPauseTime.ToString();

            List<XmlElement> timeLineNodes = (SubEvent.pausedTimeLines?? new List<PausedTimeLine>()).Select(timeline => TimelineToXmlNode(timeline, xmldoc)).ToList();
            //PauseTime.InnerText = SubEvent.getPauseTime().ToString();
            foreach(XmlElement timeLineNode in timeLineNodes)
            {
                pausedTimeSlots.AppendChild(timeLineNode);
            }
            RetValue.AppendChild(UsedUpTime);
            RetValue.AppendChild(pausedTimeSlots);
            return RetValue;
        }
        public XmlElement ReasonForPosition(List<Reason> reasons, string ElementIdentifier = "TimePositionReasons")
        {
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

        public XmlElement PausedTimelineToXmlNode(PausedTimeLine pausedTimeLine, XmlDocument xmldoc)
        {
            XmlElement PausedTimelineNode = xmldoc.CreateElement("PausedTimeline");
            XmlElement startNode = xmldoc.CreateElement("Start");
            XmlElement endNode = xmldoc.CreateElement("End");
            XmlElement idNode = xmldoc.CreateElement("Id");
            XmlElement isDeletedNode = xmldoc.CreateElement("IsDeleted");
            idNode.InnerText = pausedTimeLine.Id;
            startNode.InnerText = pausedTimeLine.Start.ToString();
            endNode.InnerText = pausedTimeLine.End.ToString();
            isDeletedNode.InnerText = pausedTimeLine.IsDeleted.ToString();
            PausedTimelineNode.AppendChild(startNode);
            PausedTimelineNode.AppendChild(endNode);
            PausedTimelineNode.AppendChild(idNode);
            PausedTimelineNode.AppendChild(isDeletedNode);
            return PausedTimelineNode;
        }


        public XmlElement BusyTimelineToXmlNode(BusyTimeLine busyTimeLine, XmlDocument xmldoc)
        {
            XmlElement timeLineNode = xmldoc.CreateElement("BusyTimeLine");
            XmlElement startNode = xmldoc.CreateElement("Start");
            XmlElement endNode = xmldoc.CreateElement("End");
            XmlElement idNode = xmldoc.CreateElement("Id");
            idNode.InnerText = busyTimeLine.Id;
            startNode.InnerText = busyTimeLine.Start.ToString();
            endNode.InnerText = busyTimeLine.End.ToString();
            timeLineNode.AppendChild(startNode);
            timeLineNode.AppendChild(endNode);
            timeLineNode.AppendChild(idNode);
            return timeLineNode;
        }


        public XmlElement TimelineToXmlNode(TimeLine timeLine, XmlDocument xmldoc)
        {
            XmlElement timeLineNode = xmldoc.CreateElement("TimeLine");
            XmlElement busyTimeSlots = xmldoc.CreateElement("BusyTimeLines");
            XmlElement startNode = xmldoc.CreateElement("Start");
            XmlElement endNode = xmldoc.CreateElement("End");
            startNode.InnerText = timeLine.Start.ToString();
            endNode.InnerText = timeLine.End.ToString();
            timeLineNode.AppendChild(startNode);
            timeLineNode.AppendChild(endNode);
            

            List<XmlElement> busyTimeLineNodes = timeLine.OccupiedSlots.Select(timeline => BusyTimelineToXmlNode(timeline, xmldoc)).ToList();
            foreach (XmlElement eachBusyTimeLineNode in busyTimeLineNodes)
            {
                busyTimeSlots.AppendChild(eachBusyTimeLineNode);
            }
            timeLineNode.AppendChild(busyTimeSlots);

            return timeLineNode;
        }

        public XmlElement CreateEventPreference(EventPreference preference, string ElementIdentifier = "EventPreference")
        {
            if(preference!=null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EventPreference));
                XmlDocument xmldoc = new XmlDocument();
                XmlElement retValue = xmldoc.CreateElement(ElementIdentifier);
                using (XmlWriter writer = xmldoc.CreateNavigator().AppendChild())
                {
                    serializer.Serialize(writer, preference);
                }
                retValue.InnerXml = xmldoc.FirstChild.InnerXml;
                return retValue;
            } else
            {
                return null;
            }
        }

        public XmlElement CreateUpdateHistory(TimeLineHistory updateHistory, string ElementIdentifier = "UpdateHistory")
        {
            if (updateHistory != null)
            {
                var updateHistory_copy = updateHistory.CreateCopy();
                XmlSerializer serializer = new XmlSerializer(typeof(TimeLineHistory));
                XmlDocument xmldoc = new XmlDocument();
                XmlElement retValue = xmldoc.CreateElement(ElementIdentifier);
                using (XmlWriter writer = xmldoc.CreateNavigator().AppendChild())
                {
                    serializer.Serialize(writer, updateHistory_copy);
                }
                retValue.InnerXml = xmldoc.FirstChild.InnerXml;
                return retValue;
            }
            else
            {
                return null;
            }
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
            if ((Arg1 != null))//&& (!Arg1.isNull))
            {
                XCoordinate = Arg1.Latitude.ToString();
                YCoordinate = Arg1.Longitude.ToString();
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
            var1.PrependChild(xmldoc.CreateElement("Id"));
            var1.ChildNodes[0].InnerText = Arg1.Id;
            var1.PrependChild(xmldoc.CreateElement("UserId"));
            var1.ChildNodes[0].InnerText = Arg1.UserId;
            var1.PrependChild(xmldoc.CreateElement("LocationValidation"));
            var1.ChildNodes[0].InnerText = Arg1.LocationValidation_DB;
            var1.PrependChild(xmldoc.CreateElement("LookupString"));
            var1.ChildNodes[0].InnerText = Arg1.LookupString;
            var1.PrependChild(xmldoc.CreateElement("IsDefault"));
            var1.ChildNodes[0].InnerText = Arg1.isDefault.ToString();
            var1.PrependChild(xmldoc.CreateElement("IsVerified"));
            var1.ChildNodes[0].InnerText = Arg1.IsVerified.ToString();
            var1.PrependChild(xmldoc.CreateElement("ThirdPartyId"));
            var1.ChildNodes[0].InnerText = Arg1.ThirdPartyId_DB;
            var1.PrependChild(xmldoc.CreateElement("ThirdPartySource"));
            var1.ChildNodes[0].InnerText = Arg1.ThirdPartySource;

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
            var1.PrependChild(xmldoc.CreateElement("Color"));
            if (Arg1 == null)
            {
                Arg1 = new EventDisplay();
            }
            var1.ChildNodes[0].InnerXml = createColorNode(Arg1.UIColor, "Color").InnerXml;
            var1.PrependChild(xmldoc.CreateElement("Type"));
            var1.ChildNodes[0].InnerText = Arg1.Default.ToString();
            var1.PrependChild(xmldoc.CreateElement("Visible"));
            var1.ChildNodes[0].InnerText = Arg1.Visible.ToString();
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
            var1.ChildNodes[0].InnerText = Arg1?.UserNote;
            var1.PrependChild(xmldoc.CreateElement("TypeSelection"));
            var1.ChildNodes[0].InnerText = Arg1?.TypeSelection.ToString();
            return var1;
        }
        public void EmptyCalendarXMLFile(string dirString = "")
        {

        }

        public virtual void deleteAllCalendarEvents(string dirString = "")
        {
            _Context.CalEvents.Where(calEvent => calEvent.CreatorId == _TilerUser.Id)
                .ForEachAsync(calEvent => {
                    calEvent.Disable(this.Now, false);
                });
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
            if (userNode != null)
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

        /// <summary>
        /// gets the Schedule dump by the dump id
        /// </summary>
        /// <param name="dumpId"></param>
        /// <returns></returns>
        public async Task<ScheduleDump> GetScheduleDump(string dumpId)
        {
            ScheduleDump retValue = await Database.ScheduleDumps.FindAsync(dumpId).ConfigureAwait(false);
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
        virtual public DateTimeOffset getDayReferenceTime()
        {
            TilerUser user = _TilerUser;
            if (user == null)
            {
                user = _Context.Users.Find(ID);
                if (user == null)
                {
                    throw new NullReferenceException("Cannot find user with ID " + ID);
                }
            }
            DateTimeOffset retValue = user.EndfOfDay;
            return retValue;
        }

        virtual protected IQueryable<TilerElements.Location> getLocationsByUserId(string userId)
        {
            IQueryable<TilerElements.Location> retValue = _Context.Locations.Where(location => location.UserId == userId);
            return retValue;
        }

        virtual public IQueryable<TilerElements.Location> getLocationsByDescription(string description)
        {
            IQueryable<TilerElements.Location> retValue = getLocationsByUserId(_TilerUser.Id).Where(location => location.SearchdDescription.Contains(description));
            return retValue;
        }

        virtual public IQueryable<TilerElements.Location> getAllLocationsQuery()
        {
            return _Context.Locations;
        }

        async virtual protected Task<Dictionary<string, TilerElements.Location>> getAllLocationsByUser()
        {
            Dictionary<string, TilerElements.Location> retValue = await _Context.Locations.Where(location => location.UserId == _TilerUser.Id).ToDictionaryAsync(obj => obj.SearchdDescription.ToLower(), obj => obj).ConfigureAwait(false);
            return retValue;
        }

        public Dictionary<string, TilerElements.Location> getLocationCache(ScheduleDump scheduleDump)
        {
            Dictionary<string, TilerElements.Location> retValue = new Dictionary<string, TilerElements.Location>();


            XmlDocument doc = scheduleDump.XmlDoc;
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

        private IQueryable<Repetition> getRepeatCalendarEvent(
            bool includeRecurringEvents, 
            HashSet<string> ids = null
            )
        {
            IQueryable<Repetition> repetitions = _Context.Repetitions
                    .Include(repetition => repetition.SubRepetitions)
                    .Include(repetition => repetition.SubRepetitions.Select(subRepetition => subRepetition.SubRepetitions))
                    ;


            if (includeRecurringEvents)
            {
                repetitions = repetitions
                    .Include(repetition => repetition.RepeatingEvents);
            }

            if (ids!=null && ids.Count> 0)
            {
                repetitions = repetitions.Join(ids,
                    repetition => repetition.Id,
                    id => id,
                    (repetition, id) => new { repetition = repetition, id = id })
                    .Select(obj => obj.repetition);
            }

            return repetitions;
        }


        public IQueryable<CalendarEvent> getItAll()
        {
            IQueryable<CalendarEvent> calEVents = _Context.CalEvents;
            
            calEVents = calEVents
                .Include(calEvent => calEvent.DataBlob_EventDB)
                .Include(calEvent => calEvent.Name)
                .Include(calEvent => calEvent.Name.Creator_EventDB)
                .Include(calEvent => calEvent.Location_DB)
                .Include(calEvent => calEvent.Creator_EventDB)
                .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                .Include(calEvent => calEvent.AllSubEvents_DB)
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ParentCalendarEvent))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Creator_EventDB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.RestrictionProfile_DB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB))
                .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB));
            ;
            return calEVents;
        }



        public IQueryable<CalendarEvent> getCalendarEventQuery(
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> ids = null
            )
        {
            Tuple<IQueryable<CalendarEvent>, List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>>> queryTuple = getCalendarEventQueryList(retrievalOptions, ids);
            IQueryable<CalendarEvent> getCalendarEventQuery = queryTuple.Item1;
            IQueryable<CalendarEvent> revisedQuery = getCalendarEventQuery;
            foreach (var queryTask in queryTuple.Item2)
            {
                revisedQuery = queryTask(revisedQuery);
            }

            return revisedQuery;
        }

        public IQueryable<CalendarEvent> getCalendarEventQueryOld(
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> ids= null
            )
        {
            IQueryable<CalendarEvent> calEvents = _Context.CalEvents.Where(calEvent => calEvent.CreatorId == _TilerUser.Id);
            if (ids != null && ids.Count > 0)
            {
                calEvents = calEvents
                .Join(ids,
                calEvent => calEvent.Id,
                id => id,
                (calEvent, id) => new { calEvent = calEvent, id = id }).Select(obj => obj.calEvent);
            }

            bool isSubEventIncluded = retrievalOptions.Contains(DataRetrivalOption.SubEvent);
            bool isAllRetrievalOptionIncluded = retrievalOptions.Contains(DataRetrivalOption.All);
            if (isSubEventIncluded || isAllRetrievalOptionIncluded)
            {
                calEvents = calEvents
                    .Include(calEvent => calEvent.AllSubEvents_DB)
                ;
            }


            
            foreach (DataRetrivalOption eachRetrievalOption in retrievalOptions)
            {
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Ui)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.UiParams_EventDB)
                        .Include(calEvent => calEvent.UiParams_EventDB.UIColor);
                    if(isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB))
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB.UIColor))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.TimeLine)
                {
                    //calEvents = calEvents
                    //    .Include(calEvent => calEvent.StartTime_EventDB)
                    //    .Include(calEvent => calEvent.EndTime_EventDB)
                    //    ;
                    //if (isAllRetrievalOptionIncluded ||isSubEventIncluded)
                    //{
                    //    calEvents = calEvents
                    //        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.StartTime_EventDB))
                    //        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.EndTime_EventDB))
                    //        ;
                    //}
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Evaluation)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.RestrictionProfile_DB)
                        .Include(calEvent => calEvent.DayPreference_DB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.RestrictionProfile_DB))
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Now)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.DataBlob)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.DataBlob_EventDB)
                        ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Name)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.Name)
                        ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Location)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.Location_DB)
                        ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                            ;
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.TimeLineHistory)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.TimeLineHistory_DB)
                        ;
                }

                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Repetition)
                {
                    calEvents = calEvents
                        .Include(calEvent => calEvent.Repetition_EventDB)
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents)
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.SubRepetitions));
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB)));
                    }
                    


                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Evaluation)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.DayPreference_DB))
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB)));
                            if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                            {
                                calEvents = calEvents
                                    .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB)));
                            }
                    }



                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Now)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB)));
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            calEvents = calEvents
                                .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB)));
                        }
                    }
                        
                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Name)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Name.Creator_EventDB)));
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name)))
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB)))
                            ;
                        }
                    }


                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.DataBlob)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.DataBlob_EventDB)));
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB)));
                        }
                    }

                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Location)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Location_DB)));
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            calEvents = calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB)));
                        }
                    }
                        
                    ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        calEvents = calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Repetition_EventDB));
                    }


                    //IQueryable<Repetition> repetitions = _Context.Repetitions
                    //.Include(repetition => repetition.SubRepetitions)
                    //.Include(repetition => repetition.SubRepetitions.Select(subRepetition => subRepetition.SubRepetitions))
                    //;
                }
            }
            
            return calEvents;
        }

        private IQueryable<SubCalendarEvent> getSubCalendarEventQuery(
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> ids = null)
        {
            IQueryable<SubCalendarEvent> subEvents = _Context.SubEvents.Where(subEvent => subEvent.CreatorId == _TilerUser.Id);
            if (ids != null && ids.Count > 0)
            {
                subEvents = subEvents
                .Join(ids,
                subEvent => subEvent.Id,
                id => id,
                (subEvent, id) => new { subEvent = subEvent, id = id }).Select(obj => obj.subEvent);
            }


            bool isAllRetrievalOptionIncluded = retrievalOptions.Contains(DataRetrivalOption.All);
            foreach (DataRetrivalOption eachRetrievalOption in retrievalOptions)
            {
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Ui)
                {
                    subEvents = subEvents
                        .Include(subEvent => subEvent.ParentCalendarEvent.UiParams_EventDB)
                        .Include(subEvent => subEvent.ParentCalendarEvent.UiParams_EventDB.UIColor)
                        .Include(subEvent => subEvent.UiParams_EventDB)
                        .Include(subEvent => subEvent.UiParams_EventDB.UIColor)
                        ;

                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.TimeLine)
                {
                    //subEvents = subEvents
                    //    .Include(calEvent => calEvent.StartTime_EventDB)
                    //    .Include(calEvent => calEvent.EndTime_EventDB)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Evaluation)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.RestrictionProfile_DB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Now)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Repetition)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.Repetition_EventDB)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.DataBlob)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.DataBlob_EventDB)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Name)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.Name.NameValue)
                        ;
                }
                else if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Location)
                {
                    subEvents = subEvents
                        .Include(calEvent => calEvent.Location_DB)
                        ;
                }
            }

            return subEvents;
        }

        /// <summary>
        /// Function retrieves the travel cache of for a specific user
        /// </summary>
        /// <param name="tilerUser"></param>
        /// <returns></returns>
        async public virtual Task<TravelCache> getTravelCache(TilerUser tilerUser)
        {
            return await getTravelCache(tilerUser.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Function retrieves the travel cache of for a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        async public virtual Task<TravelCache> getTravelCache(string userId)
        {
            var retValueQuery = _Context.TravelCaches.Where(travel => travel.Id == userId);
            var retValue = await retValueQuery
                .Include(cache => cache.LocationCombo_DB)
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (retValue == null)
            {
                retValue = new TravelCache
                {
                    Id = userId
                };
                _Context.TravelCaches.Add(retValue);
                _Context.SaveChanges();
            }
            if(retValue.Id == null)
            {
                retValue.Id = userId;
            }
            return retValue;
        }


        /// <summary>
        ///  FUnction makes a storage call to retrieve all sub events. Note this includes both completed and deleted sub events. Except the case of count decrement
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public Tuple<IEnumerable<SubCalendarEvent>, Analysis> getSubCalendarEventForAnalysis(TimeLine timeline, TilerUser user)
        {
            if (timeline!=null && user!= null)
            {
                IQueryable<SubCalendarEvent> subEventQuery = null;
                IQueryable<CalendarEvent> caleventQuery = getCalendarEventQuery(DataRetrievalSet.analysisManipulation);
                caleventQuery = caleventQuery
                    .Where(calEvent =>
                        calEvent.CreatorId == user.Id
                        && calEvent.StartTime_EventDB < timeline.End
                        && calEvent.EndTime_EventDB > timeline.Start
                    )
                    .Include(calEvent => calEvent.TimeLineHistory_DB)
                    .Include(calEvent => calEvent.TimeLineHistory_DB.TimeLines_DB)
                    .Include(calEvent => calEvent.AllSubEvents_DB)
                    ;
                var calEvents = caleventQuery.ToList();
                Analysis analysis = _Context.Analysis.Find(user.Id);
                Tuple<IEnumerable<SubCalendarEvent>, Analysis> retValue = new Tuple<IEnumerable<SubCalendarEvent>, Analysis>(new HashSet<SubCalendarEvent>(calEvents.SelectMany(calEvent => calEvent.AllSubEvents_DB).ToList()), analysis);
                return retValue;
            }
            
            if(timeline!=null)
            {
                throw new ArgumentNullException("timeline");
            }
            else
            {
                throw new ArgumentNullException("user");
            }
        }

        public virtual IQueryable<SubCalendarEvent> getAllSubCalendarEvents(TimeLine RangeOfLookUP, ReferenceNow Now, HashSet<DataRetrivalOption> retrievalOptions, bool includeOtherEntities = false) {
#if liveDebugging
            includeOtherEntities = true;
#else
#endif
            this.Now = Now;
            long rangeStart = RangeOfLookUP.Start.ToUnixTimeMilliseconds();
            long rangeEnd = RangeOfLookUP.End.ToUnixTimeMilliseconds();
            IQueryable<SubCalendarEvent> allSubCalQuery = getSubCalendarEventQuery(retrievalOptions);
            allSubCalQuery = allSubCalQuery
                .Where(subEvent =>
                        subEvent.StartTime_EventDB < RangeOfLookUP.End
                        && subEvent.EndTime_EventDB > RangeOfLookUP.Start
                        && subEvent.CreatorId == this._TilerUser.Id
                        );
            allSubCalQuery = allSubCalQuery
                .Include(subEvent => subEvent.ParentCalendarEvent)
                .Include(subEvent => subEvent.RepeatParentEvent)
                ;

            return allSubCalQuery;
        }

        public virtual IQueryable<CalendarEvent> addCalendarEventEvaluationRetrieval(IQueryable<CalendarEvent> calEventQuery)
        {
            var retValue = calEventQuery
                        .Include(calEvent => calEvent.RestrictionProfile_DB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        .Include(calEvent => calEvent.DayPreference_DB)
                        .Include(calEvent => calEvent.Location_DB)
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        ;
            return retValue;
        }

        public virtual IQueryable<T> addCalendarRepetitionRetrieval<T>(IQueryable<T> calEventQuery) where T : TilerEvent
        {
            var retValue = calEventQuery
                .Include(calEvent => calEvent.Repetition_EventDB);
            return retValue;
        }

        public virtual IQueryable<T> addCalendarDataBlobRetrieval<T>(IQueryable<T> calEventQuery) where T : TilerEvent
        {
            var retValue = calEventQuery
                .Include(calEvent => calEvent.DataBlob_EventDB);
            return retValue;
        }

        public virtual IQueryable<T> addCalendarNameRetrieval<T>(IQueryable<T> calEventQuery) where T : TilerEvent
        {
            var retValue = calEventQuery
                .Include(calEvent => calEvent.Name); ;
            return retValue;
        }

        public virtual IQueryable<T> addUiParamsRetrieval<T>(IQueryable<T> calEventQuery) where T : TilerEvent
        {
            var retValue = calEventQuery
                .Include(calEvent => calEvent.UiParams_EventDB)
                .Include(calEvent => calEvent.UiParams_EventDB.UIColor);
            return retValue;
        }







        async public virtual Task<IEnumerable<SubCalendarEvent>> getAllEnabledSubCalendarEvent(
            TimeLine RangeOfLookUP, 
            ReferenceNow Now,
            HashSet<DataRetrivalOption> retrievalOptions)
        {
            this.Now = Now;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            IQueryable<SubCalendarEvent> allSubCalQuery = getSubCalendarEventQuery(retrievalOptions);
            allSubCalQuery = allSubCalQuery
                .Where(subEvent =>
                        subEvent.IsEnabled_DB
                        && !subEvent.Complete_EventDB
                        && subEvent.StartTime_EventDB < RangeOfLookUP.End
                        && subEvent.EndTime_EventDB > RangeOfLookUP.Start
                        && subEvent.ParentCalendarEvent.IsEnabled_DB
                        && !subEvent.ParentCalendarEvent.Complete_EventDB
                        && subEvent.ParentCalendarEvent.StartTime_EventDB < RangeOfLookUP.End
                        && subEvent.ParentCalendarEvent.EndTime_EventDB > RangeOfLookUP.Start
                        && (
                                (subEvent.RepeatParentEventId == null) || 
                                (
                                    (
                                        (
                                            subEvent.RepeatParentEventId != null && subEvent.RepeatParentEvent.IsEnabled_DB
                                            && !subEvent.RepeatParentEvent.Complete_EventDB
                                            && subEvent.RepeatParentEvent.StartTime_EventDB < RangeOfLookUP.End
                                            && subEvent.RepeatParentEvent.EndTime_EventDB > RangeOfLookUP.Start
                                        ) &&
                                        (subEvent.RepeatParentEvent != null && subEvent.RepeatParentEvent.IsEnabled_DB)
                                    )
                                    && (
                                        (
                                            (subEvent.RepeatParentEvent.RepeatParentEventId == null) ||
                                            (
                                                (
                                                    subEvent.RepeatParentEvent.RepeatParentEventId != null && subEvent.RepeatParentEvent.RepeatParentEvent.IsEnabled_DB
                                                    && !subEvent.RepeatParentEvent.RepeatParentEvent.Complete_EventDB
                                                    && subEvent.RepeatParentEvent.RepeatParentEvent.StartTime_EventDB < RangeOfLookUP.End
                                                    && subEvent.RepeatParentEvent.RepeatParentEvent.EndTime_EventDB > RangeOfLookUP.Start
                                                ) &&
                                                (subEvent.RepeatParentEvent.RepeatParentEvent != null && subEvent.RepeatParentEvent.RepeatParentEvent.IsEnabled_DB)
                                            )
                                        )
                                    )
                                )
                            )

                        
                    );

            allSubCalQuery = allSubCalQuery.Where(calEvent => calEvent.CreatorId == _TilerUser.Id);
            var retValue = await allSubCalQuery
                .ToListAsync().ConfigureAwait(false);
            watch.Stop();
            TimeSpan webFrontEndSpan = watch.Elapsed;
            Debug.WriteLine("web Front End Span " + webFrontEndSpan.ToString());
            Debug.WriteLine("Bungled maybe");
            return retValue;
        }

        bool isRepeatitionLoaded(HashSet<DataRetrivalOption> dataRetrivalOptions)
        {
            bool retValue = dataRetrivalOptions.Contains(DataRetrivalOption.All) || dataRetrivalOptions.Contains(DataRetrivalOption.Repetition);
            return retValue;
        }

        /// <summary>
        /// This gets all cal events within <paramref name="RangeOfLookUP"/>. Note: This uses the subcalendar event as the bases for the query. So if a sub event is not within the rangelookup it won't pull the calendar event, this is for performance reasons. Hence why the general default is to be generous.  If you want to pull a specific calendar event then you need to include the calendar id as part of <paramref name="tilerIds"/>.
        /// <paramref name="retrievalOption"/> provides a way of stream lining whats pulled from the DB. Evaluation ignores the UI component and includes only data to compute the schedule. Information such as NowProfile, procrastination and location
        /// </summary>
        /// <param name="RangeOfLookUP"></param>
        /// <param name="Now"></param>
        /// <param name="includeSubEvents"></param>
        /// <param name="includeUpdateHistory"></param>
        /// <param name="retrievalOptions"></param>
        /// <param name="tilerIds"></param>
        /// <returns></returns>
        async public virtual Task<Dictionary<string, CalendarEvent>> getAllEnabledCalendarEvent(
            TimeLine RangeOfLookUP,
            ReferenceNow Now,
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> tilerIds = null)
        {
            if (retrievalOptions != null)
            {
                this.Now = Now;
                Stopwatch watch = new Stopwatch();
                watch.Start();
                CalendarEvent defaultCalEvent = CalendarEvent.getEmptyCalendarEvent(EventID.GenerateCalendarEvent(), Now.constNow, Now.constNow.AddHours(1));
                if (RangeOfLookUP != null)
                {
                    if (tilerIds == null)
                    {
                        tilerIds = new HashSet<string>();
                    }
                    EventID pausedEventId = _TilerUser.PausedEventId;
                    if(pausedEventId!=null)
                    {
                        tilerIds.Add(_TilerUser.PausedEventId.ToString());
                    }
                    var calIds = tilerIds.Select(o=> new EventID(o).getAllEventDictionaryLookup).ToArray();

                    HashSet<string> calIdSet = new HashSet<string>();
                    if(calIds.Length > 0)
                    {
                        calIdSet = new HashSet<string>(calIds);
                    }
                    var calEventQuery = getCalendarEventQuery(retrievalOptions);
                    
                    if(calIdSet.Count == 0)
                    {
                        calEventQuery = calEventQuery
                        .Where(calEvent =>
                                (
                                    calEvent.IsEnabled_DB
                                    && !calEvent.Complete_EventDB
                                    && (
                                        (calEvent.RepeatParentEvent == null)
                                        || (
                                            calEvent.RepeatParentEvent.IsEnabled_DB
                                            && !calEvent.RepeatParentEvent.Complete_EventDB
                                            )
                                        )
                                    && calEvent.StartTime_EventDB < RangeOfLookUP.End
                                    && calEvent.EndTime_EventDB > RangeOfLookUP.Start
                                    && calEvent.StartTime_EventDB < RangeOfLookUP.End// for performance reasons we will ignore sub events outside two weeks that are not within the range
                                    && calEvent.EndTime_EventDB > RangeOfLookUP.Start// for performance reasons we will ignore sub events outside two weeks that are not within the specified range so the range has to be generous
                                )
                            );
                    } else
                    {
                        calEventQuery = calEventQuery
                        .Where(calEvent =>
                                (
                                    calEvent.IsEnabled_DB
                                    && !calEvent.Complete_EventDB
                                    && (
                                        (calEvent.RepeatParentEvent == null)
                                        || (
                                            calEvent.RepeatParentEvent.IsEnabled_DB
                                            && !calEvent.RepeatParentEvent.Complete_EventDB
                                            )
                                        )
                                    && calEvent.StartTime_EventDB < RangeOfLookUP.End
                                    && calEvent.EndTime_EventDB > RangeOfLookUP.Start
                                    && calEvent.StartTime_EventDB < RangeOfLookUP.End// for performance reasons we will ignore sub events outside two weeks that are not within the range
                                    && calEvent.EndTime_EventDB > RangeOfLookUP.Start// for performance reasons we will ignore sub events outside two weeks that are not within the specified range so the range has to be generous
                                )
                                || calIdSet.Contains(calEvent.Id)
                            );
                    }

                    

                    #region includeCalendarEvents
                    //subCalendarEvents = subCalendarEvents
                    //    .Include(subEvent => subEvent.ParentCalendarEvent)
                    //    .Include(subEvent => subEvent.ParentCalendarEvent.RepeatParentEvent)
                    //    .Include(subEvent => subEvent.RepeatParentEvent)
                    //    .Include(subEvent => subEvent.ParentCalendarEvent.RestrictionProfile_DB)
                    //    .Include(subEvent => subEvent.RepeatParentEvent.RestrictionProfile_DB)
                    //    .Include(subEvent => subEvent.ProfileOfNow_EventDB)
                    //    .Include(subEvent => subEvent.Procrastination_EventDB)
                    //    .Include(subEvent => subEvent.Location_DB)
                    //    .Include(subEvent => subEvent.ParentCalendarEvent.DayPreference_DB)
                    //    ;

                    #endregion

                    List<CalendarEvent> calEventsRetrieved = calEventQuery.ToList();
                    HashSet<CalendarEvent> parentCals = new HashSet<CalendarEvent>();
                    HashSet<CalendarEvent> calendarEventsFromSubCalquery = new HashSet<CalendarEvent>();
                    HashSet<string> allIds = new HashSet<string>();
                    HashSet<string> subEventIds = new HashSet<string>();
                    HashSet<string> repeatParentIds = new HashSet<string>();
                    HashSet<string> retrievedParentIds = new HashSet<string>();
                    HashSet<string> parentIds = new HashSet<string>();
                    Dictionary<string, List<TilerEvent>> procrastinationToTilerEvents = new Dictionary<string, List<TilerEvent>>();
                    Dictionary<string, List<TilerEvent>> ProfileOfNowToTilerEvents = new Dictionary<string, List<TilerEvent>>();



                    List<SubCalendarEvent> subEventsRetrieved = calEventsRetrieved.SelectMany(calEvent => calEvent.AllSubEvents_DB).ToList();
                    watch.Stop();
                    TimeSpan subeventSpan = watch.Elapsed;
                    Debug.WriteLine("||||>sub event span " + subeventSpan.ToString());
                    watch.Reset();
                    watch.Start();
                    bool isRepetitionObjectLoaded = isRepeatitionLoaded(retrievalOptions);
                    foreach (SubCalendarEvent subEvent in subEventsRetrieved)
                    {
                        CalendarEvent parentCalEvent = subEvent.ParentCalendarEvent;
                        parentCalEvent.isRepeatLoaded_DB = isRepetitionObjectLoaded;
                        parentCalEvent.DefaultCalendarEvent = defaultCalEvent;
                        calendarEventsFromSubCalquery.Add(parentCalEvent);
                        allIds.Add(parentCalEvent.Id);
                        allIds.Add(subEvent.Id);
                        subEventIds.Add(subEvent.Id);
                        retrievedParentIds.Add(subEvent.SubEvent_ID.getCalendarEventComponent());

                        CalendarEvent loopParentCalEvent = parentCalEvent;
                        CalendarEvent previousParent = parentCalEvent;
                        //if (isRepetitionObjectLoaded)
                        {
                            while (loopParentCalEvent != null)
                            {
                                loopParentCalEvent.Now = Now;
                                if (loopParentCalEvent.RepeatParentEvent != null)
                                {
                                    CalendarEvent repeatParent = loopParentCalEvent.RepeatParentEvent as CalendarEvent;
                                    calendarEventsFromSubCalquery.Add(repeatParent);
                                    allIds.Add(loopParentCalEvent.RepeatParentId);
                                    repeatParent.isRepeatLoaded_DB = isRepetitionObjectLoaded;
                                }
                                loopParentCalEvent = loopParentCalEvent.RepeatParentEvent as CalendarEvent;
                            }
                        }
                        
                        
                    }

                    watch.Stop();
                    TimeSpan noParenttSpan = watch.Elapsed;
                    watch.Reset();
                    watch.Start();
                    bool procrastinateubEventPresent = true;


                    var rightOfJoin = _Context.CalEvents.Include(calendarEvent => calendarEvent.Name);
                    if (calIdSet != null && calIdSet.Count > 0)
                    {
                        foreach(string calId in calIdSet)// this is needed just in case a subevent within the regular range is acted upon. Take for exaple There is a calEvent of with subeventA, subeventB and subeventC. However the schedule performed a delete on subeventA and subeventA is out side the timeRange but subeventB and subeventC are within range. You still need to include subeventA because it is being deleted.
                        {
                            EventID eventId = new EventID(calId);
                            if(!retrievedParentIds.Contains(eventId.getCalendarEventComponent()))
                            {
                                parentIds.Add(eventId.getAllEventDictionaryLookup);
                            }
                            
                        }
                        rightOfJoin = rightOfJoin
                            //.Include(calEvent => calEvent.RepeatParentEvent)
                            //.Include(calEvent => calEvent.Location_DB)
                        ////.Include(calEvent => calEvent.AllSubEvents_DB) // This causes an error when adding a new primary type column to only subcalendar event type. When the join in the latter part of this code is run, for some reason entity framework doesnt realize that the primary type is not nullable but tries to assign a null variavle. I think this is because EF loses the default settings when joining the tables
                        .Include(calEvent => calEvent.TimeLineHistory_DB)
                        .Include(calEvent => calEvent.RestrictionProfile_DB)
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        //.Include(calEvent => calEvent.DayPreference_DB)
                        //.Include(calEvent => calEvent.TimeLineHistory_DB.TimeLines_DB)
                        //.Where(calEvent => calEvent.IsEnabled_DB && !calEvent.Complete_EventDB)
                        ;
                    }

                    if(parentIds.Count > 0)
                    {
                        var loadedCalendarEvents = parentIds.Join(rightOfJoin,
                        calEventId => calEventId,
                        dbCalEvent => dbCalEvent.Id,
                        (calEvent, dbCalEvent) => new { calEvent, dbCalEvent })
                        .Select(obj => obj.dbCalEvent)
                        .Where(calEvent => calEvent.IsEnabled_DB && !calEvent.Complete_EventDB)
                        .ToList()
                        ;

                        parentCals = new HashSet<CalendarEvent>(loadedCalendarEvents);
                        foreach (CalendarEvent calEvent in parentCals)
                        {
                            calEvent.isRepeatLoaded_DB = isRepetitionObjectLoaded;
                        }
                    }
                    

                    HashSet<CalendarEvent> calendarEvents = new HashSet<CalendarEvent>();
                    foreach (CalendarEvent calEvent in parentCals.Concat(calendarEventsFromSubCalquery))
                    {
                        if (calEvent != null)
                        {
                            calendarEvents.Add(calEvent);
                        }
                    }

                    if(isRepetitionObjectLoaded)
                    {
                        calendarEvents = new HashSet<CalendarEvent>(calendarEvents.Where(eachCalEvent => !eachCalEvent.IsRepeatsChildCalEvent));
                    }

                    watch.Stop();
                    TimeSpan parenttSpan = watch.Elapsed;
                    watch.Reset();
                    watch.Start();
                    Dictionary<string, CalendarEvent> MyCalendarEventDictionary = calendarEvents
                        .ToDictionary(calEvent => calEvent.Calendar_EventID.getAllEventDictionaryLookup, calEvent => calEvent);
                    foreach (CalendarEvent calEvent in MyCalendarEventDictionary.Values.Where(calEvent => calEvent.getIsEventRestricted))
                    {
                        CalendarEventRestricted calAsRestricted = calEvent as CalendarEventRestricted;
                        calEvent.DefaultCalendarEvent = defaultCalEvent;
                        calAsRestricted.RestrictionProfile_DB?.InitializeOverLappingDictionary();
                        if (Now != null)
                        {
                            if (retrievalOptions.Contains( DataRetrivalOption.Evaluation))
                            {
                                calAsRestricted.setNow(Now, true);
                            }
                            else
                            {
                                calAsRestricted.setNow(Now, false);
                            }
                        }
                    }

                    if (!procrastinateubEventPresent)
                    {
                        EventID clearAllId = new EventID(_TilerUser.ClearAllId);

                        var procrastinateCalEvent = MyCalendarEventDictionary[clearAllId.getAllEventDictionaryLookup];
                        procrastinateCalEvent.AllSubEvents_DB = new List<SubCalendarEvent>();
                    }

                    watch.Stop();
                    TimeSpan dictionaryReorgSpan = watch.Elapsed;
                    Debug.WriteLine("no parentSpan " + noParenttSpan.ToString());
                    Debug.WriteLine("parentSpan " + parenttSpan.ToString());
                    Debug.WriteLine("dictionaryReorgSpan " + dictionaryReorgSpan.ToString());

                    return MyCalendarEventDictionary;
                }

                throw new NullReferenceException("You have to provide the range to lookup in the user calelndar");
            }
            else
            {
                return await getAllNonEvaluationEnabledCalendarEvent(RangeOfLookUP, Now, retrievalOptions, tilerIds).ConfigureAwait(false);
            }
        }

        async public virtual Task<Dictionary<string, CalendarEvent>> getAllNonEvaluationEnabledCalendarEvent(TimeLine RangeOfLookUP, ReferenceNow Now,
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> tilerIds = null)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            CalendarEvent defaultCalEvent = CalendarEvent.getEmptyCalendarEvent(EventID.GenerateCalendarEvent(), Now.constNow, Now.constNow.AddHours(1));
            if (RangeOfLookUP != null)
            {
                IQueryable<SubCalendarEvent> subCalendarEvents = getSubCalendarEventQuery(retrievalOptions, tilerIds);
                subCalendarEvents = subCalendarEvents
                    .Where(subEvent =>
                            subEvent.StartTime_EventDB < RangeOfLookUP.End
                            && subEvent.EndTime_EventDB > RangeOfLookUP.Start
                            && subEvent.ParentCalendarEvent.IsEnabled_DB
                            && !subEvent.ParentCalendarEvent.Complete_EventDB
                            && subEvent.ParentCalendarEvent.StartTime_EventDB < RangeOfLookUP.End
                            && subEvent.ParentCalendarEvent.EndTime_EventDB > RangeOfLookUP.Start
                            && ((subEvent.RepeatParentEventId == null) || (subEvent.RepeatParentEvent != null && subEvent.RepeatParentEvent.IsEnabled_DB
                            && !subEvent.RepeatParentEvent.Complete_EventDB
                            && subEvent.RepeatParentEvent.StartTime_EventDB < RangeOfLookUP.End
                            && subEvent.RepeatParentEvent.EndTime_EventDB > RangeOfLookUP.Start))
                            && (
                                    (subEvent.RepeatParentEventId == null) ||
                                    (
                                        (
                                            (
                                                subEvent.RepeatParentEventId != null && subEvent.RepeatParentEvent.IsEnabled_DB
                                                && !subEvent.RepeatParentEvent.Complete_EventDB
                                                && subEvent.RepeatParentEvent.StartTime_EventDB < RangeOfLookUP.End
                                                && subEvent.RepeatParentEvent.EndTime_EventDB > RangeOfLookUP.Start
                                            ) &&
                                            (subEvent.RepeatParentEvent != null && subEvent.RepeatParentEvent.IsEnabled_DB)
                                        )
                                        && (
                                            (
                                                (subEvent.RepeatParentEvent.RepeatParentEventId == null) ||
                                                (
                                                    (
                                                        subEvent.RepeatParentEvent.RepeatParentEventId != null && subEvent.RepeatParentEvent.RepeatParentEvent.IsEnabled_DB
                                                        && !subEvent.RepeatParentEvent.RepeatParentEvent.Complete_EventDB
                                                        && subEvent.RepeatParentEvent.RepeatParentEvent.StartTime_EventDB < RangeOfLookUP.End
                                                        && subEvent.RepeatParentEvent.RepeatParentEvent.EndTime_EventDB > RangeOfLookUP.Start
                                                    ) &&
                                                    (subEvent.RepeatParentEvent.RepeatParentEvent != null && subEvent.RepeatParentEvent.RepeatParentEvent.IsEnabled_DB)
                                                )
                                            )
                                        )
                                    )
                                )
                        )
                        .Include(subEvent => subEvent.ParentCalendarEvent.RepeatParentEvent)
                        .Include(subEvent => subEvent.ParentCalendarEvent.TimeLineHistory_DB)
                        .Include(subEvent => subEvent.ParentCalendarEvent.TimeLineHistory_DB.TimeLines_DB)
                        .Include(subEvent => subEvent.RepeatParentEvent)
                        ;

                HashSet<CalendarEvent> parentCals = new HashSet<CalendarEvent>();
                HashSet<CalendarEvent> repeatCalendarEvents = new HashSet<CalendarEvent>();
                HashSet<string> allIds = new HashSet<string>();
                HashSet<string> repeatParentIds = new HashSet<string>();
                HashSet<string> parentIds = new HashSet<string>();
                HashSet<string> repeatIds = new HashSet<string>();
                List<SubCalendarEvent> subEventsRetrieved = subCalendarEvents.ToList();
                watch.Stop();
                TimeSpan subeventSpan = watch.Elapsed;
                Debug.WriteLine("sub event span " + subeventSpan.ToString());
                watch.Reset();
                watch.Start();
                foreach (SubCalendarEvent subEvent in subCalendarEvents)
                {

                    CalendarEvent parentCalEvent = subEvent.ParentCalendarEvent;
                    parentCalEvent.DefaultCalendarEvent = defaultCalEvent;
                    parentIds.Add(subEvent.ParentCalendarEvent.Id);
                    allIds.Add(subEvent.ParentCalendarEvent.Id);
                    if (subEvent.RepeatParentEvent != null)
                    {
                        repeatIds.Add(subEvent.RepeatParentEvent.Id);
                        repeatCalendarEvents.Add(subEvent.RepeatParentEvent as CalendarEvent);
                        parentIds.Add(subEvent.RepeatParentEvent.Id);
                    }

                    if (subEvent.ParentCalendarEvent.RepeatParentEvent != null)
                    {
                        parentIds.Add(subEvent.ParentCalendarEvent.RepeatParentEvent.Id);
                    }
                }

                watch.Stop();
                TimeSpan noParenttSpan = watch.Elapsed;
                watch.Reset();
                watch.Start();
                bool procrastinateubEventPresent = true;
                if (!parentIds.Contains(_TilerUser.ClearAllId))
                {
                    parentIds.Add(_TilerUser.ClearAllId);
                    procrastinateubEventPresent = false;
                }


                var loadedCalendarEvents = parentIds.Join(_Context.CalEvents
                    .Include(calendarEvent => calendarEvent.DataBlob_EventDB)
                    .Include(calendarEvent => calendarEvent.Name)
                    .Include(calendarEvent => calendarEvent.Name.Creator_EventDB)
                    .Include(calendarEvent => calendarEvent.Location_DB)
                    .Include(calendarEvent => calendarEvent.Creator_EventDB)
                    .Include(calendarEvent => calendarEvent.ProfileOfNow_EventDB)
                    .Include(calendarEvent => calendarEvent.DayPreference_DB)
                    .Include(calendarEvent => calendarEvent.Procrastination_EventDB)
                    .Include(calendarEvent => calendarEvent.RestrictionProfile_DB)
                    .Include(calendarEvent => calendarEvent.TimeLineHistory_DB)
                    .Include(calendarEvent => calendarEvent.TimeLineHistory_DB.TimeLines_DB)
                    ,
                    calEventId => calEventId,
                    dbCalEvent => dbCalEvent.Id,
                    (calEvent, dbCalEvent) => new { calEvent, dbCalEvent })
                    .Select(obj => obj.dbCalEvent).ToList();

                parentCals = new HashSet<CalendarEvent>(loadedCalendarEvents);





                List<CalendarEvent> calendarEvents = new List<CalendarEvent>();
                if (repeatCalendarEvents.Count > 0)
                {
                    IQueryable<Repetition> repetittions = getRepeatCalendarEvent(false);
                    var enumerable = repeatCalendarEvents.Join(
                        repetittions,
                        calEvent => calEvent.EventRepetitionId,
                        repetition => repetition.Id,
                        (calEvent, repetition) => new { repetition = repetition, calEvent = calEvent }).ToList();
                    //.AsEnumerable();

                    foreach (var obj in enumerable)
                    {
                        obj.calEvent.Repetition_EventDB = obj.repetition;

                    }

                }



                calendarEvents.AddRange(parentCals);
                watch.Stop();

                TimeSpan parenttSpan = watch.Elapsed;
                watch.Reset();
                watch.Start();

                List<CalendarEvent> parentCalEvents = new List<CalendarEvent>();
                List<CalendarEvent> childCalEvents = new List<CalendarEvent>();
                List<CalendarEvent> weekDayCalendarEvents = new List<CalendarEvent>();
                foreach (var calEvent in calendarEvents)
                {
                    calEvent.DefaultCalendarEvent = defaultCalEvent;
                    if (!calEvent.IsRepeatsChildCalEvent)
                    {
                        parentCalEvents.Add(calEvent);
                    }
                    else
                    {
                        if (!calEvent.IsRepeatsChildCalEvent || !calEvent.IsRecurring)
                        {
                            childCalEvents.Add(calEvent);
                        }
                        else
                        {
                            weekDayCalendarEvents.Add(calEvent);
                        }

                    }
                }

                Dictionary<string, Tuple<CalendarEvent, List<CalendarEvent>>> calToRepeatCalEvents = parentCalEvents.ToDictionary(obj => obj.Calendar_EventID.getCalendarEventComponent(), obj => new Tuple<CalendarEvent, List<CalendarEvent>>(obj, new List<CalendarEvent>()));

                foreach (CalendarEvent calEvent in childCalEvents)
                {
                    calEvent.DefaultCalendarEvent = defaultCalEvent;
                    string calId = calEvent.Calendar_EventID.getCalendarEventComponent();
                    if (calToRepeatCalEvents.ContainsKey(calId))
                    {
                        calToRepeatCalEvents[calId].Item2.Add(calEvent);
                    }
                }

                foreach (CalendarEvent calEvent in parentCalEvents.Where(calEvent => calEvent.IsRecurring))
                {
                    calEvent.DefaultCalendarEvent = defaultCalEvent;
                    string calComponentId = calEvent.getTilerID.getCalendarEventComponent();
                    if (calToRepeatCalEvents.ContainsKey(calComponentId))
                    {
                        List<CalendarEvent> calEvents = calToRepeatCalEvents[calComponentId].Item2;
                        if (calEvent.Repeat.SubRepetitions != null && calEvent.Repeat.SubRepetitions.Count > 0)
                        {
                            var weekDayToRepetition = calEvents.ToLookup(repCalEvent => (DayOfWeek)(Convert.ToInt32(repCalEvent.getTilerID.getRepeatDayCalendarEventComponent())), repCalEvent => repCalEvent);
                            foreach (var kvp in weekDayToRepetition)
                            {
                                Repetition repetition = calEvent.Repeat.getSubRepeitionByWeekDay(kvp.Key);
                                var allEvents = weekDayToRepetition[kvp.Key];
                                repetition.RepeatingEvents = allEvents.ToList();
                            }


                        }
                        else
                        {
                            if (calEvents.Count != calEvent.Repeat.RepeatingEvents.Count)
                            {
                                var AllCalEvents = calEvent.Repeat.RepeatingEvents.Except(calEvents).ToList();
                                foreach (var calEVent in AllCalEvents)
                                {
                                    _Context.Entry(calEVent).State = EntityState.Detached; // this is needed because of the call to IQueryable<Repetition> repetittions = getRepeatCalendarEvent(DataRetrivalOption.Evaluation, false);. That line autopopulates and JOINS(capitalization is no mistake) Repetion.repeatingevents with any corresponding calendar event (based on the autogenerated repetion_id column) that are already attached to the context AKA already part of context object. 
                                }
                                calEvent.Repeat.RepeatingEvents = calEvents;
                            }

                        }
                    }
                }



                Dictionary<string, CalendarEvent> MyCalendarEventDictionary = parentCalEvents.Where(calEvent => !calEvent.IsRepeatsChildCalEvent).ToDictionary(calEvent => calEvent.Calendar_EventID.getAllEventDictionaryLookup, calEvent => calEvent);
                foreach (CalendarEvent calEvent in MyCalendarEventDictionary.Values.Where(calEvent => calEvent.getIsEventRestricted))
                {
                    CalendarEventRestricted calAsRestricted = calEvent as CalendarEventRestricted;
                    calEvent.DefaultCalendarEvent = defaultCalEvent;
                    
                        calAsRestricted.RestrictionProfile_DB.InitializeOverLappingDictionary();
                        if (Now != null)
                        {
                            if (retrievalOptions.Contains(DataRetrivalOption.Evaluation))
                            {
                                calAsRestricted.setNow(Now, true);
                            }
                            else
                            {
                                calAsRestricted.setNow(Now, false);
                            }
                        }

                }

                if (!procrastinateubEventPresent)
                {
                    EventID clearAllId = new EventID(_TilerUser.ClearAllId);

                    var procrastinateCalEvent = MyCalendarEventDictionary[clearAllId.getAllEventDictionaryLookup];
                    procrastinateCalEvent.AllSubEvents_DB = new List<SubCalendarEvent>();
                }

                watch.Stop();
                TimeSpan dictionaryReorgSpan = watch.Elapsed;
                Debug.WriteLine("no parentSpan " + noParenttSpan.ToString());
                Debug.WriteLine("parentSpan " + parenttSpan.ToString());
                Debug.WriteLine("dictionaryReorgSpan " + dictionaryReorgSpan.ToString());

                return MyCalendarEventDictionary;
            }

            throw new NullReferenceException("You have to provide the range to lookup in the user calelndar");

        }
        

        public virtual Tuple<Dictionary<string, CalendarEvent>, Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>> getAllCalendarFromXml(ScheduleDump scheduleDump, ReferenceNow now)
        {
            Dictionary<string, CalendarEvent> MyCalendarEventDictionary = new Dictionary<string, CalendarEvent>();
            List<CalendarEvent> googleCalendarEvents = new List<CalendarEvent>();
            Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>> thirdPartyCalendarEvent = new Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>();
            XmlDocument doc = scheduleDump.XmlDoc;
            XmlNode IdNode = doc.DocumentElement.SelectSingleNode("/ScheduleLog/LastIDCounter");

            XmlNode EventSchedulesNodes = doc.DocumentElement.SelectSingleNode("/ScheduleLog/EventSchedules");
            CalendarEvent defaultCalendarEvent = CalendarEvent.getEmptyCalendarEvent(EventID.GenerateCalendarEvent(), Now.constNow, Now.constNow.AddHours(1));


            if (EventSchedulesNodes.ChildNodes != null)
            {
                string googleId = EventID.generateGoogleCalendarEventID(0).getCalendarEventComponent();
                foreach (XmlNode EventScheduleNode in EventSchedulesNodes.ChildNodes)
                {
                    CalendarEvent RetrievedEvent;
                    RetrievedEvent = getCalendarEventObjFromNode(EventScheduleNode);
                    if (RetrievedEvent != null)
                    {
                        RetrievedEvent.DefaultCalendarEvent = defaultCalendarEvent;
                        if(RetrievedEvent.IsFromRecurringAndNotChildRepeatCalEvent && RetrievedEvent.isRepeatLoaded)
                        {
                            foreach(CalendarEvent calEvent in RetrievedEvent.Repeat.RecurringCalendarEvents())
                            {
                                calEvent.DefaultCalendarEvent = defaultCalendarEvent;
                            }
                        }
                        string id = RetrievedEvent.Calendar_EventID.getAllEventDictionaryLookup;
                        
                        string calendarComponent = RetrievedEvent.Calendar_EventID.getCalendarEventComponent();
                        if (googleId == calendarComponent)
                        {
                            googleCalendarEvents.AddRange(RetrievedEvent.Repeat.RecurringCalendarEvents());
                            googleCalendarEvents.Add(RetrievedEvent);
                        } else
                        {
                            MyCalendarEventDictionary.Add(id, RetrievedEvent);
                        }
                    }
                }
            }
#region GoogleCalendarEvent
            if(googleCalendarEvents.Count > 0)
            {
                ThirdPartyCalendarEvent googleCalendarEvent = new GoogleCalendarEvent(googleCalendarEvents, _TilerUser);
                thirdPartyCalendarEvent.Add(ThirdPartyControl.CalendarTool.google, new List<CalendarEvent>() { googleCalendarEvent });
            }
            
#endregion

            Tuple<Dictionary<string, CalendarEvent>, Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>> retValue = new Tuple<Dictionary<string, CalendarEvent>, Dictionary<ThirdPartyControl.CalendarTool, List<CalendarEvent>>>(
                MyCalendarEventDictionary,
                thirdPartyCalendarEvent);

            return retValue;
        }

        public virtual CalendarEvent getCalendarEventObjFromNode(XmlNode EventScheduleNode)
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
            Name = new DB_EventName(null, null, EventScheduleNode.SelectSingleNode("Name").InnerText, NameId);
            ID = EventScheduleNode.SelectSingleNode("ID").InnerText;
            //EventScheduleNode.SelectSingleNode("ID").InnerXml = "<wetin></wetin>";
            Deadline = EventScheduleNode.SelectSingleNode("Deadline").InnerText;
            Rigid = EventScheduleNode.SelectSingleNode("RigidFlag").InnerText;
            XmlNode RecurrenceXmlNode = EventScheduleNode.SelectSingleNode("Recurrence");
            EventRepetitionflag = EventScheduleNode.SelectSingleNode("RepetitionFlag").InnerText;
            string IsRepeatsChildCalEventString = EventScheduleNode.SelectSingleNode("IsRepeatsChildCalEvent")?.InnerText;


            //DateTimeOffset StartDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("StartTime").InnerText).UtcDateTime;
            //DateTimeOffset EndDateTimeStruct = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("Deadline").InnerText).UtcDateTime;
            //DateTimeOffset start = DateTimeOffset.Parse(EventScheduleNode.SelectSingleNode("StartTime").InnerText);
            DateTimeOffset StartDateTimeStruct = Utility.ParseTime(EventScheduleNode.SelectSingleNode("StartTime").InnerText);
            DateTimeOffset EndDateTimeStruct = Utility.ParseTime(EventScheduleNode.SelectSingleNode("Deadline").InnerText);
            DateTimeOffset start = Utility.ParseTime(EventScheduleNode.SelectSingleNode("StartTime").InnerText);
            StartDateTime = EventScheduleNode.SelectSingleNode("StartTime").InnerText.Split(' ');

            StartDate = StartDateTime[0];
            StartTime = StartDateTime[1] + StartDateTime[2];
            EndDateTime = EventScheduleNode.SelectSingleNode("Deadline").InnerText.Split(' ');
            DateTimeOffset end = Utility.ParseTime(EventScheduleNode.SelectSingleNode("Deadline").InnerText);
            EndDate = EndDateTime[0];
            EndTime = EndDateTime[1] + EndDateTime[2];
            DateTimeOffset StartTimeConverted = Utility.ParseTime(StartDate);// new DateTimeOffset(Convert.ToInt32(StartDate.Split('/')[2]), Convert.ToInt32(StartDate.Split('/')[0]), Convert.ToInt32(StartDate.Split('/')[1]));
            DateTimeOffset EndTimeConverted = Utility.ParseTime(EndDate); //new DateTimeOffset(Convert.ToInt32(EndDate.Split('/')[2]), Convert.ToInt32(EndDate.Split('/')[0]), Convert.ToInt32(EndDate.Split('/')[1]));

            Repetition Recurrence;
            if (Convert.ToBoolean(EventRepetitionflag))
            {
                Recurrence = getRepetitionObject(RecurrenceXmlNode);
                if(Recurrence!=null)
                {
                    StartTimeConverted = (Recurrence.Range.Start);
                    EndTimeConverted = (Recurrence.Range.End);
                }
            }
            else
            {
                Recurrence = new Repetition();
            }
            Split = Convert.ToInt32(EventScheduleNode.SelectSingleNode("Split").InnerText);
            PreDeadline = TimeSpan.Parse(EventScheduleNode.SelectSingleNode("PreDeadline").InnerText);
            CalendarEventDuration = TimeSpan.Parse(EventScheduleNode.SelectSingleNode("Duration").InnerText);
            PrepTime = TimeSpan.Parse(EventScheduleNode.SelectSingleNode("PrepTime").InnerText);
            Completed = EventScheduleNode.SelectSingleNode("Completed").InnerText;
            EnableFlag = EventScheduleNode.SelectSingleNode("Enabled").InnerText;
            string iniStartTimeString = EventScheduleNode.SelectSingleNode("IniStartTime")?.InnerText;
            string iniEndTimeString = EventScheduleNode.SelectSingleNode("IniEndTime")?.InnerText;
            long iniStartTime = iniStartTimeString.isNot_NullEmptyOrWhiteSpace() ? Convert.ToInt64(iniStartTimeString) : -1;
            long iniEndTime = iniEndTimeString.isNot_NullEmptyOrWhiteSpace() ? Convert.ToInt64(iniEndTimeString) : -1;
            string timeLineUpdates = EventScheduleNode.SelectSingleNode("TimeLineUpdates")?.InnerText;


            bool EVentEnableFlag = Convert.ToBoolean(EnableFlag);
            bool completedFlag = Convert.ToBoolean(Completed);

            XmlNode completeNode = EventScheduleNode.SelectSingleNode("CompletionCount");
            XmlNode deleteNode = EventScheduleNode.SelectSingleNode("DeletionCount");
            int CompleteCount = 0;
            int DeleteCount = 0;

            TilerElements.Location location = getLocation(EventScheduleNode);
            MiscData noteData = getMiscData(EventScheduleNode);
            EventDisplay UiData = getDisplayUINode(EventScheduleNode);
            Procrastination procrastinationData = generateProcrastinationObject(EventScheduleNode);
            NowProfile NowProfileData = generateNowProfile(EventScheduleNode);
            bool procrastinationEventFlag = Convert.ToBoolean(EventScheduleNode.SelectSingleNode("isProcrastinateEvent")?.InnerText ?? "False");
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
                if (creator.Id == this._TilerUser.Id)
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
                    RetrievedEvent = new RigidCalendarEvent(
                    Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Recurrence, location, UiData, noteData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, new EventID(ID), NowProfileData, null);
                }




            }
            else
            {
                RetrievedEvent = new CalendarEvent(Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Split, Recurrence, location, UiData, noteData, procrastinationData, NowProfileData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, new EventID(ID), null);
            }

            if (rigidFlag && procrastinationEventFlag)
            {
                creator.ClearAllId = ID;
                RetrievedEvent = new DB_ProcrastinateCalendarEvent(new EventID(creator.getClearAllEventsId()), Name, start, end, CalendarEventDuration, PreDeadline, PrepTime, Recurrence, location, UiData, noteData, EVentEnableFlag, completedFlag, creator, userGroup, timeZone, Split, NowProfileData);
            }
            else
            {
                RetrievedEvent = new DB_CalendarEvent(RetrievedEvent, procrastinationData, NowProfileData);
            }
            Name.Creator_EventDB = RetrievedEvent.getCreator;
            Name.AssociatedEvent = RetrievedEvent;
            SubCalendarEvent[] AllSubCalEvents = ReadSubSchedulesFromXMLNode(EventScheduleNode.SelectSingleNode("EventSubSchedules"), RetrievedEvent).ToArray();
            XmlNode restrictedNode = EventScheduleNode.SelectSingleNode("Restricted");
            XmlNode eventPreferenceNode = EventScheduleNode.SelectSingleNode("EventPreference");
            XmlNode updateHistoryNode = EventScheduleNode.SelectSingleNode("UpdateHistory");

            XmlNode calendarTypeNode = EventScheduleNode.SelectSingleNode("ThirdpartyType");
            if (calendarTypeNode != null)
            {
                ThirdPartyControl.CalendarTool calendarType = getThirdPartyCalendarType(calendarTypeNode);
                if (calendarType != ThirdPartyControl.CalendarTool.tiler)
                {
                    DB_CalendarEvent calEvent = (RetrievedEvent as DB_CalendarEvent);
                    calEvent.CalendarType = calendarType;
                }
            }

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
                    RetrievedEvent = new DB_CalendarEventRestricted(RetrievedEvent, myRestrictionProfile, this.Now);
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
            if (timeCreatednode == null)
            {
                timeCreatednode = EventScheduleNode.SelectSingleNode("TimeCreaated"); // this is to handle previous typo issues
            }

            if (timeCreatednode != null)
            {
                timeCreated = Utility.ParseTime(timeCreatednode.InnerText.ToString());
                RetrievedEvent.TimeCreated = timeCreated;
            }
            EventPreference eventPreference = null;
            if (eventPreferenceNode!=null && !string.IsNullOrEmpty(eventPreferenceNode.InnerXml) && !string.IsNullOrWhiteSpace(eventPreferenceNode.InnerXml))
            {
                eventPreference = getEventPreference(eventPreferenceNode);
            }
            else
            {
                eventPreference = EventPreference.NullInstance;
            }

            TimeLineHistory updateHistory = null;
            if (updateHistoryNode != null && !string.IsNullOrEmpty(updateHistoryNode.InnerXml) && !string.IsNullOrWhiteSpace(updateHistoryNode.InnerXml))
            {
                updateHistory = getupdateHistory(updateHistoryNode);
            }
            
            RetrievedEvent.DayPreference_DB = eventPreference;

            RetrievedEvent.TimeLineHistory_DB = updateHistory;

            RetrievedEvent.InitializeCounts(DeleteCount, CompleteCount);

            XmlNode LastCompletionTimesNode = EventScheduleNode.SelectSingleNode("LastCompletionTimes");
            if(LastCompletionTimesNode!=null)
            {
                RetrievedEvent.LastCompletionTime_DB = LastCompletionTimesNode.InnerText;
            }

            XmlNode MyEventScheduleNode = EventScheduleNode.SelectSingleNode("Access_DB");

            XmlNode DeadlineSuggestionNode = EventScheduleNode.SelectSingleNode("DeadlineSuggestion");
            if (DeadlineSuggestionNode != null)
            {
                RetrievedEvent.DeadlineSuggestion_DB =long.Parse(DeadlineSuggestionNode.InnerText);
            }
            
            if (MyEventScheduleNode!=null)
            {
                RetrievedEvent.Access_DB = MyEventScheduleNode.InnerText;
            }
            if(Recurrence!=null)
            {
                foreach (CalendarEvent calEvent in Recurrence.RepeatingEvents)
                {
                    calEvent.RepeatParent_DB = RetrievedEvent;
                }
            } else
            {
                RetrievedEvent.IsRecurring = Convert.ToBoolean(EventRepetitionflag);
                RetrievedEvent.isRepeatLoaded_DB = false;
            }

            if(!string.IsNullOrEmpty(IsRepeatsChildCalEventString) && !string.IsNullOrWhiteSpace(IsRepeatsChildCalEventString))
            {
                RetrievedEvent.IsRepeatsChildCalEvent = Convert.ToBoolean(IsRepeatsChildCalEventString);
            }
            


            RetrievedEvent.InitialStartTime_DB = iniStartTime;
            RetrievedEvent.InitialEndTime_DB= iniEndTime;

            return RetrievedEvent;
        }

        public WeatherReason getWeatherReason(XmlNode ReasonNode)
        {
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

        public EventPreference getEventPreference(XmlNode PreferenceNode)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(PreferenceNode.OuterXml);
            stw.Flush();

            stm.Position = 0;
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = PreferenceNode.Name;
            xRoot.IsNullable = true;
            XmlSerializer ser = new XmlSerializer(typeof(EventPreference), xRoot);
            EventPreference result = (ser.Deserialize(stm) as EventPreference);

            return result;
        }

        public TimeLineHistory getupdateHistory(XmlNode UpdateHistoryNode)
        {
            MemoryStream stm = new MemoryStream();
            StreamWriter stw = new StreamWriter(stm);
            stw.Write(UpdateHistoryNode.OuterXml);
            stw.Flush();

            stm.Position = 0;
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = UpdateHistoryNode.Name;
            xRoot.IsNullable = true;
            XmlSerializer ser = new XmlSerializer(typeof(TimeLineHistory), xRoot);
            TimeLineHistory result = (ser.Deserialize(stm) as TimeLineHistory);

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
            if (ProcrastinationProfileNode == null)
            {
                return null;
            }
            string ProcrastinationPreferredStartString = ProcrastinationProfileNode.SelectSingleNode("ProcrastinationPreferredStart")?.InnerText;
            string ProcrastinationDislikedStartString = ProcrastinationProfileNode.SelectSingleNode("ProcrastinationDislikedStart")?.InnerText;
            string ProcrastinationDislikedDaySectionString = ProcrastinationProfileNode.SelectSingleNode("ProcrastinationDislikedDaySection")?.InnerText;
            DateTimeOffset ProcrastinationPreferredStart = string.IsNullOrEmpty(ProcrastinationPreferredStartString) ? new DateTimeOffset() : DateTimeOffset.Parse(ProcrastinationPreferredStartString).UtcDateTime; ;
            DateTimeOffset ProcrastinationDislikedStart = string.IsNullOrEmpty(ProcrastinationDislikedStartString) ? new DateTimeOffset() : DateTimeOffset.Parse(ProcrastinationDislikedStartString).UtcDateTime;
            int DaySection = string.IsNullOrEmpty(ProcrastinationDislikedDaySectionString) ? 0 : Convert.ToInt32(ProcrastinationDislikedDaySectionString);
            DB_Procrastination retValue = new DB_Procrastination(ProcrastinationDislikedStart, ProcrastinationPreferredStart, DaySection);
            return retValue;
        }

        protected virtual List<SubCalendarEvent> ReadSubSchedulesFromXMLNode(XmlNode MyXmlNode, CalendarEvent MyParent)
        {
            List<SubCalendarEvent> MyArrayOfNodes = new List<SubCalendarEvent>();
            string ID = "";
            DateTimeOffset Start = new DateTimeOffset();
            DateTimeOffset End = new DateTimeOffset();
            TimeSpan PrepTime = new TimeSpan();
            BusyTimeLine BusySlot = new BusyTimeLine();
            bool Enabled;
            for (int i = 0; i < MyXmlNode.ChildNodes.Count; i++)
            {

                XmlNode SubEventNode = MyXmlNode.ChildNodes[i];
                BusyTimeLine SubEventActivePeriod = new BusyTimeLine(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText, stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText), stringToDateTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText));
                ID = EventID.convertToSubcalendarEventID(MyXmlNode.ChildNodes[i].SelectSingleNode("ID").InnerText).ToString();
                Start = Utility.ParseTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveStartTime").InnerText);
                End = Utility.ParseTime(MyXmlNode.ChildNodes[i].SelectSingleNode("ActiveEndTime").InnerText);

                string iniStartTimeString = MyXmlNode.ChildNodes[i].SelectSingleNode("IniStartTime")?.InnerText;
                string iniEndTimeString = MyXmlNode.ChildNodes[i].SelectSingleNode("IniEndTime")?.InnerText;


                long iniStartTimeInMs = iniStartTimeString.isNot_NullEmptyOrWhiteSpace() ? Convert.ToInt64(iniStartTimeString) : -1;
                long iniEndTimeInMS = iniEndTimeString.isNot_NullEmptyOrWhiteSpace() ? Convert.ToInt64(iniEndTimeString) : -1;


                DateTimeOffset iniStartTime = DateTimeOffset.FromUnixTimeMilliseconds(iniStartTimeInMs);
                DateTimeOffset iniEndTime = DateTimeOffset.FromUnixTimeMilliseconds(iniEndTimeInMS);

                bool rigidFlag = MyParent.isRigid;
                XmlNode rigidNode = MyXmlNode.ChildNodes[i].SelectSingleNode("Rigid");
                if (rigidNode != null)
                {
                    rigidFlag = Convert.ToBoolean(rigidNode.InnerText);
                }


                BusySlot = new BusyTimeLine(ID, Start, End);
                PrepTime = new TimeSpan(ConvertToMinutes(MyXmlNode.ChildNodes[i].SelectSingleNode("PrepTime").InnerText) * 60 * 10000000);
                //stringToDateTime();
                Start = Utility.ParseTime(MyXmlNode.ChildNodes[i].SelectSingleNode("StartTime").InnerText);
                End = Utility.ParseTime(MyXmlNode.ChildNodes[i].SelectSingleNode("EndTime").InnerText);
                Enabled = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Enabled").InnerText);
                bool CompleteFlag = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("Complete").InnerText);
                TilerElements.Location var1 = getLocation(MyXmlNode.ChildNodes[i]);
                MiscData noteData = getMiscData(MyXmlNode.ChildNodes[i]);
                EventDisplay UiData = getDisplayUINode(MyXmlNode.ChildNodes[i]);
                ConflictProfile conflictProfile = getConflctProfile(MyXmlNode.ChildNodes[i]);
                string nameString = MyXmlNode.ChildNodes[i].SelectSingleNode("Name")?.InnerText ?? MyParent.getName.NameValue;
                string id = MyXmlNode.ChildNodes[i].SelectSingleNode("NameId")?.InnerText ?? MyParent.getName.NameId;
                EventName name = new DB_EventName(null, null, nameString, id);
                TilerUser creator = getTilerLoggedUser(MyXmlNode.ChildNodes[i].SelectSingleNode("UserNode"));
                if ((creator as DB_TilerUser).isNull)
                {
                    creator = MyParent.getCreator;
                }
                string timeZone = MyXmlNode.ChildNodes[i].SelectSingleNode("TimeZone")?.InnerText ?? "UTC";
                DB_TilerUserGroup userGroup = getTilerUserGroup(MyXmlNode.ChildNodes[i].SelectSingleNode("UserGroup"));
                Tuple<TimeSpan, DateTimeOffset, List<PausedTimeLine>> PauseData = getPauseData(SubEventNode);

                SubCalendarEvent retrievedSubEvent;
                bool procrastinationEventFlag = Convert.ToBoolean(MyXmlNode.ChildNodes[i].SelectSingleNode("isProcrastinateEvent")?.InnerText ?? "False");
                if (!procrastinationEventFlag)
                {
                    retrievedSubEvent = new DB_SubCalendarEvent(MyParent, creator, userGroup, timeZone, ID, name, BusySlot, Start, End, PrepTime, ID, rigidFlag, Enabled, UiData, noteData, CompleteFlag, var1, MyParent.StartToEnd, conflictProfile);
                    retrievedSubEvent = new DB_SubCalendarEvent(retrievedSubEvent, MyParent.getNowInfo, MyParent.getProcrastinationInfo, MyParent);
                    (retrievedSubEvent as DB_SubCalendarEvent).UsedTime_EventDB = (long)PauseData.Item1.TotalMilliseconds;
                    (retrievedSubEvent as DB_SubCalendarEvent).setPausedTimeSlots(PauseData.Item3);
                }
                else
                {
                    DB_ProcrastinateAllSubCalendarEvent procrastinateSubEvent = new DB_ProcrastinateAllSubCalendarEvent(creator, userGroup, timeZone, new TimeLine(Start, End), new EventID(ID), MyParent.Location, MyParent as ProcrastinateCalendarEvent, Enabled, CompleteFlag);
                    procrastinateSubEvent.UsedPauseTime_DB = (long)PauseData.Item1.TotalMilliseconds;
                    (procrastinateSubEvent ).setPausedTimeSlots(PauseData.Item3);
                    retrievedSubEvent = procrastinateSubEvent;
                }
                name.Creator_EventDB = retrievedSubEvent.getCreator;
                name.AssociatedEvent = retrievedSubEvent;
                retrievedSubEvent.ThirdPartyID = MyXmlNode.ChildNodes[i].SelectSingleNode("ThirdPartyID").InnerText;//this is a hack to just update the Third partyID
                XmlNode restrictedNode = MyXmlNode.ChildNodes[i].SelectSingleNode("Restricted");





                if (restrictedNode != null)
                {
                    if (Convert.ToBoolean(restrictedNode.InnerText))
                    {
                        XmlNode RestrictionProfileNode = MyXmlNode.ChildNodes[i].SelectSingleNode("RestrictionProfile");
                        DB_RestrictionProfile myRestrictionProfile = (DB_RestrictionProfile)getRestrictionProfile(RestrictionProfileNode);
                        retrievedSubEvent = new DB_SubCalendarEventRestricted(retrievedSubEvent, myRestrictionProfile, MyParent as CalendarEventRestricted, this.Now);
                        (retrievedSubEvent as DB_SubCalendarEventRestricted).UsedTime_EventDB = (long)PauseData.Item1.TotalMilliseconds;
                        (retrievedSubEvent as DB_SubCalendarEventRestricted).setPausedTimeSlots(PauseData.Item3);
                    }
                }

                DateTimeOffset timeCreated;
                XmlNode timeCreatednode = MyXmlNode.ChildNodes[i].SelectSingleNode("TimeCreated");
                if (timeCreatednode != null)
                {
                    timeCreated = Utility.ParseTime(timeCreatednode.InnerText);
                    retrievedSubEvent.TimeCreated = timeCreated;
                }

                TimeSpan TravelTimeAfter;
                TimeSpan TravelTimeBefore;
                TimeSpan.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("TravelTimeAfter")?.InnerText ?? "", out TravelTimeAfter);
                TimeSpan.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("TravelTimeBefore")?.InnerText ?? "", out TravelTimeBefore);

                bool isSleep, isWake, isTardy;
                bool.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("isSleep")?.InnerText ?? "", out isSleep);
                bool.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("isWake")?.InnerText ?? "", out isWake);
                bool.TryParse(MyXmlNode.ChildNodes[i].SelectSingleNode("isTardy")?.InnerText ?? "", out isTardy);

                retrievedSubEvent.isSleep = isSleep;
                retrievedSubEvent.isWake = isWake;
                retrievedSubEvent.IsTardy_DB = isTardy;

                retrievedSubEvent.TravelTimeAfter = TravelTimeAfter;
                retrievedSubEvent.TravelTimeBefore = TravelTimeBefore;
                MyArrayOfNodes.Add(retrievedSubEvent);
                XmlNode calendarTypeNode = SubEventNode.SelectSingleNode("ThirdpartyType");
                if (calendarTypeNode != null)
                {
                    ThirdPartyControl.CalendarTool calendarType = getThirdPartyCalendarType(calendarTypeNode);
                    if (calendarType != ThirdPartyControl.CalendarTool.tiler)
                    {
                        DB_SubCalendarEvent subEvent = (retrievedSubEvent as DB_SubCalendarEvent);
                        subEvent.CalendarType = calendarType;
                    }
                }

                XmlNode autoDeletedNode = MyXmlNode.ChildNodes[i].SelectSingleNode("AutoDeleted");
                if(autoDeletedNode != null)
                {
                    retrievedSubEvent.AutoDeleted_EventDB = Convert.ToBoolean(autoDeletedNode.InnerText);
                }

                XmlNode AutoDeletionReasonNode = MyXmlNode.ChildNodes[i].SelectSingleNode("AutoDeletionReason");
                if (AutoDeletionReasonNode != null)
                {
                    retrievedSubEvent.AutoDeletion_ReasonDB = AutoDeletionReasonNode.InnerText;
                }

                XmlNode RepetitionLockNode = MyXmlNode.ChildNodes[i].SelectSingleNode("RepetitionLock");
                if (RepetitionLockNode != null)
                {
                    retrievedSubEvent.RepetitionLock_DB = Convert.ToBoolean(RepetitionLockNode.InnerText);
                }
                XmlNode NowLockNode = MyXmlNode.ChildNodes[i].SelectSingleNode("NowLock");
                if (NowLockNode != null)
                {
                    retrievedSubEvent.NowLock_DB = Convert.ToBoolean(NowLockNode.InnerText);
                }

                XmlNode LocationValidationIdNode = MyXmlNode.ChildNodes[i].SelectSingleNode("LocationValidationId");
                if (LocationValidationIdNode != null)
                {
                    retrievedSubEvent.LocationValidationId_DB = Convert.ToString(LocationValidationIdNode.InnerText);
                }

                retrievedSubEvent.InitialStartTime_DB = iniStartTimeInMs;
                retrievedSubEvent.InitialEndTime_DB = iniEndTimeInMS;

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
                foreach (XmlNode eachNode in node.ChildNodes)
                {
                    string OPtionName = eachNode.SelectSingleNode("Option").InnerText;
                    Reason generatedReason = createDictionaryOfOPtionToFunction[OPtionName](eachNode);
                    reasons.Add(generatedReason);
                }
            }

            subCalevent.updateReasons(reasons);
        }


        Tuple<TimeSpan, DateTimeOffset, List<PausedTimeLine>> getPauseData(XmlNode ReferenceNode)
        {
            Tuple<TimeSpan, DateTimeOffset, List<PausedTimeLine>> RetValue = new Tuple<TimeSpan, DateTimeOffset, List<PausedTimeLine>>(new TimeSpan(), new DateTimeOffset(), new List<PausedTimeLine>());
            XmlNode PauseInformation = ReferenceNode.SelectSingleNode("PauseInformation");
            if (PauseInformation != null)
            {
                TimeSpan UsedUpTime = TimeSpan.Parse(PauseInformation.SelectSingleNode("UsedUpTime").InnerText);
                XmlNode pauseTimeNode = PauseInformation.SelectSingleNode("PauseTime");
                DateTimeOffset PauseTime = new DateTimeOffset();
                if (pauseTimeNode != null && pauseTimeNode.InnerText.isNot_NullEmptyOrWhiteSpace())
                {
                    PauseTime = Utility.ParseTime(pauseTimeNode.InnerText);
                }
                    
                XmlNode PausedTimeSlots = PauseInformation.SelectSingleNode("PausedTimeSlots");
                List<PausedTimeLine> pausedTimeLines = new List<PausedTimeLine>();
                if (PausedTimeSlots != null)
                {
                    foreach(XmlNode timeLineNode in PausedTimeSlots.ChildNodes)
                    {
                        PausedTimeLine timeLine = pauseTimeLineNodeToTimeLine(timeLineNode);
                        if(timeLine!=null)
                        {
                            pausedTimeLines.Add(timeLine);
                        }
                        
                    }
                }
                
                RetValue = new Tuple<TimeSpan, DateTimeOffset, List<PausedTimeLine>>(UsedUpTime, PauseTime, pausedTimeLines);
            }

            return RetValue;
        }


        BusyTimeLine busyTimeLineNodeToTimeLine(XmlNode ReferenceNode)
        {
            XmlNode startNode = ReferenceNode.SelectSingleNode("Start");
            XmlNode endNode = ReferenceNode.SelectSingleNode("End");
            XmlNode idNode = ReferenceNode.SelectSingleNode("Id");

            BusyTimeLine retValue = new BusyTimeLine(idNode.InnerText, DateTimeOffset.Parse(startNode.InnerText), DateTimeOffset.Parse(endNode.InnerText));
            return retValue;
        }


        PausedTimeLine pauseTimeLineNodeToTimeLine(XmlNode ReferenceNode)
        {
            XmlNode startNode = ReferenceNode.SelectSingleNode("Start");
            XmlNode endNode = ReferenceNode.SelectSingleNode("End");
            XmlNode idNode = ReferenceNode.SelectSingleNode("Id");
            XmlNode isDeletedNode = ReferenceNode.SelectSingleNode("IsDeleted");

            PausedTimeLine retValue = null;
            if (
                idNode != null && 
                endNode != null && 
                startNode != null && 
                endNode.InnerText.isNot_NullEmptyOrWhiteSpace() &&
                startNode.InnerText.isNot_NullEmptyOrWhiteSpace())
            {
                retValue = new PausedTimeLine(idNode.InnerText, DateTimeOffset.Parse(startNode.InnerText), DateTimeOffset.Parse(endNode.InnerText));
            }

            if(retValue!= null && isDeletedNode !=null && isDeletedNode.InnerText.isNot_NullEmptyOrWhiteSpace())
            {
                retValue.IsDeleted = Convert.ToBoolean(isDeletedNode.InnerText);
            }
            
            return retValue;
        }

        TimeLine timeLineNodeToTimeLine(XmlNode ReferenceNode)
        {
            XmlNode startNode = ReferenceNode.SelectSingleNode("Start");
            XmlNode endNode = ReferenceNode.SelectSingleNode("End");
            XmlNode BusyTimeLinesNode = ReferenceNode.SelectSingleNode("BusyTimeLines");
            List<BusyTimeLine> activeSlots = new List<BusyTimeLine>();
            foreach (XmlNode eachBusyTimeLineNode in BusyTimeLinesNode) {
                BusyTimeLine busyTimeLine = busyTimeLineNodeToTimeLine(eachBusyTimeLineNode);
                activeSlots.Add(busyTimeLine);
            }


            TimeLine retValue = new TimeLine(DateTimeOffset.Parse(startNode.InnerText), DateTimeOffset.Parse(endNode.InnerText));
            retValue.AddBusySlots(activeSlots);
            return retValue;
        }

        NowProfile generateNowProfile(XmlNode ReferenceNode)
        {
            XmlNode NowProfileNode = ReferenceNode.SelectSingleNode("NowProfile");
            if (NowProfileNode == null)
            {
                return new NowProfile();
            }
            string innerText = NowProfileNode.SelectSingleNode("Initialized").InnerText;
            bool initializedFlag = string.IsNullOrEmpty(innerText) ? false : Convert.ToBoolean(NowProfileNode.SelectSingleNode("Initialized").InnerText);
            innerText = NowProfileNode.SelectSingleNode("PreferredStart").InnerText;
            DateTimeOffset preferredTime = string.IsNullOrEmpty(innerText) ? new DateTimeOffset() : Utility.ParseTime(innerText);
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
            DayOfWeek retValue = Utility.ParseEnum<DayOfWeek>(RestrictionDayOfWeek.SelectSingleNode("RestrictionDayOfWeek").InnerText);
            return retValue;
        }

        ThirdPartyControl.CalendarTool getThirdPartyCalendarType(XmlNode thirdPartyTypeNode)
        {
            ThirdPartyControl.CalendarTool retValue = Utility.ParseEnum<ThirdPartyControl.CalendarTool>(thirdPartyTypeNode.InnerText);
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
            DateTimeOffset Start = Utility.ParseTime(RestrictionTimeLineNode.SelectSingleNode("StartTime").InnerText);
            DateTimeOffset End = Utility.ParseTime(RestrictionTimeLineNode.SelectSingleNode("EndTime").InnerText);
            TimeSpan rangeSpan = TimeSpan.FromTicks(Convert.ToInt64(RestrictionTimeLineNode.SelectSingleNode("RangeSpan").InnerText));
            DB_RestrictionTimeLine retValue = new DB_RestrictionTimeLine(Start, End, rangeSpan);
            return retValue;
        }

        Repetition getRepetitionObject(XmlNode RecurrenceXmlNode)
        {
            
            if(!string.IsNullOrEmpty( RecurrenceXmlNode.InnerXml) && !string.IsNullOrEmpty(RecurrenceXmlNode.InnerXml))
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
                            repetitionNodes.Add(getRepetitionObject(eacXmlNode));
                        }

                        RetValue = new Repetition(true, new TimeLine(Utility.ParseTime(RepeatStart), Utility.ParseTime(RepeatEnd)), RepeatFrequency, repetitionNodes.ToArray(), repetitionDay);
                        return RetValue;
                    }

                }



                RetValue = new Repetition(true, new TimeLine(Utility.ParseTime(RepeatStart), Utility.ParseTime(RepeatEnd)), RepeatFrequency, getAllRepeatCalendarEvents(XmlNodeWithList), repetitionDay);

                return RetValue;
            }
            return null;
            
        }

        CalendarEvent[] getAllRepeatCalendarEvents(XmlNode RepeatEventSchedulesNode)
        {
            XmlNodeList ListOfRepeatEventScheduleNode = RepeatEventSchedulesNode.ChildNodes;
            List<CalendarEvent> ListOfRepeatCalendarNodes = new List<CalendarEvent>();

            foreach (XmlNode MyNode in ListOfRepeatEventScheduleNode)
            {
                CalendarEvent myCalendarEvent = getCalendarEventObjFromNode(MyNode);
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
            return generateLocationObjectFromNode(var1);
        }

        TilerElements.Location generateLocationObjectFromNode(XmlNode var1)
        {
            bool UninitializedLocation = false;
            if (var1 == null)
            {
                return new TilerElements.Location();
            }
            else
            {
                string ID = var1.SelectSingleNode("Id")?.InnerText?? Guid.NewGuid().ToString();
                string XCoordinate_Str = var1.SelectSingleNode("XCoordinate").InnerText;
                string YCoordinate_Str = var1.SelectSingleNode("YCoordinate").InnerText;
                string Descripion = var1.SelectSingleNode("Description").InnerText;
                string UserId = var1.SelectSingleNode("UserId")?.InnerText;
                Descripion = string.IsNullOrEmpty(Descripion) ? "" : Descripion;
                string Address = var1.SelectSingleNode("Address").InnerText;
                Address = string.IsNullOrEmpty(Address) ? "" : Address;
                UninitializedLocation = Convert.ToBoolean(var1.SelectSingleNode("isNull").InnerText);
                string CheckDefault_Str = var1.SelectSingleNode("CheckCalendarEvent") == null ? 0.ToString() : var1.SelectSingleNode("CheckCalendarEvent").InnerText;

                if (string.IsNullOrEmpty(XCoordinate_Str) || string.IsNullOrEmpty(YCoordinate_Str))
                {
                    return new TilerElements.Location(Address);
                }
                else
                {
                    double xCoOrdinate = double.MaxValue;
                    double yCoOrdinate = double.MaxValue;

                    bool isDefault;
                    if (CheckDefault_Str == "0")
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
                    }

                    if (!(double.TryParse(YCoordinate_Str, out yCoOrdinate)))
                    {
                        yCoOrdinate = double.MaxValue;
                        UninitializedLocation = true;
                    }

                    bool IsVerified = false;

                    string LocationValidation_Str = var1.SelectSingleNode("LocationValidation") == null ? "" : var1.SelectSingleNode("LocationValidation").InnerText;
                    string LookupString_Str = var1.SelectSingleNode("LookupString") == null ? "" : var1.SelectSingleNode("LookupString").InnerText;
                    string IsVerified_Str = var1.SelectSingleNode("IsVerified") == null ? "" : var1.SelectSingleNode("IsVerified").InnerText;
                    if(string.IsNullOrEmpty(IsVerified_Str) || string.IsNullOrWhiteSpace(IsVerified_Str))
                    {
                        IsVerified = !(isDefault && UninitializedLocation);
                    } else
                    {
                        IsVerified = Convert.ToBoolean(IsVerified_Str);
                    }

                    TilerElements.Location retValue = new TilerElements.Location(xCoOrdinate, yCoOrdinate, Address, Descripion, UninitializedLocation, isDefault, ID);
                    retValue.LookupString = LookupString_Str;
                    retValue.LocationValidation_DB = LocationValidation_Str;
                    retValue.UserId = UserId;
                    retValue.IsVerified = IsVerified;
                    return retValue;
                }
            }
        }
        MiscData getMiscData(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("MiscData");
            string stringData = (var1.SelectSingleNode("UserNote").InnerText);
            string innerText = var1.SelectSingleNode("TypeSelection").InnerText;
            int NoteData = Convert.ToInt32(string.IsNullOrEmpty(innerText)?"0":"1");
            MiscData retValue = new MiscData(stringData, NoteData);
            return retValue;
        }
        public EventDisplay getDisplayUINode(XmlNode Arg1)
        {
            XmlNode var1 = Arg1.SelectSingleNode("UIParams");
            int DisplayType = Convert.ToInt32(var1.SelectSingleNode("Type").InnerText);
            bool DisplayFlag = Convert.ToBoolean(var1.SelectSingleNode("Visible").InnerText);
            EventDisplay retValue;
            TilerColor colorNode = getColorNode(var1);
            if (DisplayType == 0)
            {
                retValue = new EventDisplay();
            }
            else
            {

                retValue = new EventDisplay(DisplayFlag, colorNode, DisplayType);
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

        public virtual async Task<CalendarEvent> getCalendarEventWithID(string tilerId,  HashSet<DataRetrivalOption> retrievalOptions = null)
        {
            
            return await getCalendarEventWithID(retrievalOptions, new HashSet<string>() { tilerId }).ConfigureAwait(false);
        }

        public  Tuple<IQueryable<CalendarEvent>, List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>>> getCalendarEventQueryList(
            HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> ids = null
            )
        {
            List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>> returnedQueries = new List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>>();
            IQueryable<CalendarEvent> calEventQuery = _Context.CalEvents.Where(calEvent => calEvent.CreatorId == _TilerUser.Id);
            if (ids != null && ids.Count > 0)
            {
                HashSet<string> reviseIds = new HashSet<string>( ids.Select(o=> (new EventID(o)).getRepeatCalendarEventID()));
                calEventQuery = calEventQuery
                .Join(reviseIds,
                calEvent => calEvent.Id,
                id => id,
                (calEvent, id) => new { calEvent = calEvent, id = id }).Select(obj => obj.calEvent);
            }

            bool isSubEventIncluded = retrievalOptions.Contains(DataRetrivalOption.SubEvent);
            bool isAllRetrievalOptionIncluded = retrievalOptions.Contains(DataRetrivalOption.All);
            if (isSubEventIncluded || isAllRetrievalOptionIncluded)
            {
                Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                {
                    return calEvents
                   .Include(calEvent => calEvent.AllSubEvents_DB);
                };
                returnedQueries.Add(task);
            }



            foreach (DataRetrivalOption eachRetrievalOption in retrievalOptions)
            {
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Ui)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                            .Include(calEvent => calEvent.UiParams_EventDB)
                            .Include(calEvent => calEvent.UiParams_EventDB.UIColor);
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB))
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.UiParams_EventDB.UIColor))
                            ;
                        };
                        returnedQueries.Add(subTask);
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.TimeLine)
                {
                    //calEvents = calEvents
                    //    .Include(calEvent => calEvent.StartTime_EventDB)
                    //    .Include(calEvent => calEvent.EndTime_EventDB)
                    //    ;
                    //if (isAllRetrievalOptionIncluded ||isSubEventIncluded)
                    //{
                    //    calEvents = calEvents
                    //        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.StartTime_EventDB))
                    //        .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.EndTime_EventDB))
                    //        ;
                    //}
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Evaluation)
                {

                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.RestrictionProfile_DB)
                        .Include(calEvent => calEvent.DayPreference_DB)
                        .Include(calEvent => calEvent.Procrastination_EventDB)
                        ;
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.RestrictionProfile_DB))
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB))
                            ;
                        };
                        returnedQueries.Add(subTask);
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Now)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.ProfileOfNow_EventDB)
                        ;
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB))
                            ;
                        };
                        returnedQueries.Add(subTask);
                            
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.DataBlob)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.DataBlob_EventDB)
                        ;
                    };
                    returnedQueries.Add(task);

                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB))
                            ;
                        };
                        returnedQueries.Add(subTask);
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Name)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.Name)
                        ;
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))
                            ;
                        };
                        returnedQueries.Add(subTask); 
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Location)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.Location_DB)
                        ;
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB))
                            ;
                        };
                        returnedQueries.Add(subTask);
                    }
                }
                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.TimeLineHistory)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.TimeLineHistory_DB)
                        ;
                    };
                    returnedQueries.Add(task);
                }

                if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Repetition)
                {
                    Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> task = (calEvents) =>
                    {
                        return calEvents
                        .Include(calEvent => calEvent.Repetition_EventDB)
                        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents)
                        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.SubRepetitions));
                    };
                    returnedQueries.Add(task);
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB)));
                        };
                        returnedQueries.Add(subTask);
                    }



                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Evaluation)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.DayPreference_DB))
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB)));
                        };
                        returnedQueries.Add(subTask);
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> repetitionSubTask = (calEvents) =>
                            {
                                return calEvents
                                .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB)));
                            };
                            returnedQueries.Add(repetitionSubTask);
                        }
                    }



                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Now)
                    {

                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB)));
                        };
                        returnedQueries.Add(subTask);
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> repetitionSubTask = (calEvents) =>
                            {
                                return calEvents
                                .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB)));
                            };
                            returnedQueries.Add(repetitionSubTask);
                        }
                    }

                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Name)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Name.Creator_EventDB)));
                        };
                        returnedQueries.Add(subTask);
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> repetitionSubTask = (calEvents) =>
                            {
                                return calEvents
                                    .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name)))
                                    .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB)))
                                    ;
                            };
                            returnedQueries.Add(repetitionSubTask);
                        }
                    }


                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.DataBlob)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.DataBlob_EventDB)));
                        };
                        returnedQueries.Add(subTask);
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> repetitionSubTask = (calEvents) =>
                            {
                                return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB)));
                            };
                            returnedQueries.Add(repetitionSubTask);
                        }
                    }

                    if (isAllRetrievalOptionIncluded || eachRetrievalOption == DataRetrivalOption.Location)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Location_DB)));
                        };
                        returnedQueries.Add(subTask);
                        if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                        {
                            Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> repetitionSubTask = (calEvents) =>
                            {
                                return calEvents
                            .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB)));
                            };
                            returnedQueries.Add(repetitionSubTask); 
                        }
                    }

                    ;
                    if (isAllRetrievalOptionIncluded || isSubEventIncluded)
                    {
                        Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>> subTask = (calEvents) =>
                        {
                            return calEvents
                            .Include(calEvent => calEvent.AllSubEvents_DB.Select(subEvent => subEvent.Repetition_EventDB));
                        };
                        returnedQueries.Add(subTask); 
                    }

                }
            }

            Tuple<IQueryable<CalendarEvent>, List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>>> retValue = new Tuple<IQueryable<CalendarEvent>, List<Func<IQueryable<CalendarEvent>, IQueryable<CalendarEvent>>>>(
                calEventQuery, returnedQueries
                );

            return retValue;
        }

        public async Task<CalendarEvent> getCalendarEventWithID(HashSet<DataRetrivalOption> retrievalOptions,
            HashSet<string> tilerIds = null)
        {
            string id = null;
            if(tilerIds.Count == 1)
            {
                id = tilerIds.Single();
                EventID idObj = new EventID(id);
                id = idObj.getRepeatCalendarEventID();
            }

            if (retrievalOptions == null)
            {
                retrievalOptions = DataRetrievalSet.All;
            }

            ConcurrentBag<CalendarEvent> parentCalEvents = new ConcurrentBag<CalendarEvent>();

            ConcurrentBag<CalendarEvent> childCalEvents = new ConcurrentBag<CalendarEvent>();
            CalendarEvent retValue = getCalendarEventQuery(retrievalOptions, tilerIds)
                    //.Include(calEvent => calEvent.Repetition_EventDB)
                    //.Include(calEvent => calEvent.ProfileOfNow_EventDB)
                    //.Include(calEvent => calEvent.Procrastination_EventDB)
                    //.Include(calEvent => calEvent.AllSubEvents_DB)
                    //.Include(calEvent => calEvent.DayPreference_DB)
                    //.Include(calEvent => calEvent.TimeLineHistory_DB)
                    //.Include(calEvent => calEvent.TimeLineHistory_DB.TimeLines_DB)
                    //.Include(calEvent => calEvent.DataBlob_EventDB)
                    //.Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents)
                    //.Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB))
                    //.Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB))
                    //.Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB))
                    .SingleOrDefault(calEvent => calEvent.Id == id);


            // Multiple getCalendarEventQuery calls because we're trying to leverage the caching functionality of entity framework. Attributes that have been included don't get pulled in again meaning the row size from DB is smaller.
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.DayPreference_DB)).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.ProfileOfNow_EventDB)))
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB)))
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.SubRepetitions)).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB))).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name))).SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Name.Creator_EventDB)))
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.DataBlob_EventDB)))
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Location_DB)))
            //        .SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB.Select(subEvent => subEvent.Procrastination_EventDB)))
            //        .SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.AllSubEvents_DB)))
            //        .SingleOrDefault(calEvent => calEvent.Id == id);
            //retValue = getCalendarEventQuery(retrievalOption, includeSubEvents)
            //        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.Procrastination_EventDB)))
            //        .Include(calEvent => calEvent.Repetition_EventDB.SubRepetitions.Select(repetition => repetition.RepeatingEvents.Select(repCalEvent => repCalEvent.ProfileOfNow_EventDB)))
            //        .SingleOrDefault(calEvent => calEvent.Id == id);

            if (retValue != null && retValue.getIsEventRestricted)
            {
                (retValue as CalendarEventRestricted).RestrictionProfile_DB.InitializeOverLappingDictionary();
            }
            retValue.isRepeatLoaded_DB = true;
            return retValue;
        }

        public void reloadContext()
        {
            foreach (var entity in _Context.ChangeTracker.Entries())
            {
                entity.Reload();
            }
        }
        /// <summary>
        /// Function returns a subcalendarevent  based on the provided ID. It checks if the userId is also valid.
        /// set <paramref name="includeParentCalevent"/> to false if you don't want the subcalendarevent calendar event to include the parent calendar event of the subevent
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="includeParentCalevent"></param>
        /// <param name="includeRepetition"></param>
        /// <returns></returns>
        public async Task<SubCalendarEvent> getSubEventWithID(string ID, bool includeParentCalevent = true, bool includeRepetition = true)
        {
            IQueryable<SubCalendarEvent> subEventQuery = Database.SubEvents
                .Include(subEvent => subEvent.DataBlob_EventDB)
                .Include(subEvent => subEvent.Name)
                .Include(subEvent => subEvent.Name.Creator_EventDB)
                .Include(subEvent => subEvent.Location_DB)
                .Include(subEvent => subEvent.Creator_EventDB)
                .Include(subEvent => subEvent.DataBlob_EventDB)
                .Include(subEvent => subEvent.Procrastination_EventDB)
                .Include(subEvent => subEvent.ProfileOfNow_EventDB)
                .Include(subEvent => subEvent.UiParams_EventDB.UIColor)
                .Include(subEvent => subEvent.RestrictionProfile_DB);

            if(includeParentCalevent)
            {
                subEventQuery = subEventQuery.Include(subEvent => subEvent.ParentCalendarEvent);
            }

            if(includeRepetition)
            {
                subEventQuery = subEventQuery
                    .Include(subEvent => subEvent.Repetition_EventDB)
                    .Include(subEvent => subEvent.Repetition_EventDB.RepeatingEvents);
            }


            SubCalendarEvent retValue = await subEventQuery
                .SingleOrDefaultAsync(subEvent => subEvent.CreatorId == _TilerUser.Id && subEvent.Id == ID).ConfigureAwait(false);
            if (retValue!= null && retValue.getIsEventRestricted)
            {
                (retValue as SubCalendarEventRestricted).RestrictionProfile_DB.InitializeOverLappingDictionary();
            }
            return retValue;
        }

        public async Task<IQueryable<CalendarEvent>> getEnabledCalendarEventWithName(string Name)
        {
            IQueryable<EventName> eventNames = _Context.EventNames
                .Where(name => name.CreatorId == _TilerUser.Id && name.Name.Contains(Name));
            var result = eventNames.Join(_Context.CalEvents
                .Include(CalEvent => CalEvent.UiParams_EventDB)
                .Include(CalEvent => CalEvent.DataBlob_EventDB)
                .Where(calEvent => !calEvent.IsRepeatsChildCalEvent && calEvent.IsEnabled_DB),
                eventName => eventName.Id,
                calEvent => calEvent.Name.Id,
                (eventName, calEvent) => new { calEvent = calEvent, eventName = eventName }
                );
            var res = result
                .Select(obj => obj.calEvent);

            var retValue = res
                .Include(obj => obj.DataBlob_EventDB)
                .Include(obj => obj.UiParams_EventDB)
                .Include(obj => obj.Name);
            return retValue;
        }


        public IQueryable<CalendarEvent> getCalendarEventsForUser(string userId)
        {
            IQueryable<CalendarEvent> retValue = _Context.CalEvents
                .Include(CalEvent => CalEvent.Name)
                .Include(CalEvent => CalEvent.UiParams_EventDB)
                .Include(CalEvent => CalEvent.DataBlob_EventDB)
                .Where(calEvent => calEvent.CreatorId == userId);
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

        public async virtual Task createTempDump(ReferenceNow now)
        {
            _TempDump = await CreateScheduleDump(_AllScheduleData.Values, this._TilerUser, now, "").ConfigureAwait(false);
        }


        /// <summary>
        /// This function wipes out the database completely.BECAREFUL. You''l need to comment out the lines below return
        /// </summary>
        public void deleteAllDatabaseData()
        {
            return;
            //_Context.Database.ExecuteSqlCommand("TRUNCATE TABLE Undoes");
            //_Context.Database.ExecuteSqlCommand("TRUNCATE TABLE TilerEvents");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM Procrastinations");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventNames");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Reasons");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Locations");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Repetitions");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  MiscDatas");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  TilerUserGroups");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventDisplays");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  TilerColors");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUsers");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionDays");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionTimeLines");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  RestrictionProfiles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  Classifications");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  NowProfiles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  EventTimeLines");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetRoles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserRoles");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserLogins");
            //_Context.Database.ExecuteSqlCommand("DELETE FROM  AspNetUserClaims");


        }

        public async Task<Analysis> getAnalysis(TilerUser user)
        {
            Analysis retValue = _Context.Analysis.Find(user.Id);
            if(retValue == null)
            {
                Analysis scheduleAnalysis = Analysis.generateAnalysisObject(user);
                scheduleAnalysis.setUser(user);
                _Context.Analysis.Add(scheduleAnalysis);
                await _Context.SaveChangesAsync().ConfigureAwait(false);
                retValue = scheduleAnalysis;
            }
            return retValue;
        }

        async virtual public Task<Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>, Analysis>> getProfileInfo(TimeLine RangeOfLookup, ReferenceNow Now,  HashSet<DataRetrivalOption> retrievalOptions, bool createDump = true, HashSet<string> calendarIds = null)
        {
            Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>, Analysis> retValue;
            if (this.Status)
            {
                Task<Dictionary<string, TilerElements.Location>> TaskLocationCache = getAllLocationsByUser();
                Dictionary<string, TilerElements.Location> LocationCache = await TaskLocationCache.ConfigureAwait(false);
                RangeOfLookup  = RangeOfLookup == null ? new TimeLine(Now.constNow.AddYears(-200), Now.constNow.AddYears(200)) : RangeOfLookup;
                Stopwatch scheduleLoadWatch = new Stopwatch();
                scheduleLoadWatch.Start();
                Dictionary<string, CalendarEvent> AllScheduleData = await this.getAllEnabledCalendarEvent(RangeOfLookup, Now, retrievalOptions: retrievalOptions, tilerIds: calendarIds);
                scheduleLoadWatch.Stop();
                Analysis analysis = await getAnalysis(this.getTilerRetrievedUser()).ConfigureAwait(false);
                Debug.WriteLine("Total DB Lookup took " + scheduleLoadWatch.Elapsed.ToString());
                if (createDump && _UpdateBigData)
                {
                    _AllScheduleData = AllScheduleData;
                    await createTempDump(Now).ConfigureAwait(false);
                }
                
                DateTimeOffset ReferenceTime = getDayReferenceTime();

                await populateDefaultLocation(LocationCache).ConfigureAwait(false);

                retValue = new Tuple<Dictionary<string, CalendarEvent>, DateTimeOffset, Dictionary<string, TilerElements.Location>, Analysis>(AllScheduleData, ReferenceTime, LocationCache, analysis);
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
                locations = await getAllLocationsByUser().ConfigureAwait(false);
                return;
            }

            if ((locations.Count < 1))
            {
                retValue = TilerElements.Location.getDefaultLocation();
                DefaultLocation = retValue;
                locations = await getAllLocationsByUser().ConfigureAwait(false);
                return;
            }

            TilerElements.Location AverageLocations = TilerElements.Location.AverageGPSLocation(locations.Values);

            xLocation = AverageLocations.Latitude;
            yLocation = AverageLocations.Longitude;
            TilerElements.Location.InitializeDefaultLongLat(xLocation, yLocation);
            DefaultLocation = AverageLocations;

        }
#endregion

#region Properties

        virtual public int LastUserID
        {
            get
            {
                return Convert.ToInt32(_TilerUser.LatestId);
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

        virtual public TilerDbContext Database
        {
            get
            {
                return _Context;
            }
        }

        public ReferenceNow Now { get; set; }

        #endregion

        public void disableUpdateBigData()
        {
            _UpdateBigData = false;
        }

        public void enableUpdateBigData()
        {
            _UpdateBigData = true;
        }

    }

}
