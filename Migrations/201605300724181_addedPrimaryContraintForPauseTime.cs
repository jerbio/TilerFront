namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedPrimaryContraintForPauseTime : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "EventId" }, unique: true, name: "UserIdAndSubEventIdClustering");
        }
        
        public override void Down()
        {
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "EventId" }, name: "UserIdAndSubEventIdClustering");
        }
    }
}
