namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GoogleNotificationAuthentication1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GoogleNotificationWatchResponseModels", "expiration", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GoogleNotificationWatchResponseModels", "expiration", c => c.String());
        }
    }
}
