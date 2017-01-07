namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ApplicationUserToTilerUser : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ThirdPartyCalendarAuthenticationModels");
            AddColumn("dbo.AspNetUsers", "TimeZone", c => c.String());
            AddColumn("dbo.AspNetUsers", "ClearAllId", c => c.String());
            AddColumn("dbo.AspNetUsers", "CalendarType", c => c.String());
            AlterColumn("dbo.ThirdPartyCalendarAuthenticationModels", "ProviderID", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.ThirdPartyCalendarAuthenticationModels", new[] { "TilerID", "Email", "ProviderID" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.ThirdPartyCalendarAuthenticationModels");
            AlterColumn("dbo.ThirdPartyCalendarAuthenticationModels", "ProviderID", c => c.Int(nullable: false));
            DropColumn("dbo.AspNetUsers", "CalendarType");
            DropColumn("dbo.AspNetUsers", "ClearAllId");
            DropColumn("dbo.AspNetUsers", "TimeZone");
            AddPrimaryKey("dbo.ThirdPartyCalendarAuthenticationModels", new[] { "TilerID", "Email", "ProviderID" });
        }
    }
}
