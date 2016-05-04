namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class pauseEventTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PausedEvent",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        EventId = c.String(nullable: false),
                        PauseTime = c.DateTimeOffset(nullable: false, precision: 7),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PausedEvent", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.PausedEvent", new[] { "User_Id" });
            DropTable("dbo.PausedEvent");
        }
    }
}
