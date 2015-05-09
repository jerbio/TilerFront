namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GoogleNotificationAuthentication : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ThirdPartyCalendarAuthentications", newName: "ThirdPartyCalendarAuthenticationModels");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ThirdPartyCalendarAuthenticationModels", newName: "ThirdPartyCalendarAuthentications");
        }
    }
}
