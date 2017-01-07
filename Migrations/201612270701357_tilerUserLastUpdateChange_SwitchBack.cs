namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tilerUserLastUpdateChange_SwitchBack : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "LastChange", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "LastChange", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
    }
}
