namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addedPausedEventForeignKeyRelationship : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.PausedEvent", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.PausedEvent", name: "IX_User_Id", newName: "IX_UserId");
            DropPrimaryKey("dbo.PausedEvent");
            AlterColumn("dbo.PausedEvent", "EventId", c => c.String(nullable: false, maxLength: 128));
            AddPrimaryKey("dbo.PausedEvent", "EventId");
            DropColumn("dbo.PausedEvent", "Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PausedEvent", "Id", c => c.String(nullable: false, maxLength: 128));
            DropPrimaryKey("dbo.PausedEvent");
            AlterColumn("dbo.PausedEvent", "EventId", c => c.String());
            AddPrimaryKey("dbo.PausedEvent", "Id");
            RenameIndex(table: "dbo.PausedEvent", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.PausedEvent", name: "UserId", newName: "User_Id");
        }
    }
}
