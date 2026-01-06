using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // لإضافة [Index] و [ForeignKey]
using System;
using System.Collections.Generic; // تأكد من إضافة مساحة الاسم هذه لتتمكن من الوصول إلى ICollection
// تأكد من إضافة مساحة الاسم هذه لتتمكن من الوصول إلى فئة Lawyer والنماذج الأخرى
using LawyersSyndicatePortal.Models; // مهم جداً!
using System.Linq; // تم إضافة مساحة الاسم هذه لتتمكن من استخدام LINQ

namespace LawyersSyndicatePortal.Models
{
    // يمكنك إضافة بيانات ملف تعريف للمستخدم عن طريق إضافة خصائص أخرى إلى فئة ApplicationUser
    public class ApplicationUser : IdentityUser
    {
        // الخصائص الإضافية لجدول المستخدمين
        [Required]
        [StringLength(255)]
        [Display(Name = "الاسم الرباعي")]
        public string FullName { get; set; }

        // رقم الهوية (ليس Primary Key لجدول AspNetUsers، بل حقل فريد)
        [Required]
        [StringLength(50)]
        [Index(IsUnique = true)] // يضمن أن رقم الهوية لا يتكرر بين المستخدمين
        [Display(Name = "رقم الهوية")]
        public string IdNumber { get; set; }

        // خاصية رقم هوية المحامي المرتبط (المفتاح الخارجي لجدول Lawyer)
        // يمكن أن تكون null إذا لم يكن المستخدم مرتبطاً بمحامٍ
        [StringLength(50)] // تم التعديل ليتطابق مع طول المفتاح الأساسي في Lawyer (Lawyer.IdNumber)
        [Display(Name = "رقم هوية المحامي المرتبط")]
        public string LinkedLawyerIdNumber { get; set; }

        // خاصية التنقل للمحامي المرتبط
        [ForeignKey("LinkedLawyerIdNumber")] // يربط هذا الحقل بالمفتاح الأساسي في جدول Lawyer
        public virtual Lawyer LinkedLawyer { get; set; }

        // تاريخ إنشاء الحساب
        public DateTime CreationDate { get; set; } = DateTime.Now;

        // NEW: Property to indicate if the user is linked to a lawyer
        // جديد: خاصية للإشارة إلى ما إذا كان المستخدم مرتبطًا بمحامٍ
        [Display(Name = "هل المستخدم مرتبط بمحامٍ؟")]
        public bool IsLinkedToLawyer { get; set; }

        // NEW: خاصية التنقل لحالة قراءة التعميمات لكل مستخدم
        public virtual ICollection<UserBroadcastReadStatus> BroadcastReadStatuses { get; set; }

