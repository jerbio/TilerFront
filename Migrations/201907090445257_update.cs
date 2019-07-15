namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OtherDeviceAuthentications",
                c => new
                    {
                        ClientId = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(maxLength: 128),
                        Secret = c.String(),
                        Start = c.Long(nullable: false),
                        Expiration = c.Long(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Device = c.String(),
                        DeviceId = c.String(),
                    })
                .PrimaryKey(t => t.ClientId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OtherDeviceAuthentications", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.OtherDeviceAuthentications", new[] { "UserId" });
            DropTable("dbo.OtherDeviceAuthentications");
        }
    }
}
