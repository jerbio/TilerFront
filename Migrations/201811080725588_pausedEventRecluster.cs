namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pausedEventRecluster : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.PausedEvent", "UserIdAndPauseStatus");
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            AlterColumn("dbo.PausedEvent", "UserId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.PausedEvent", new[] { "UserId", "EventId" });
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "isPauseDeleted" }, name: "UserIdAndPauseStatus");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "EventId" }, unique: true, name: "UserIdAndSubEventIdClustering");
            AddForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.PausedEvent", "UserIdAndSubEventIdClustering");
            DropIndex("dbo.PausedEvent", "UserIdAndPauseStatus");
            DropPrimaryKey("dbo.PausedEvent");
            AlterColumn("dbo.PausedEvent", "UserId", c => c.String(maxLength: 128));
            AddPrimaryKey("dbo.PausedEvent", "EventId");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "EventId" }, unique: true, name: "UserIdAndSubEventIdClustering");
            CreateIndex("dbo.PausedEvent", new[] { "UserId", "isPauseDeleted" }, clustered: true, name: "UserIdAndPauseStatus");
            AddForeignKey("dbo.PausedEvent", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
