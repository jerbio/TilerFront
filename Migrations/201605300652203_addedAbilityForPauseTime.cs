namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedAbilityForPauseTime : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PausedEvent", new[] { "UserId" });
            DropPrimaryKey("dbo.PausedEvent", "PK_dbo.PausedEvent");
            DropIndex("dbo.PausedEvent", "PK_dbo.PausedEvent");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "isPauseDeleted" }, clustered: true, name: "UserIdAndPauseStatus");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "EventId" }, name: "UserIdAndSubEventIdClustering");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            DropIndex("dbo.PausedEvent", "UserIdAndPauseStatus");
            CreateIndex("dbo.PausedEvent", "UserId");
            CreateIndex("dbo.PausedEvent", "UserId",name : "PK_dbo.PausedEvent",  clustered: true);
        }
    }
}
