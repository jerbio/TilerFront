namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class locationUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PausedEvent", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.PausedEvent", new[] { "User_Id" });
            DropTable("dbo.PausedEvent");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PausedEvent",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        EventId = c.String(),
                        PauseTime = c.DateTimeOffset(nullable: false, precision: 7),
                        isPauseDeleted = c.Boolean(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.PausedEvent", "User_Id");
            AddForeignKey("dbo.PausedEvent", "User_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
