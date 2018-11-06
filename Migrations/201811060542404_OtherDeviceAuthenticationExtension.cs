namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OtherDeviceAuthenticationExtension : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.OtherDeviceAuthentications", "DeviceId", c => c.String());
            //AlterColumn("dbo.OtherDeviceAuthentications", "Start", c => c.Long(nullable: false));
            //AlterColumn("dbo.OtherDeviceAuthentications", "Expiration", c => c.Long(nullable: false));
            DropTable("dbo.OtherDeviceAuthentications");
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
                    DeviceId = c.String()
                })
                .PrimaryKey(t => t.ClientId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);

        }
        
        public override void Down()
        {
            AlterColumn("dbo.OtherDeviceAuthentications", "Expiration", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.OtherDeviceAuthentications", "Start", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.OtherDeviceAuthentications", "DeviceId");
        }
    }
}
