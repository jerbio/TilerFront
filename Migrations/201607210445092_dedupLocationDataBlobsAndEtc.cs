namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dedupLocationDataBlobsAndEtc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Location_Elements", "FullAddress", c => c.String());
            DropColumn("dbo.Procrastinations", "PreferredStartTime");
            DropColumn("dbo.Procrastinations", "DislikedStartTime");
            DropColumn("dbo.Procrastinations", "DislikedDaySection");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Procrastinations", "DislikedDaySection", c => c.Int(nullable: false));
            AddColumn("dbo.Procrastinations", "DislikedStartTime", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Procrastinations", "PreferredStartTime", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.Location_Elements", "FullAddress");
        }
    }
}
