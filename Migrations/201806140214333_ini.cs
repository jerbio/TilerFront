namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ini : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventTimeLines",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        StartOfTimeLine = c.DateTimeOffset(nullable: false, precision: 7),
                        EndOfTimeLine = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoStartOfTimeLine = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoEndOfTimeLine = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoId = c.String(),
                        FirstInstantiation = c.Boolean(nullable: false),
                        BusyTimeSpan_DB = c.Time(precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TilerEvents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ThirdPartyID = c.String(),
                        TimeCreated = c.DateTimeOffset(nullable: false, precision: 7),
                        LocationId = c.String(maxLength: 128),
                        UiParamsId = c.String(maxLength: 128),
                        StartTime_EventDB = c.DateTimeOffset(nullable: false, precision: 7),
                        EndTime_EventDB = c.DateTimeOffset(nullable: false, precision: 7),
                        Complete_EventDB = c.Boolean(nullable: false),
                        UserDeleted_EventDB = c.Boolean(nullable: false),
                        DataBlobId = c.String(maxLength: 128),
                        EventRepetitionId = c.String(maxLength: 128),
                        Duration_EventDB = c.Time(nullable: false, precision: 7),
                        otherPartyID_EventDB = c.String(),
                        PreDeadline_EventDB = c.Time(nullable: false, precision: 7),
                        Preptime_EventDB = c.Time(nullable: false, precision: 7),
                        RigidSchedule_EventDB = c.Boolean(nullable: false),
                        Priority_EventDB = c.Int(nullable: false),
                        isRestricted_EventDB = c.Boolean(nullable: false),
                        ProcrastinationId = c.String(maxLength: 128),
                        ThirdPartyFlag_EventDB = c.Boolean(nullable: false),
                        ThirdPartyTypeInfo_EventDB = c.String(),
                        CreatorId = c.String(maxLength: 128),
                        UsedTime_EventDB = c.Time(nullable: false, precision: 7),
                        SemanticsId = c.String(maxLength: 128),
                        TilerUserGroupId = c.String(maxLength: 128),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        TravelTimeBefore = c.Time(precision: 7),
                        TravelTimeAfter = c.Time(precision: 7),
                        isWake = c.Boolean(),
                        isSleep = c.Boolean(),
                        CalendarEventId = c.String(maxLength: 128),
                        CalendarId = c.String(maxLength: 128),
                        HardRangeStartTime_EventDB = c.DateTimeOffset(precision: 7),
                        HardRangeEndTime_EventDB = c.DateTimeOffset(precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Repetition_Id = c.String(maxLength: 128),
                        Repetition_Id1 = c.String(maxLength: 128),
                        ActiveSlot_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .ForeignKey("dbo.MiscDatas", t => t.DataBlobId)
                .ForeignKey("dbo.Locations", t => t.LocationId)
                .ForeignKey("dbo.Procrastinations", t => t.ProcrastinationId)
                .ForeignKey("dbo.Repetitions", t => t.Repetition_Id)
                .ForeignKey("dbo.Repetitions", t => t.Repetition_Id1)
                .ForeignKey("dbo.Repetitions", t => t.EventRepetitionId)
                .ForeignKey("dbo.Classifications", t => t.SemanticsId)
                .ForeignKey("dbo.EventDisplays", t => t.UiParamsId)
                .ForeignKey("dbo.TilerUserGroups", t => t.TilerUserGroupId)
                .ForeignKey("dbo.EventTimeLines", t => t.ActiveSlot_Id)
                .ForeignKey("dbo.TilerEvents", t => t.CalendarId)
                .ForeignKey("dbo.TilerEvents", t => t.CalendarEventId)
                .Index(t => t.LocationId)
                .Index(t => t.UiParamsId)
                .Index(t => t.DataBlobId)
                .Index(t => t.EventRepetitionId)
                .Index(t => t.ProcrastinationId)
                .Index(t => t.CreatorId)
                .Index(t => t.SemanticsId)
                .Index(t => t.TilerUserGroupId)
                .Index(t => t.CalendarEventId)
                .Index(t => t.CalendarId)
                .Index(t => t.Repetition_Id)
                .Index(t => t.Repetition_Id1)
                .Index(t => t.ActiveSlot_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(),
                        TimeZone = c.String(),
                        EndfOfDay = c.DateTimeOffset(nullable: false, precision: 7),
                        LastScheduleModification = c.DateTimeOffset(nullable: false, precision: 7),
                        ClearAllId = c.String(),
                        LatestId = c.String(),
                        CalendarType = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        TilerUserGroup_id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerUserGroups", t => t.TilerUserGroup_id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.TilerUserGroup_id);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.MiscDatas",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserNote = c.String(),
                        Type = c.Int(nullable: false),
                        UserTypedData = c.String(),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        UndoType = c.Int(nullable: false),
                        UndoUserTypedData = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Locations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Description = c.String(),
                        Address = c.String(),
                        Latitude = c.Double(nullable: false),
                        Longitude = c.Double(nullable: false),
                        isNull = c.Boolean(nullable: false),
                        isDefault = c.Boolean(nullable: false),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        UndoLatitude = c.Double(nullable: false),
                        UndoLongitude = c.Double(nullable: false),
                        UndoTaggedDescription = c.String(),
                        UndoTaggedAddress = c.String(),
                        UndoNullLocation = c.Boolean(nullable: false),
                        UndoDefaultFlag = c.Boolean(nullable: false),
                        LocationReason_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reasons", t => t.LocationReason_Id)
                .Index(t => t.LocationReason_Id);
            
            CreateTable(
                "dbo.EventNames",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstInstantiation = c.Boolean(nullable: false),
                        CreatorId = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerEvents", t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.CreatorId);
            
            CreateTable(
                "dbo.Procrastinations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UndoFromTime = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoBeginTime = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoSectionOfDay = c.Int(nullable: false),
                        FromTime = c.DateTimeOffset(nullable: false, precision: 7),
                        BeginTIme = c.DateTimeOffset(nullable: false, precision: 7),
                        SectionOfDay = c.Int(nullable: false),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        UndoAssociatedEvent_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerEvents", t => t.Id)
                .ForeignKey("dbo.TilerEvents", t => t.UndoAssociatedEvent_Id)
                .Index(t => t.Id)
                .Index(t => t.UndoAssociatedEvent_Id);
            
            CreateTable(
                "dbo.NowProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PreferredTime = c.DateTimeOffset(nullable: false, precision: 7),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerEvents", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Repetitions",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UndoInitializingRangeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoInitializingRangeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoRepetitionRangeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoRepetitionRangeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        UndoRepetitionFrequency = c.String(),
                        UndoEnableRepeat = c.Boolean(nullable: false),
                        UndoRepetitionWeekDay = c.Int(nullable: false),
                        RepetitionFrequency = c.String(),
                        RepetitionRangeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        RepetitionRangeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        initializingRangeStart = c.DateTimeOffset(nullable: false, precision: 7),
                        initializingRangeEnd = c.DateTimeOffset(nullable: false, precision: 7),
                        EnableRepeat = c.Boolean(nullable: false),
                        LocationId = c.String(maxLength: 128),
                        ParentRepetitionId = c.String(maxLength: 128),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        Repetition_Id = c.String(maxLength: 128),
                        Repetition_Id1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Repetitions", t => t.Repetition_Id)
                .ForeignKey("dbo.Locations", t => t.LocationId)
                .ForeignKey("dbo.TilerEvents", t => t.Id)
                .ForeignKey("dbo.Repetitions", t => t.ParentRepetitionId)
                .ForeignKey("dbo.Repetitions", t => t.Repetition_Id1)
                .Index(t => t.Id)
                .Index(t => t.LocationId)
                .Index(t => t.ParentRepetitionId)
                .Index(t => t.Repetition_Id)
                .Index(t => t.Repetition_Id1);
            
            CreateTable(
                "dbo.Classifications",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Initialized = c.Boolean(nullable: false),
                        Placement = c.String(),
                        Succubus = c.String(),
                        LeisureType = c.String(),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        UndoPlacement = c.String(),
                        UndoSuccubus = c.String(),
                        UndoLeisureType = c.String(),
                        UndoInitialized = c.Boolean(nullable: false),
                        _AssociatedEvent_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerEvents", t => t._AssociatedEvent_Id)
                .ForeignKey("dbo.TilerEvents", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t._AssociatedEvent_Id);
            
            CreateTable(
                "dbo.EventDisplays",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ColorId = c.String(maxLength: 128),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerColors", t => t.ColorId)
                .Index(t => t.ColorId);
            
            CreateTable(
                "dbo.TilerColors",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        R = c.Int(nullable: false),
                        G = c.Int(nullable: false),
                        B = c.Int(nullable: false),
                        O = c.Double(nullable: false),
                        User = c.Int(nullable: false),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TilerUserGroups",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.GoogleNotificationWatchResponseModels",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        kind = c.String(),
                        resourceId = c.String(),
                        resourceUri = c.String(),
                        token = c.String(),
                        expiration = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.PausedEvent",
                c => new
                    {
                        UserId = c.String(maxLength: 128),
                        EventId = c.String(nullable: false, maxLength: 128),
                        PauseTime = c.DateTimeOffset(nullable: false, precision: 7),
                        isPauseDeleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EventId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.UserId, t.isPauseDeleted }, name: "UserIdAndPauseStatus")
                .Index(t => new { t.UserId, t.EventId }, unique: true, name: "UserIdAndSubEventIdClustering");
            
            CreateTable(
                "dbo.Reasons",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Option_DB = c.String(),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        UsedUp_DB = c.Time(precision: 7),
                        Available_DB = c.Time(precision: 7),
                        CurrentUse_DB = c.Time(precision: 7),
                        UsedUp = c.String(),
                        Available = c.String(),
                        CurrentUse = c.String(),
                        Deadline = c.DateTimeOffset(precision: 7),
                        Duration = c.Time(precision: 7),
                        IdOrders = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RestrictionDays",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        DayOfWeek = c.String(),
                        RestrictionTimeLineId = c.String(maxLength: 128),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                        RestrictionProfile_Id = c.String(maxLength: 128),
                        RestrictionProfile_Id1 = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RestrictionTimeLines", t => t.RestrictionTimeLineId)
                .ForeignKey("dbo.RestrictionProfiles", t => t.RestrictionProfile_Id)
                .ForeignKey("dbo.RestrictionProfiles", t => t.RestrictionProfile_Id1)
                .Index(t => t.RestrictionTimeLineId)
                .Index(t => t.RestrictionProfile_Id)
                .Index(t => t.RestrictionProfile_Id1);
            
            CreateTable(
                "dbo.RestrictionTimeLines",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                        Span = c.Time(nullable: false, precision: 7),
                        End = c.DateTimeOffset(nullable: false, precision: 7),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RestrictionProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        StartDayOfWeek = c.String(),
                        FirstInstantiation = c.Boolean(nullable: false),
                        UndoId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ThirdPartyCalendarAuthenticationModels",
                c => new
                    {
                        TilerID = c.String(nullable: false, maxLength: 128),
                        Email = c.String(nullable: false, maxLength: 128),
                        ProviderID = c.String(nullable: false, maxLength: 128),
                        ID = c.String(),
                        isLongLived = c.Boolean(nullable: false),
                        Token = c.String(),
                        RefreshToken = c.String(),
                        Deadline = c.DateTimeOffset(nullable: false, precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.TilerID, t.Email, t.ProviderID });
            
            CreateTable(
                "dbo.Undoes",
                c => new
                    {
                        id = c.String(nullable: false, maxLength: 128),
                        userId = c.String(maxLength: 128),
                        activeId = c.String(),
                        lastUndoId = c.String(),
                        creationTime = c.DateTimeOffset(nullable: false, precision: 7),
                        lastUndoTime = c.DateTimeOffset(nullable: false, precision: 7),
                        LastAction = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.AspNetUsers", t => t.userId)
                .Index(t => t.userId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Undoes", "userId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RestrictionDays", "RestrictionProfile_Id1", "dbo.RestrictionProfiles");
            DropForeignKey("dbo.RestrictionDays", "RestrictionProfile_Id", "dbo.RestrictionProfiles");
            DropForeignKey("dbo.RestrictionDays", "RestrictionTimeLineId", "dbo.RestrictionTimeLines");
            DropForeignKey("dbo.Locations", "LocationReason_Id", "dbo.Reasons");
            DropForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventNames", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TilerEvents", "CalendarEventId", "dbo.TilerEvents");
            DropForeignKey("dbo.TilerEvents", "CalendarId", "dbo.TilerEvents");
            DropForeignKey("dbo.TilerEvents", "ActiveSlot_Id", "dbo.EventTimeLines");
            DropForeignKey("dbo.TilerEvents", "TilerUserGroupId", "dbo.TilerUserGroups");
            DropForeignKey("dbo.AspNetUsers", "TilerUserGroup_id", "dbo.TilerUserGroups");
            DropForeignKey("dbo.TilerEvents", "UiParamsId", "dbo.EventDisplays");
            DropForeignKey("dbo.EventDisplays", "ColorId", "dbo.TilerColors");
            DropForeignKey("dbo.TilerEvents", "SemanticsId", "dbo.Classifications");
            DropForeignKey("dbo.Classifications", "Id", "dbo.TilerEvents");
            DropForeignKey("dbo.Classifications", "_AssociatedEvent_Id", "dbo.TilerEvents");
            DropForeignKey("dbo.TilerEvents", "EventRepetitionId", "dbo.Repetitions");
            DropForeignKey("dbo.Repetitions", "Repetition_Id1", "dbo.Repetitions");
            DropForeignKey("dbo.TilerEvents", "Repetition_Id1", "dbo.Repetitions");
            DropForeignKey("dbo.Repetitions", "ParentRepetitionId", "dbo.Repetitions");
            DropForeignKey("dbo.Repetitions", "Id", "dbo.TilerEvents");
            DropForeignKey("dbo.Repetitions", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.Repetitions", "Repetition_Id", "dbo.Repetitions");
            DropForeignKey("dbo.TilerEvents", "Repetition_Id", "dbo.Repetitions");
            DropForeignKey("dbo.NowProfiles", "Id", "dbo.TilerEvents");
            DropForeignKey("dbo.TilerEvents", "ProcrastinationId", "dbo.Procrastinations");
            DropForeignKey("dbo.Procrastinations", "UndoAssociatedEvent_Id", "dbo.TilerEvents");
            DropForeignKey("dbo.Procrastinations", "Id", "dbo.TilerEvents");
            DropForeignKey("dbo.EventNames", "Id", "dbo.TilerEvents");
            DropForeignKey("dbo.TilerEvents", "LocationId", "dbo.Locations");
            DropForeignKey("dbo.TilerEvents", "DataBlobId", "dbo.MiscDatas");
            DropForeignKey("dbo.TilerEvents", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Undoes", new[] { "userId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.RestrictionDays", new[] { "RestrictionProfile_Id1" });
            DropIndex("dbo.RestrictionDays", new[] { "RestrictionProfile_Id" });
            DropIndex("dbo.RestrictionDays", new[] { "RestrictionTimeLineId" });
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            DropIndex("dbo.PausedEvent", "UserIdAndPauseStatus");
            DropIndex("dbo.EventDisplays", new[] { "ColorId" });
            DropIndex("dbo.Classifications", new[] { "_AssociatedEvent_Id" });
            DropIndex("dbo.Classifications", new[] { "Id" });
            DropIndex("dbo.Repetitions", new[] { "Repetition_Id1" });
            DropIndex("dbo.Repetitions", new[] { "Repetition_Id" });
            DropIndex("dbo.Repetitions", new[] { "ParentRepetitionId" });
            DropIndex("dbo.Repetitions", new[] { "LocationId" });
            DropIndex("dbo.Repetitions", new[] { "Id" });
            DropIndex("dbo.NowProfiles", new[] { "Id" });
            DropIndex("dbo.Procrastinations", new[] { "UndoAssociatedEvent_Id" });
            DropIndex("dbo.Procrastinations", new[] { "Id" });
            DropIndex("dbo.EventNames", new[] { "CreatorId" });
            DropIndex("dbo.EventNames", new[] { "Id" });
            DropIndex("dbo.Locations", new[] { "LocationReason_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", new[] { "TilerUserGroup_id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.TilerEvents", new[] { "ActiveSlot_Id" });
            DropIndex("dbo.TilerEvents", new[] { "Repetition_Id1" });
            DropIndex("dbo.TilerEvents", new[] { "Repetition_Id" });
            DropIndex("dbo.TilerEvents", new[] { "CalendarId" });
            DropIndex("dbo.TilerEvents", new[] { "CalendarEventId" });
            DropIndex("dbo.TilerEvents", new[] { "TilerUserGroupId" });
            DropIndex("dbo.TilerEvents", new[] { "SemanticsId" });
            DropIndex("dbo.TilerEvents", new[] { "CreatorId" });
            DropIndex("dbo.TilerEvents", new[] { "ProcrastinationId" });
            DropIndex("dbo.TilerEvents", new[] { "EventRepetitionId" });
            DropIndex("dbo.TilerEvents", new[] { "DataBlobId" });
            DropIndex("dbo.TilerEvents", new[] { "UiParamsId" });
            DropIndex("dbo.TilerEvents", new[] { "LocationId" });
            DropTable("dbo.Undoes");
            DropTable("dbo.ThirdPartyCalendarAuthenticationModels");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.RestrictionProfiles");
            DropTable("dbo.RestrictionTimeLines");
            DropTable("dbo.RestrictionDays");
            DropTable("dbo.Reasons");
            DropTable("dbo.PausedEvent");
            DropTable("dbo.GoogleNotificationWatchResponseModels");
            DropTable("dbo.TilerUserGroups");
            DropTable("dbo.TilerColors");
            DropTable("dbo.EventDisplays");
            DropTable("dbo.Classifications");
            DropTable("dbo.Repetitions");
            DropTable("dbo.NowProfiles");
            DropTable("dbo.Procrastinations");
            DropTable("dbo.EventNames");
            DropTable("dbo.Locations");
            DropTable("dbo.MiscDatas");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.TilerEvents");
            DropTable("dbo.EventTimeLines");
        }
    }
}
