namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class restrictionProfileId : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.TilerEvents", name: "RestrictionProfile_DB_Id", newName: "RestrictionProfileId");
            RenameIndex(table: "dbo.TilerEvents", name: "IX_RestrictionProfile_DB_Id", newName: "IX_RestrictionProfileId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.TilerEvents", name: "IX_RestrictionProfileId", newName: "IX_RestrictionProfile_DB_Id");
            RenameColumn(table: "dbo.TilerEvents", name: "RestrictionProfileId", newName: "RestrictionProfile_DB_Id");
        }
    }
}
