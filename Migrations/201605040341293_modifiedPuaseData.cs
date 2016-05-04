namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class modifiedPuaseData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PausedEvent", "isPauseDeleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PausedEvent", "isPauseDeleted");
        }
    }
}
