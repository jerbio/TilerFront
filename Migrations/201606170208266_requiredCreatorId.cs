namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class requiredCreatorId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CalendarEvents", "CreatorId", "dbo.AspNetUsers");
            DropIndex("dbo.CalendarEvents", new[] { "CreatorId" });
            AlterColumn("dbo.CalendarEvents", "CreatorId", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.SubCalendarEvents", "CreatorId", c => c.String(nullable: false));
            CreateIndex("dbo.CalendarEvents", "CreatorId");
            AddForeignKey("dbo.CalendarEvents", "CreatorId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CalendarEvents", "CreatorId", "dbo.AspNetUsers");
            DropIndex("dbo.CalendarEvents", new[] { "CreatorId" });
            AlterColumn("dbo.SubCalendarEvents", "CreatorId", c => c.String());
            AlterColumn("dbo.CalendarEvents", "CreatorId", c => c.String(maxLength: 128));
            CreateIndex("dbo.CalendarEvents", "CreatorId");
            AddForeignKey("dbo.CalendarEvents", "CreatorId", "dbo.AspNetUsers", "Id");
        }
    }
}
