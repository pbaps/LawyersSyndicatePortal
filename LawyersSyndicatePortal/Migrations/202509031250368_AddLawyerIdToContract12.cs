namespace LawyersSyndicatePortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLawyerIdToContract12 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PbaContracts", "ReceiptNumber", c => c.String());
            AddColumn("dbo.PbaContracts", "FileName", c => c.String());
            AddColumn("dbo.PbaContracts", "LawyerMembershipNumber", c => c.String());
            AddColumn("dbo.PbaContracts", "DeedType", c => c.String());
            AddColumn("dbo.PbaContracts", "FirstPartyName", c => c.String());
            AddColumn("dbo.PbaContracts", "FirstPartyId", c => c.String());
            AddColumn("dbo.PbaContracts", "SecondPartyName", c => c.String());
            AddColumn("dbo.PbaContracts", "SecondPartyId", c => c.String());
            AddColumn("dbo.PbaContracts", "ContractAuthenticationDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PbaContracts", "AuthenticatorName", c => c.String());
            AddColumn("dbo.PbaContracts", "SyndicateBranch", c => c.String());
            AlterColumn("dbo.PbaContracts", "LawyerName", c => c.String());
            AlterColumn("dbo.PbaContracts", "EmployeeName", c => c.String());
            DropColumn("dbo.PbaContracts", "ContractNumber");
            DropColumn("dbo.PbaContracts", "ContractType");
            DropColumn("dbo.PbaContracts", "ContractDate");
            DropColumn("dbo.PbaContracts", "ContractAmount");
            DropColumn("dbo.PbaContracts", "LegalServicesProvided");
            DropColumn("dbo.PbaContracts", "ContractDurationYears");
            DropColumn("dbo.PbaContracts", "ContractEndDate");
            DropColumn("dbo.PbaContracts", "ContractStatus");
            DropColumn("dbo.PbaContracts", "Notes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PbaContracts", "Notes", c => c.String());
            AddColumn("dbo.PbaContracts", "ContractStatus", c => c.String(maxLength: 100));
            AddColumn("dbo.PbaContracts", "ContractEndDate", c => c.DateTime());
            AddColumn("dbo.PbaContracts", "ContractDurationYears", c => c.Int());
            AddColumn("dbo.PbaContracts", "LegalServicesProvided", c => c.String(nullable: false));
            AddColumn("dbo.PbaContracts", "ContractAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.PbaContracts", "ContractDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.PbaContracts", "ContractType", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.PbaContracts", "ContractNumber", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.PbaContracts", "EmployeeName", c => c.String(maxLength: 255));
            AlterColumn("dbo.PbaContracts", "LawyerName", c => c.String(nullable: false, maxLength: 255));
            DropColumn("dbo.PbaContracts", "SyndicateBranch");
            DropColumn("dbo.PbaContracts", "AuthenticatorName");
            DropColumn("dbo.PbaContracts", "ContractAuthenticationDate");
            DropColumn("dbo.PbaContracts", "SecondPartyId");
            DropColumn("dbo.PbaContracts", "SecondPartyName");
            DropColumn("dbo.PbaContracts", "FirstPartyId");
            DropColumn("dbo.PbaContracts", "FirstPartyName");
            DropColumn("dbo.PbaContracts", "DeedType");
            DropColumn("dbo.PbaContracts", "LawyerMembershipNumber");
            DropColumn("dbo.PbaContracts", "FileName");
            DropColumn("dbo.PbaContracts", "ReceiptNumber");
        }
    }
}
