namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CalendarEvents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ThirdPartyID = c.String(),
                        UsedTime = c.Time(nullable: false, precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Classification_Id = c.String(maxLength: 128),
                        Location_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Classifications", t => t.Classification_Id)
                .ForeignKey("dbo.Location_Elements", t => t.Location_Id)
                .Index(t => t.Classification_Id)
                .Index(t => t.Location_Id);
            
            CreateTable(
                "dbo.Classifications",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Placement = c.Int(nullable: false),
                        Succubus = c.Int(nullable: false),
                        LeisureType = c.Int(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Location_Elements",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        city = c.String(),
                        State = c.String(),
                        Country = c.String(),
                        Zip = c.String(),
                        Longitude = c.Double(),
                        Latitude = c.Double(),
                        IsDefault = c.Boolean(),
                        isNullLocation = c.Boolean(),
                        Name = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Repetition",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NowProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        hasBeenSet = c.Boolean(),
                        BestStartTime = c.DateTimeOffset(precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventNames",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MiscDatas",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        NoteData = c.String(),
                        SourceOfdata = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Procrastinations",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PreferredStartTime = c.DateTimeOffset(nullable: false, precision: 7),
                        DislikedStartTime = c.DateTimeOffset(nullable: false, precision: 7),
                        DislikedDaySection = c.Int(nullable: false),
                        UnwanteDaySection = c.Int(),
                        UndesiredStart = c.DateTimeOffset(precision: 7),
                        DesiredStart = c.DateTimeOffset(precision: 7),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SubCalendarEvents",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                        End = c.DateTimeOffset(nullable: false, precision: 7),
                        ThirdPartyID = c.String(),
                        UsedTime = c.Time(nullable: false, precision: 7),
                        CalendarEnd = c.DateTimeOffset(precision: 7),
                        CalendarStart = c.DateTimeOffset(precision: 7),
                        ConflictLevel = c.Int(),
                        CreatorId = c.String(),
                        HumaneEnd = c.DateTimeOffset(precision: 7),
                        HumaneStart = c.DateTimeOffset(precision: 7),
                        InitializingStart = c.DateTimeOffset(precision: 7),
                        isDeleted = c.Boolean(),
                        isDeletedByUser = c.Boolean(),
                        isRepeat = c.Boolean(),
                        isRigid = c.Boolean(),
                        NonHumaneEnd = c.DateTimeOffset(precision: 7),
                        NonHumaneStart = c.DateTimeOffset(precision: 7),
                        Urgency = c.Int(),
                        isDeviated = c.Boolean(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        Classification_Id = c.String(maxLength: 128),
                        Location_Id = c.String(maxLength: 128),
                        conflict_Id = c.String(maxLength: 128),
                        ProcrastinationProfile_Id = c.String(maxLength: 128),
                        Restriction_Id = c.String(maxLength: 128),
                        UIData_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Classifications", t => t.Classification_Id)
                .ForeignKey("dbo.Location_Elements", t => t.Location_Id)
                .ForeignKey("dbo.ConflictProfiles", t => t.conflict_Id)
                .ForeignKey("dbo.Procrastinations", t => t.ProcrastinationProfile_Id)
                .ForeignKey("dbo.RestrictionProfiles", t => t.Restriction_Id)
                .ForeignKey("dbo.EventDisplays", t => t.UIData_Id)
                .Index(t => t.Classification_Id)
                .Index(t => t.Location_Id)
                .Index(t => t.conflict_Id)
                .Index(t => t.ProcrastinationProfile_Id)
                .Index(t => t.Restriction_Id)
                .Index(t => t.UIData_Id);
            
            CreateTable(
                "dbo.ConflictProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        ConflictType = c.Int(nullable: false),
                        ConflictCount = c.Int(nullable: false),
                        Flag = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventDisplays",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        isVisible = c.Boolean(nullable: false),
                        isDefault = c.Int(nullable: false),
                        isCompleteUI = c.Boolean(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        UIColor_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TilerColors", t => t.UIColor_Id)
                .Index(t => t.UIColor_Id);
            
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
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(),
                        LastChange = c.DateTime(nullable: false),
                        ReferenceDay = c.DateTimeOffset(nullable: false, precision: 7),
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
                        DB_SubCalendarEventRestricted_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SubCalendarEvents", t => t.DB_SubCalendarEventRestricted_Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex")
                .Index(t => t.DB_SubCalendarEventRestricted_Id);
            
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
                "dbo.RestrictionProfiles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DB_RestrictionTimeLine",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        RestrictionProfileId = c.String(maxLength: 128),
                        WeekDay = c.Int(nullable: false),
                        Start = c.DateTimeOffset(nullable: false, precision: 7),
                        End = c.DateTimeOffset(nullable: false, precision: 7),
                        Span = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RestrictionProfiles", t => t.RestrictionProfileId)
                .Index(t => t.RestrictionProfileId);
            
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
                .PrimaryKey(t => t.EventId,clustered:false)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => new { t.UserId, t.isPauseDeleted }, clustered: true, name: "UserIdAndPauseStatus")
                .Index(t => new { t.UserId, t.EventId }, unique: true, name: "UserIdAndSubEventIdClustering");
            
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "DB_SubCalendarEventRestricted_Id", "dbo.SubCalendarEvents");
            DropForeignKey("dbo.SubCalendarEvents", "UIData_Id", "dbo.EventDisplays");
            DropForeignKey("dbo.SubCalendarEvents", "Restriction_Id", "dbo.RestrictionProfiles");
            DropForeignKey("dbo.DB_RestrictionTimeLine", "RestrictionProfileId", "dbo.RestrictionProfiles");
            DropForeignKey("dbo.SubCalendarEvents", "ProcrastinationProfile_Id", "dbo.Procrastinations");
            DropForeignKey("dbo.SubCalendarEvents", "conflict_Id", "dbo.ConflictProfiles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventDisplays", "UIColor_Id", "dbo.TilerColors");
            DropForeignKey("dbo.SubCalendarEvents", "Location_Id", "dbo.Location_Elements");
            DropForeignKey("dbo.SubCalendarEvents", "Classification_Id", "dbo.Classifications");
            DropForeignKey("dbo.CalendarEvents", "Location_Id", "dbo.Location_Elements");
            DropForeignKey("dbo.CalendarEvents", "Classification_Id", "dbo.Classifications");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            DropIndex("dbo.PausedEvent", "UserIdAndPauseStatus");
            DropIndex("dbo.DB_RestrictionTimeLine", new[] { "RestrictionProfileId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", new[] { "DB_SubCalendarEventRestricted_Id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.EventDisplays", new[] { "UIColor_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "UIData_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "Restriction_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "ProcrastinationProfile_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "conflict_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "Location_Id" });
            DropIndex("dbo.SubCalendarEvents", new[] { "Classification_Id" });
            DropIndex("dbo.CalendarEvents", new[] { "Location_Id" });
            DropIndex("dbo.CalendarEvents", new[] { "Classification_Id" });
            DropTable("dbo.ThirdPartyCalendarAuthenticationModels");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.PausedEvent");
            DropTable("dbo.GoogleNotificationWatchResponseModels");
            DropTable("dbo.DB_RestrictionTimeLine");
            DropTable("dbo.RestrictionProfiles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.TilerColors");
            DropTable("dbo.EventDisplays");
            DropTable("dbo.ConflictProfiles");
            DropTable("dbo.SubCalendarEvents");
            DropTable("dbo.Procrastinations");
            DropTable("dbo.MiscDatas");
            DropTable("dbo.EventNames");
            DropTable("dbo.NowProfiles");
            DropTable("dbo.Repetition");
            DropTable("dbo.Location_Elements");
            DropTable("dbo.Classifications");
            DropTable("dbo.CalendarEvents");
        }
    }
}