        public int Temp { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // لاحظ أن authenticationType يجب أن يطابق AuthenticationType المحدد في CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // أضف مطالبات المستخدم المخصصة هنا
            userIdentity.AddClaim(new Claim("FullName", this.FullName));
            userIdentity.AddClaim(new Claim("IdNumber", this.IdNumber));

            // إضافة رقم هوية المحامي إذا كان موجوداً
            if (!string.IsNullOrEmpty(this.LinkedLawyerIdNumber))
            {
                userIdentity.AddClaim(new Claim("LinkedLawyerIdNumber", this.LinkedLawyerIdNumber));
            }

            // NEW: إضافة جميع الصلاحيات الممنوحة للمستخدم كـ Claims
            using (var db = new ApplicationDbContext())
            {
                // جلب معرفات أدوار المستخدم
                var userRoleIds = this.Roles.Select(r => r.RoleId).ToList();

                // التحقق من وجود دور "Admin" باستخدام Claims
                if (userIdentity.HasClaim(ClaimTypes.Role, "Admin"))
                {
                    var allPermissions = db.Permissions.Select(p => p.Name).ToList();
                    foreach (var permission in allPermissions)
                    {
                        userIdentity.AddClaim(new Claim("Permission", permission));
                    }
                }
                else // إذا لم يكن المستخدم "Admin"، أضف الصلاحيات بناءً على أدواره
                {
                    // جلب الصلاحيات المرتبطة بأدوار المستخدم
                    var permissions = db.RolePermissions
            .Where(rp => userRoleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

                    // إضافة كل صلاحية كـ Claim
                    foreach (var permission in permissions)
                    {
                        userIdentity.AddClaim(new Claim("Permission", permission));
                    }
                }
            }

            return userIdentity;
        }

        public ApplicationUser()
        {
            // تهيئة مجموعات التنقل
            BroadcastReadStatuses = new HashSet<UserBroadcastReadStatus>(); // تهيئة المجموعة الجديدة
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
         : base("DefaultConnection", throwIfV1Schema: false) // "DefaultConnection" هو اسم connection string في Web.config
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        // DbSets للموديلات المطلوبة
        public DbSet<Lawyer> Lawyers { get; set; }
        public DbSet<PersonalDetail> PersonalDetails { get; set; }
        public DbSet<FamilyDetail> FamilyDetails { get; set; }
        public DbSet<HealthStatus> HealthStatuses { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Spouse> Spouses { get; set; }
        public DbSet<FamilyMemberInjury> FamilyMemberInjuries { get; set; }
        public DbSet<OfficeDetail> OfficeDetails { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<HomeDamage> HomeDamages { get; set; }
        public DbSet<OfficeDamage> OfficeDamages { get; set; }
        public DbSet<DetentionDetail> DetentionDetails { get; set; }
        public DbSet<ColleagueInfo> ColleagueInfos { get; set; }
        public DbSet<MartyrColleague> MartyrColleagues { get; set; }
        public DbSet<DetainedColleague> DetainedColleagues { get; set; }
        public DbSet<InjuredColleague> InjuredColleagues { get; set; }
        public DbSet<GeneralInfo> GeneralInfos { get; set; }
        public DbSet<ReceivedAid> ReceivedAids { get; set; }
        public DbSet<LawyerAttachment> LawyerAttachments { get; set; } // NEW: DbSet للمرفقات
        public DbSet<Message> Messages { get; set; } // NEW: DbSet لنموذج الرسائل
        public DbSet<Broadcast> Broadcasts { get; set; } // NEW: DbSet لنموذج التعميمات
        // NEW: إضافة DbSet لنموذج حالة قراءة التعميمات لكل مستخدم
        public DbSet<UserBroadcastReadStatus> UserBroadcastReadStatuses { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; } // DbSet لنموذج ContactMessage
                                                                   // جديد: إضافة DbSet لنموذج سجل التحديث
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<PbaContract> PbaContracts { get; set; }

        // NEW: إضافة DbSets لنماذج الاختبارات
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamAttendee> ExamAttendees { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // يجب أن تكون هذه في البداية لتطبيق تكوينات Identity Framework أولاً
            base.OnModelCreating(modelBuilder);

            // تكوين العلاقة بين ApplicationUser و Lawyer
            modelBuilder.Entity<ApplicationUser>()
    .HasOptional(u => u.LinkedLawyer) // المستخدم قد لا يكون مرتبطاً بمحامٍ (خاصية التنقل في ApplicationUser)
                .WithMany(l => l.LinkedUsers) // المحامي الواحد يمكن أن يرتبط به عدة مستخدمين (خاصية التنقل في Lawyer)
                .HasForeignKey(u => u.LinkedLawyerIdNumber); // تحديد المفتاح الخارجي في جدول AspNetUsers

            // تكوين العلاقة بين Lawyer و PersonalDetail
            modelBuilder.Entity<PersonalDetail>()
    .HasRequired(pd => pd.Lawyer)
    .WithMany(l => l.PersonalDetails)
    .HasForeignKey(pd => pd.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و FamilyDetail
            modelBuilder.Entity<FamilyDetail>()
    .HasRequired(fd => fd.Lawyer)
    .WithMany(l => l.FamilyDetails)
    .HasForeignKey(fd => fd.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و HealthStatus
            modelBuilder.Entity<HealthStatus>()
    .HasRequired(hs => hs.Lawyer)
    .WithMany(l => l.HealthStatuses)
    .HasForeignKey(hs => hs.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين FamilyDetail و Child
            modelBuilder.Entity<Child>()
    .HasRequired(c => c.FamilyDetail)
    .WithMany(fd => fd.Children)
    .HasForeignKey(c => c.FamilyDetailId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين FamilyDetail و Spouse
            modelBuilder.Entity<Spouse>()
    .HasRequired(s => s.FamilyDetail)
    .WithMany(fd => fd.Spouses)
    .HasForeignKey(s => s.FamilyDetailId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين HealthStatus و FamilyMemberInjury
            modelBuilder.Entity<FamilyMemberInjury>()
    .HasRequired(fmi => fmi.HealthStatus)
    .WithMany(hs => hs.FamilyMemberInjuries)
    .HasForeignKey(fmi => fmi.HealthStatusId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و OfficeDetail
            modelBuilder.Entity<OfficeDetail>()
    .HasRequired(od => od.Lawyer)
    .WithMany(l => l.OfficeDetails)
    .HasForeignKey(od => od.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين OfficeDetail و Partner
            modelBuilder.Entity<Partner>()
    .HasRequired(p => p.OfficeDetail)
    .WithMany(od => od.Partners)
    .HasForeignKey(p => p.OfficeDetailId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و HomeDamage
            modelBuilder.Entity<HomeDamage>()
    .HasRequired(hd => hd.Lawyer)
    .WithMany(l => l.HomeDamages)
    .HasForeignKey(hd => hd.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و OfficeDamage
            modelBuilder.Entity<OfficeDamage>()
    .HasRequired(od => od.Lawyer)
    .WithMany(l => l.OfficeDamages)
    .HasForeignKey(od => od.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و DetentionDetail
            modelBuilder.Entity<DetentionDetail>()
    .HasRequired(dd => dd.Lawyer)
    .WithMany(l => l.DetentionDetails)
    .HasForeignKey(dd => dd.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و ColleagueInfo
            modelBuilder.Entity<ColleagueInfo>()
    .HasRequired(ci => ci.Lawyer)
    .WithMany(l => l.ColleagueInfos)
    .HasForeignKey(ci => ci.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين ColleagueInfo و MartyrColleague
            modelBuilder.Entity<MartyrColleague>()
    .HasRequired(mc => mc.ColleagueInfo)
    .WithMany(ci => ci.MartyrColleagues)
    .HasForeignKey(mc => mc.ColleagueInfoId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين ColleagueInfo و DetainedColleague (جديد)
            modelBuilder.Entity<DetainedColleague>()
    .HasRequired(dc => dc.ColleagueInfo)
    .WithMany(ci => ci.DetainedColleagues)
    .HasForeignKey(dc => dc.ColleagueInfoId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين ColleagueInfo و InjuredColleague (جديد)
            modelBuilder.Entity<InjuredColleague>()
    .HasRequired(ic => ic.ColleagueInfo)
    .WithMany(ci => ci.InjuredColleagues)
    .HasForeignKey(ic => ic.ColleagueInfoId)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين Lawyer و GeneralInfo (جديد)
            modelBuilder.Entity<GeneralInfo>()
    .HasRequired(gi => gi.Lawyer)
    .WithMany(l => l.GeneralInfos)
    .HasForeignKey(gi => gi.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // تكوين العلاقة بين GeneralInfo و ReceivedAid (جديد)
            modelBuilder.Entity<ReceivedAid>()
    .HasRequired(ra => ra.GeneralInfo)
    .WithMany(gi => gi.ReceivedAids)
    .HasForeignKey(ra => ra.GeneralInfoId)
    .WillCascadeOnDelete(false);

            // NEW: تكوين العلاقة بين Lawyer و LawyerAttachment
            modelBuilder.Entity<LawyerAttachment>()
    .HasRequired(la => la.Lawyer) // المرفق يتطلب محامياً مرتبطاً
                .WithMany(l => l.LawyerAttachments) // المحامي الواحد يمكن أن يكون لديه العديد من المرفقات
                .HasForeignKey(la => la.LawyerIdNumber) // تحديد المفتاح الخارجي في جدول LawyerAttachments
                .WillCascadeOnDelete(false); // منع الحذف المتتالي للمرفقات عند حذف المحامي

            // تكوين العلاقة بين Message و ApplicationUser للمرسل
            modelBuilder.Entity<Message>()
    .HasRequired(m => m.SenderUser)
    .WithMany() // لا توجد خاصية تنقل عكسية في ApplicationUser لـ SentMessages
                .HasForeignKey(m => m.SenderId)
    .WillCascadeOnDelete(false); // منع الحذف المتتالي

            // تكوين العلاقة بين Message و ApplicationUser للمستلم
            modelBuilder.Entity<Message>()
    .HasRequired(m => m.ReceiverUser)
    .WithMany() // لا توجد خاصية تنقل عكسية في ApplicationUser لـ ReceivedMessages
                .HasForeignKey(m => m.ReceiverId)
    .WillCascadeOnDelete(false); // منع الحذف المتتالي

            // تكوين العلاقة بين Broadcast و ApplicationUser للمرسل (المسؤول)
            modelBuilder.Entity<Broadcast>()
    .HasRequired(b => b.SenderUser)
    .WithMany() // لا توجد خاصية تنقل عكسية في ApplicationUser للتعميمات المرسلة
                .HasForeignKey(b => b.SenderId)
    .WillCascadeOnDelete(false); // منع الحذف المتتالي

            // NEW: تكوين العلاقة بين UserBroadcastReadStatus و ApplicationUser
            modelBuilder.Entity<UserBroadcastReadStatus>()
    .HasRequired(ubrs => ubrs.User)
    .WithMany(u => u.BroadcastReadStatuses)
    .HasForeignKey(ubrs => ubrs.UserId)
    .WillCascadeOnDelete(true); // إذا حذف المستخدم، يتم حذف سجلات القراءة الخاصة به

            // NEW: تكوين العلاقة بين UserBroadcastReadStatus و Broadcast
            modelBuilder.Entity<UserBroadcastReadStatus>()
    .HasRequired(ubrs => ubrs.Broadcast)
    .WithMany() // لا توجد خاصية تنقل عكسية محددة في Broadcast لـ UserBroadcastReadStatuses
                .HasForeignKey(ubrs => ubrs.BroadcastId)
    .WillCascadeOnDelete(true); // إذا حذف التعميم، يتم حذف سجلات القراءة المرتبطة به

            // التأكد من أن زوج (UserId, BroadcastId) فريد لمنع تكرار سجلات القراءة
            modelBuilder.Entity<UserBroadcastReadStatus>()
    .HasIndex(ubrs => new { ubrs.UserId, ubrs.BroadcastId })
    .IsUnique();


            /////////////////////////
            // علاقة Exam و Question (واحد إلى متعدد)
            // علاقة Exam بـ Question
            modelBuilder.Entity<Question>()
    .HasRequired(q => q.Exam)
    .WithMany(e => e.Questions)
    .HasForeignKey(q => q.ExamId)
    .WillCascadeOnDelete(true);

            // علاقة Question بـ QuestionOption
            modelBuilder.Entity<QuestionOption>()
    .HasRequired(qo => qo.Question)
    .WithMany(q => q.Options)
    .HasForeignKey(qo => qo.QuestionId)
    .WillCascadeOnDelete(true);

            // علاقة Exam بـ ExamAttendee
            modelBuilder.Entity<ExamAttendee>()
    .HasRequired(ea => ea.Exam)
    .WithMany(e => e.ExamAttendees)
    .HasForeignKey(ea => ea.ExamId)
    .WillCascadeOnDelete(true);

            // علاقة Lawyer و ExamAttendee (واحد إلى متعدد)
            modelBuilder.Entity<ExamAttendee>()
    .HasRequired(ea => ea.Lawyer)
    .WithMany()
    .HasForeignKey(ea => ea.LawyerIdNumber)
    .WillCascadeOnDelete(false);

            // علاقة ExamAttendee بـ Answer
            modelBuilder.Entity<Answer>()
    .HasRequired(a => a.ExamAttendee)
    .WithMany(ea => ea.Answers)
    .HasForeignKey(a => a.ExamAttendeeId)
    .WillCascadeOnDelete(true);

            // علاقة Question بـ Answer
            modelBuilder.Entity<Answer>()
    .HasRequired(a => a.Question)
    .WithMany(q => q.Answers)
    .HasForeignKey(a => a.QuestionId)
    .WillCascadeOnDelete(false);

            // علاقة ExamAttendee بـ ExamResult
            modelBuilder.Entity<ExamResult>()
    .HasRequired(er => er.ExamAttendee)
    .WithOptional(ea => ea.ExamResult)
    .WillCascadeOnDelete(false);



            ///////////////////////
            ///
            // NEW: Configuration for the new RolePermission model
            ///
            // NEW: Configuration for the new RolePermission model
            modelBuilder.Entity<RolePermission>()
    .HasRequired(rp => rp.Role)
    .WithMany()
    .HasForeignKey(rp => rp.RoleId)
    .WillCascadeOnDelete(true);

            modelBuilder.Entity<RolePermission>()
             .HasRequired(rp => rp.Permission)
             .WithMany()
             .HasForeignKey(rp => rp.PermissionId)
             .WillCascadeOnDelete(true);



            // تكوينات خاصة لمستخدمي Identity (لا تغير هذه الأسطر)
            // base.OnModelCreating(modelBuilder); // تم نقل هذا السطر إلى بداية الدالة
            modelBuilder.Entity<IdentityUserLogin>().HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });
            modelBuilder.Entity<IdentityRole>().HasKey(r => r.Id);
            modelBuilder.Entity<IdentityUserRole>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserClaim>().HasKey(c => c.Id); // تأكد من وجود هذا إذا لم يكن موجوداً
        }
    }
}
