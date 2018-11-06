namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tilerUserEnhancement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "BeginningOfWeek", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "BeginningOfWeek");
        }
    }
}
