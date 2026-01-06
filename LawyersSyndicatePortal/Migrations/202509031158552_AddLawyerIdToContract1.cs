namespace LawyersSyndicatePortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLawyerIdToContract1 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Contracts", newName: "PbaContracts");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.PbaContracts", newName: "Contracts");
        }
    }
}
