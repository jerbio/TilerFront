namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dedupLocationDataBlobsAndEtc2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventNames", "EventId", c => c.String());
            AddColumn("dbo.EventNames", "SecondaryEventId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.EventNames", "SecondaryEventId");
            DropColumn("dbo.EventNames", "EventId");
        }
    }
}
