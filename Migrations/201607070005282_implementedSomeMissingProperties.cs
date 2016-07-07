namespace TilerFront.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class implementedSomeMissingProperties : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.EventDisplays", name: "UIColor_Id", newName: "Color_Id");
            RenameIndex(table: "dbo.EventDisplays", name: "IX_UIColor_Id", newName: "IX_Color_Id");
            AddColumn("dbo.EventNames", "Name", c => c.String());
            AddColumn("dbo.ConflictProfiles", "Type", c => c.Int());
            AddColumn("dbo.ConflictProfiles", "Count", c => c.Int());
            AddColumn("dbo.ConflictProfiles", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.TilerColors", "UserColorSelection", c => c.Int(nullable: false));
            AlterColumn("dbo.ConflictProfiles", "Flag", c => c.Boolean());
            AlterColumn("dbo.EventDisplays", "isVisible", c => c.Boolean());
            AlterColumn("dbo.EventDisplays", "isDefault", c => c.Int());
            AlterColumn("dbo.EventDisplays", "isCompleteUI", c => c.Boolean());
            DropColumn("dbo.ConflictProfiles", "ConflictType");
            DropColumn("dbo.ConflictProfiles", "ConflictCount");
            DropColumn("dbo.TilerColors", "User");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TilerColors", "User", c => c.Int(nullable: false));
            AddColumn("dbo.ConflictProfiles", "ConflictCount", c => c.Int(nullable: false));
            AddColumn("dbo.ConflictProfiles", "ConflictType", c => c.Int(nullable: false));
            AlterColumn("dbo.EventDisplays", "isCompleteUI", c => c.Boolean(nullable: false));
            AlterColumn("dbo.EventDisplays", "isDefault", c => c.Int(nullable: false));
            AlterColumn("dbo.EventDisplays", "isVisible", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ConflictProfiles", "Flag", c => c.Boolean(nullable: false));
            DropColumn("dbo.TilerColors", "UserColorSelection");
            DropColumn("dbo.ConflictProfiles", "Discriminator");
            DropColumn("dbo.ConflictProfiles", "Count");
            DropColumn("dbo.ConflictProfiles", "Type");
            DropColumn("dbo.EventNames", "Name");
            RenameIndex(table: "dbo.EventDisplays", name: "IX_Color_Id", newName: "IX_UIColor_Id");
            RenameColumn(table: "dbo.EventDisplays", name: "Color_Id", newName: "UIColor_Id");
        }
    }
}
