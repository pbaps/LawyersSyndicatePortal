namespace LawyersSyndicatePortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CorrectedFinalMigration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Temp", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Temp");
        }
    }
}
