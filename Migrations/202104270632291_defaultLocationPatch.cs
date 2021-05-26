namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class defaultLocationPatch : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ThirdPartyCalendarAuthenticationModels", "DefaultLocationId", c => c.String(maxLength: 128));
            CreateIndex("dbo.ThirdPartyCalendarAuthenticationModels", "DefaultLocationId");
            AddForeignKey("dbo.ThirdPartyCalendarAuthenticationModels", "DefaultLocationId", "dbo.Locations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThirdPartyCalendarAuthenticationModels", "DefaultLocationId", "dbo.Locations");
            DropIndex("dbo.ThirdPartyCalendarAuthenticationModels", new[] { "DefaultLocationId" });
            DropColumn("dbo.ThirdPartyCalendarAuthenticationModels", "DefaultLocationId");
        }
    }
}
