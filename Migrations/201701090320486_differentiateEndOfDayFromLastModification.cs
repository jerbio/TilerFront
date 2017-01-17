namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class differentiateEndOfDayFromLastModification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "EndfOfDay", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.AspNetUsers", "LastScheduleModification", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.AspNetUsers", "LastChange");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "LastChange", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropColumn("dbo.AspNetUsers", "LastScheduleModification");
            DropColumn("dbo.AspNetUsers", "EndfOfDay");
        }
    }
}
