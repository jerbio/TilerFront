namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class subclendarEventsAddidition : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.SubCalendarEvents", name: "CalendarEventPersist_Id", newName: "CalendarEvent_Id");
            RenameIndex(table: "dbo.SubCalendarEvents", name: "IX_CalendarEventPersist_Id", newName: "IX_CalendarEvent_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.SubCalendarEvents", name: "IX_CalendarEvent_Id", newName: "IX_CalendarEventPersist_Id");
            RenameColumn(table: "dbo.SubCalendarEvents", name: "CalendarEvent_Id", newName: "CalendarEventPersist_Id");
        }
    }
}
