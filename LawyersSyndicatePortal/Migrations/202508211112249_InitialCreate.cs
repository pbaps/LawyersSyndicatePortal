namespace LawyersSyndicatePortal.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAnswer = c.String(),
                        AcquiredScore = c.Decimal(precision: 18, scale: 2),
                        IsCorrect = c.Boolean(nullable: false),
                        IsGraded = c.Boolean(nullable: false),
                        CorrectionDate = c.DateTime(),
                        CorrectorId = c.String(),
                        QuestionId = c.Int(nullable: false),
                        ExamAttendeeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExamAttendees", t => t.ExamAttendeeId, cascadeDelete: true)
                .ForeignKey("dbo.Questions", t => t.QuestionId)
                .Index(t => t.QuestionId)
                .Index(t => t.ExamAttendeeId);
            
            CreateTable(
                "dbo.ExamAttendees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExamId = c.Int(nullable: false),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        MobileNumber = c.String(maxLength: 20),
                        CanAttend = c.Boolean(nullable: false),
                        IsExamVisible = c.Boolean(nullable: false),
                        IsCompleted = c.Boolean(nullable: false),
                        StartTime = c.DateTime(),
                        EndTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Exams", t => t.ExamId, cascadeDelete: true)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.ExamId)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.Exams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(),
                        ExamDateTime = c.DateTime(nullable: false),
                        DurationMinutes = c.Int(nullable: false),
                        ExamType = c.String(),
                        PassingScorePercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxScore = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsPublished = c.Boolean(nullable: false),
                        IsRandomized = c.Boolean(nullable: false),
                        CanRetake = c.Boolean(nullable: false),
                        RetakeDelayDays = c.Int(nullable: false),
                        IsResultVisible = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExamResults",
                c => new
                    {
                        ExamAttendeeId = c.Int(nullable: false),
                        ExamId = c.Int(nullable: false),
                        LawyerIdNumber = c.String(maxLength: 50),
                        TotalExamScore = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalScoreAchieved = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PassPercentage = c.Decimal(precision: 18, scale: 2),
                        IsPassed = c.Boolean(),
                        IsGradingComplete = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ExamAttendeeId)
                .ForeignKey("dbo.Exams", t => t.ExamId, cascadeDelete: true)
                .ForeignKey("dbo.ExamAttendees", t => t.ExamAttendeeId)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.ExamAttendeeId)
                .Index(t => t.ExamId)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.Lawyers",
                c => new
                    {
                        IdNumber = c.String(nullable: false, maxLength: 50),
                        FullName = c.String(nullable: false, maxLength: 255),
                        ProfessionalStatus = c.String(maxLength: 100),
                        IsTrainee = c.Boolean(nullable: false),
                        TrainerLawyerName = c.String(maxLength: 255),
                        MembershipNumber = c.String(maxLength: 50),
                        TrainingStartDate = c.DateTime(),
                        PracticeStartDate = c.DateTime(),
                        Gender = c.String(maxLength: 10),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdNumber);
            
            CreateTable(
                "dbo.ColleagueInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        KnowsOfMartyrColleagues = c.Boolean(nullable: false),
                        HasMartyrs = c.Boolean(nullable: false),
                        KnowsOfDetainedColleagues = c.Boolean(nullable: false),
                        HasDetained = c.Boolean(nullable: false),
                        KnowsOfInjuredColleagues = c.Boolean(nullable: false),
                        HasInjured = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.DetainedColleagues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ColleagueInfoId = c.Int(nullable: false),
                        DetainedName = c.String(maxLength: 255),
                        ContactNumber = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ColleagueInfoes", t => t.ColleagueInfoId)
                .Index(t => t.ColleagueInfoId);
            
            CreateTable(
                "dbo.InjuredColleagues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ColleagueInfoId = c.Int(nullable: false),
                        InjuredName = c.String(maxLength: 255),
                        ContactNumber = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ColleagueInfoes", t => t.ColleagueInfoId)
                .Index(t => t.ColleagueInfoId);
            
            CreateTable(
                "dbo.MartyrColleagues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ColleagueInfoId = c.Int(nullable: false),
                        MartyrName = c.String(maxLength: 255),
                        ContactNumber = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ColleagueInfoes", t => t.ColleagueInfoId)
                .Index(t => t.ColleagueInfoId);
            
            CreateTable(
                "dbo.DetentionDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        WasDetained = c.Boolean(nullable: false),
                        DetentionDuration = c.String(maxLength: 100),
                        DetentionStartDate = c.DateTime(),
                        IsStillDetained = c.Boolean(nullable: false),
                        ReleaseDate = c.DateTime(),
                        DetentionType = c.String(maxLength: 255),
                        DetentionLocation = c.String(maxLength: 500),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.FamilyDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        MaritalStatus = c.String(maxLength: 50),
                        NumberOfSpouses = c.Int(),
                        HasChildren = c.Boolean(nullable: false),
                        NumberOfChildren = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.Children",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FamilyDetailId = c.Int(nullable: false),
                        ChildName = c.String(nullable: false, maxLength: 255),
                        DateOfBirth = c.DateTime(),
                        IdNumber = c.String(maxLength: 50),
                        Gender = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FamilyDetails", t => t.FamilyDetailId)
                .Index(t => t.FamilyDetailId);
            
            CreateTable(
                "dbo.Spouses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FamilyDetailId = c.Int(nullable: false),
                        SpouseName = c.String(maxLength: 255),
                        SpouseIdNumber = c.String(maxLength: 50),
                        SpouseMobileNumber = c.String(maxLength: 20),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FamilyDetails", t => t.FamilyDetailId)
                .Index(t => t.FamilyDetailId);
            
            CreateTable(
                "dbo.GeneralInfoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        PracticesShariaLaw = c.Boolean(nullable: false),
                        ShariaLawPracticeStartDate = c.DateTime(),
                        ReceivedAidFromSyndicate = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.ReceivedAids",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GeneralInfoId = c.Int(nullable: false),
                        AidType = c.String(maxLength: 255),
                        ReceivedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GeneralInfoes", t => t.GeneralInfoId)
                .Index(t => t.GeneralInfoId);
            
            CreateTable(
                "dbo.HealthStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        LawyerCondition = c.String(maxLength: 50),
                        InjuryDetails = c.String(maxLength: 500),
                        TreatmentNeeded = c.String(maxLength: 500),
                        LawyerDiagnosis = c.String(maxLength: 1000),
                        HasFamilyMembersInjured = c.Boolean(nullable: false),
                        FamilyMembersInjured = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.FamilyMemberInjuries",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HealthStatusId = c.Int(nullable: false),
                        InjuryDetails = c.String(maxLength: 500),
                        InjuredFamilyMemberName = c.String(maxLength: 255),
                        RelationshipToLawyer = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.HealthStatus", t => t.HealthStatusId)
                .Index(t => t.HealthStatusId);
            
            CreateTable(
                "dbo.HomeDamages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        HasHomeDamage = c.Boolean(nullable: false),
                        DamageType = c.String(maxLength: 100),
                        DamageDetails = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.LawyerAttachments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        FileName = c.String(nullable: false, maxLength: 255),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        FileSize = c.Long(nullable: false),
                        ContentType = c.String(nullable: false, maxLength: 50),
                        AttachmentType = c.String(nullable: false, maxLength: 100),
                        Notes = c.String(maxLength: 1000),
                        UploadDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FullName = c.String(nullable: false, maxLength: 255),
                        IdNumber = c.String(nullable: false, maxLength: 50),
                        LinkedLawyerIdNumber = c.String(maxLength: 50),
                        CreationDate = c.DateTime(nullable: false),
                        IsLinkedToLawyer = c.Boolean(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LinkedLawyerIdNumber)
                .Index(t => t.IdNumber, unique: true)
                .Index(t => t.LinkedLawyerIdNumber)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserBroadcastReadStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        BroadcastId = c.Int(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Broadcasts", t => t.BroadcastId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => new { t.UserId, t.BroadcastId }, unique: true);
            
            CreateTable(
                "dbo.Broadcasts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderId = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(nullable: false, maxLength: 255),
                        Body = c.String(nullable: false),
                        SentDate = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.SenderId)
                .Index(t => t.SenderId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.OfficeDamages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        DamageType = c.String(maxLength: 100),
                        DamageDetails = c.String(maxLength: 1000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.OfficeDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        OfficeName = c.String(maxLength: 255),
                        OfficeAddress = c.String(maxLength: 500),
                        PropertyType = c.String(maxLength: 100),
                        PropertyStatus = c.String(maxLength: 100),
                        HasPartners = c.Boolean(nullable: false),
                        NumberOfPartners = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.Partners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OfficeDetailId = c.Int(nullable: false),
                        PartnerName = c.String(maxLength: 255),
                        PartnerMembershipNumber = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OfficeDetails", t => t.OfficeDetailId)
                .Index(t => t.OfficeDetailId);
            
            CreateTable(
                "dbo.PersonalDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LawyerIdNumber = c.String(nullable: false, maxLength: 50),
                        Gender = c.String(maxLength: 50),
                        EmailAddress = c.String(maxLength: 255),
                        OriginalGovernorate = c.String(maxLength: 100),
                        CurrentGovernorate = c.String(maxLength: 100),
                        AccommodationType = c.String(maxLength: 100),
                        FullAddress = c.String(maxLength: 500),
                        MobileNumber = c.String(maxLength: 20),
                        AltMobileNumber1 = c.String(maxLength: 20),
                        AltMobileNumber2 = c.String(maxLength: 20),
                        WhatsAppNumber = c.String(maxLength: 20),
                        LandlineNumber = c.String(maxLength: 20),
                        BankName = c.String(maxLength: 100),
                        BankBranch = c.String(maxLength: 100),
                        BankAccountNumber = c.String(maxLength: 50),
                        IBAN = c.String(maxLength: 50),
                        WalletType = c.String(maxLength: 50),
                        WalletAccountNumber = c.String(maxLength: 100),
                        DateOfBirth = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lawyers", t => t.LawyerIdNumber)
                .Index(t => t.LawyerIdNumber);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        QuestionType = c.Int(nullable: false),
                        Score = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ImagePath = c.String(),
                        CorrectAnswer = c.String(),
                        DurationSeconds = c.Int(),
                        RequiresManualGrading = c.Boolean(nullable: false),
                        ExamId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Exams", t => t.ExamId, cascadeDelete: true)
                .Index(t => t.ExamId);
            
            CreateTable(
                "dbo.QuestionOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        IsCorrect = c.Boolean(nullable: false),
                        QuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Questions", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.AuditLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminName = c.String(nullable: false),
                        ControllerName = c.String(nullable: false, maxLength: 255),
                        Timestamp = c.DateTime(nullable: false),
                        Action = c.String(nullable: false, maxLength: 255),
                        Details = c.String(),
                        TableName = c.String(maxLength: 100),
                        EntityId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ContactMessages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        Subject = c.String(nullable: false, maxLength: 255),
                        MessageBody = c.String(nullable: false),
                        SentDate = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        ReplyDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SenderId = c.String(nullable: false, maxLength: 128),
                        SenderLawyerIdNumber = c.String(maxLength: 50),
                        SenderLawyerFullName = c.String(maxLength: 255),
                        ReceiverId = c.String(nullable: false, maxLength: 128),
                        Subject = c.String(nullable: false, maxLength: 255),
                        Body = c.String(nullable: false),
                        SentDate = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                        IsAdminBroadcast = c.Boolean(nullable: false),
                        MessageType = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ReceiverId)
                .ForeignKey("dbo.AspNetUsers", t => t.SenderId)
                .Index(t => t.SenderId)
                .Index(t => t.ReceiverId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Messages", "SenderId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Messages", "ReceiverId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Answers", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Answers", "ExamAttendeeId", "dbo.ExamAttendees");
            DropForeignKey("dbo.ExamAttendees", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.ExamAttendees", "ExamId", "dbo.Exams");
            DropForeignKey("dbo.QuestionOptions", "QuestionId", "dbo.Questions");
            DropForeignKey("dbo.Questions", "ExamId", "dbo.Exams");
            DropForeignKey("dbo.ExamResults", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.PersonalDetails", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.Partners", "OfficeDetailId", "dbo.OfficeDetails");
            DropForeignKey("dbo.OfficeDetails", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.OfficeDamages", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.AspNetUsers", "LinkedLawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserBroadcastReadStatus", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserBroadcastReadStatus", "BroadcastId", "dbo.Broadcasts");
            DropForeignKey("dbo.Broadcasts", "SenderId", "dbo.AspNetUsers");
            DropForeignKey("dbo.LawyerAttachments", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.HomeDamages", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.HealthStatus", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.FamilyMemberInjuries", "HealthStatusId", "dbo.HealthStatus");
            DropForeignKey("dbo.ReceivedAids", "GeneralInfoId", "dbo.GeneralInfoes");
            DropForeignKey("dbo.GeneralInfoes", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.Spouses", "FamilyDetailId", "dbo.FamilyDetails");
            DropForeignKey("dbo.FamilyDetails", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.Children", "FamilyDetailId", "dbo.FamilyDetails");
            DropForeignKey("dbo.DetentionDetails", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.MartyrColleagues", "ColleagueInfoId", "dbo.ColleagueInfoes");
            DropForeignKey("dbo.ColleagueInfoes", "LawyerIdNumber", "dbo.Lawyers");
            DropForeignKey("dbo.InjuredColleagues", "ColleagueInfoId", "dbo.ColleagueInfoes");
            DropForeignKey("dbo.DetainedColleagues", "ColleagueInfoId", "dbo.ColleagueInfoes");
            DropForeignKey("dbo.ExamResults", "ExamAttendeeId", "dbo.ExamAttendees");
            DropForeignKey("dbo.ExamResults", "ExamId", "dbo.Exams");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.Messages", new[] { "ReceiverId" });
            DropIndex("dbo.Messages", new[] { "SenderId" });
            DropIndex("dbo.QuestionOptions", new[] { "QuestionId" });
            DropIndex("dbo.Questions", new[] { "ExamId" });
            DropIndex("dbo.PersonalDetails", new[] { "LawyerIdNumber" });
            DropIndex("dbo.Partners", new[] { "OfficeDetailId" });
            DropIndex("dbo.OfficeDetails", new[] { "LawyerIdNumber" });
            DropIndex("dbo.OfficeDamages", new[] { "LawyerIdNumber" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.Broadcasts", new[] { "SenderId" });
            DropIndex("dbo.UserBroadcastReadStatus", new[] { "UserId", "BroadcastId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "LinkedLawyerIdNumber" });
            DropIndex("dbo.AspNetUsers", new[] { "IdNumber" });
            DropIndex("dbo.LawyerAttachments", new[] { "LawyerIdNumber" });
            DropIndex("dbo.HomeDamages", new[] { "LawyerIdNumber" });
            DropIndex("dbo.FamilyMemberInjuries", new[] { "HealthStatusId" });
            DropIndex("dbo.HealthStatus", new[] { "LawyerIdNumber" });
            DropIndex("dbo.ReceivedAids", new[] { "GeneralInfoId" });
            DropIndex("dbo.GeneralInfoes", new[] { "LawyerIdNumber" });
            DropIndex("dbo.Spouses", new[] { "FamilyDetailId" });
            DropIndex("dbo.Children", new[] { "FamilyDetailId" });
            DropIndex("dbo.FamilyDetails", new[] { "LawyerIdNumber" });
            DropIndex("dbo.DetentionDetails", new[] { "LawyerIdNumber" });
            DropIndex("dbo.MartyrColleagues", new[] { "ColleagueInfoId" });
            DropIndex("dbo.InjuredColleagues", new[] { "ColleagueInfoId" });
            DropIndex("dbo.DetainedColleagues", new[] { "ColleagueInfoId" });
            DropIndex("dbo.ColleagueInfoes", new[] { "LawyerIdNumber" });
            DropIndex("dbo.ExamResults", new[] { "LawyerIdNumber" });
            DropIndex("dbo.ExamResults", new[] { "ExamId" });
            DropIndex("dbo.ExamResults", new[] { "ExamAttendeeId" });
            DropIndex("dbo.ExamAttendees", new[] { "LawyerIdNumber" });
            DropIndex("dbo.ExamAttendees", new[] { "ExamId" });
            DropIndex("dbo.Answers", new[] { "ExamAttendeeId" });
            DropIndex("dbo.Answers", new[] { "QuestionId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.Messages");
            DropTable("dbo.ContactMessages");
            DropTable("dbo.AuditLogs");
            DropTable("dbo.QuestionOptions");
            DropTable("dbo.Questions");
            DropTable("dbo.PersonalDetails");
            DropTable("dbo.Partners");
            DropTable("dbo.OfficeDetails");
            DropTable("dbo.OfficeDamages");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.Broadcasts");
            DropTable("dbo.UserBroadcastReadStatus");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.LawyerAttachments");
            DropTable("dbo.HomeDamages");
            DropTable("dbo.FamilyMemberInjuries");
            DropTable("dbo.HealthStatus");
            DropTable("dbo.ReceivedAids");
            DropTable("dbo.GeneralInfoes");
            DropTable("dbo.Spouses");
            DropTable("dbo.Children");
            DropTable("dbo.FamilyDetails");
            DropTable("dbo.DetentionDetails");
            DropTable("dbo.MartyrColleagues");
            DropTable("dbo.InjuredColleagues");
            DropTable("dbo.DetainedColleagues");
            DropTable("dbo.ColleagueInfoes");
            DropTable("dbo.Lawyers");
            DropTable("dbo.ExamResults");
            DropTable("dbo.Exams");
            DropTable("dbo.ExamAttendees");
            DropTable("dbo.Answers");
        }
    }
}
