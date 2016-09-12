namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uniqueLocationNamePerUserUpdate : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Location_Elements", "UserId_CacheNameHash");
            CreateIndex("dbo.Location_Elements", new[] { "CreatorId", "NameHash" }, unique: true, name: "UserId_CacheNameHash");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Location_Elements", "UserId_CacheNameHash");
            CreateIndex("dbo.Location_Elements", new[] { "CreatorId", "NameHash" }, name: "UserId_CacheNameHash");
        }
    }
}
