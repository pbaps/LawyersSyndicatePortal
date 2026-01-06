using System;
using System.Data.Entity.Migrations;

namespace LawyersSyndicatePortal.Migrations
{
    public partial class AddPermissionsTables : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Permissions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 256),
                    Description = c.String(maxLength: 500),
                    ControllerName = c.String(nullable: false, maxLength: 256),
                    ActionName = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);

            CreateTable(
                "dbo.RolePermissions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RoleId = c.String(nullable: false, maxLength: 128),
                    PermissionId = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Permissions", t => t.PermissionId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.PermissionId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.RolePermissions", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.RolePermissions", "PermissionId", "dbo.Permissions");
            DropIndex("dbo.RolePermissions", new[] { "PermissionId" });
            DropIndex("dbo.RolePermissions", new[] { "RoleId" });
            DropIndex("dbo.Permissions", new[] { "Name" });
            DropTable("dbo.RolePermissions");
            DropTable("dbo.Permissions");
        }
    }
}
