namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init1 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.SubCalendarEvents", name: "DB_CalendarEvent_Id", newName: "CalendarEventPersist_Id");
            RenameColumn(table: "dbo.AspNetUsers", name: "DB_CalendarEvent_Id", newName: "CalendarEventPersist_Id");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_DB_CalendarEvent_Id", newName: "IX_CalendarEventPersist_Id");
            RenameIndex(table: "dbo.SubCalendarEvents", name: "IX_DB_CalendarEvent_Id", newName: "IX_CalendarEventPersist_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.SubCalendarEvents", name: "IX_CalendarEventPersist_Id", newName: "IX_DB_CalendarEvent_Id");
            RenameIndex(table: "dbo.AspNetUsers", name: "IX_CalendarEventPersist_Id", newName: "IX_DB_CalendarEvent_Id");
            RenameColumn(table: "dbo.AspNetUsers", name: "CalendarEventPersist_Id", newName: "DB_CalendarEvent_Id");
            RenameColumn(table: "dbo.SubCalendarEvents", name: "CalendarEventPersist_Id", newName: "DB_CalendarEvent_Id");
        }
    }
}
