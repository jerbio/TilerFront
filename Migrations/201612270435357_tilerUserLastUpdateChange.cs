namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tilerUserLastUpdateChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.AspNetUsers", "LastChange", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "LastChange", c => c.DateTime(nullable: false));
            DropColumn("dbo.AspNetUsers", "Discriminator");
        }
    }
}
