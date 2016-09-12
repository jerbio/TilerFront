namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uniqueLocationNamePerUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Location_Elements", "CreatorId", c => c.String(maxLength: 128));
            AddColumn("dbo.Location_Elements", "NameHash", c => c.String(maxLength: 128));
            AddColumn("dbo.Location_Elements", "_User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Location_Elements", new[] { "CreatorId", "NameHash" }, name: "UserId_CacheNameHash");
            CreateIndex("dbo.Location_Elements", "_User_Id");
            AddForeignKey("dbo.Location_Elements", "_User_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Location_Elements", "CreatorId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            DropColumn("dbo.Location_Elements", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Location_Elements", "UserId", c => c.String());
            DropForeignKey("dbo.Location_Elements", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Location_Elements", "_User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Location_Elements", new[] { "_User_Id" });
            DropIndex("dbo.Location_Elements", "UserId_CacheNameHash");
            DropColumn("dbo.Location_Elements", "_User_Id");
            DropColumn("dbo.Location_Elements", "NameHash");
            DropColumn("dbo.Location_Elements", "CreatorId");
        }
    }
}
