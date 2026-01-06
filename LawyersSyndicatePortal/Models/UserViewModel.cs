using System;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Display(Name = "الاسم الكامل")]
        public string FullName { get; set; }

        [Display(Name = "الرقم الوطني")]
        public string IdNumber { get; set; }

        // معرف المحامي المرتبط (يمكن أن يكون فارغًا)
        [Display(Name = "رقم هوية المحامي المرتبط")]
        public string LinkedLawyerIdNumber { get; set; }

        // اسم المحامي المرتبط (لعرضه بدلاً من المعرف)
        [Display(Name = "اسم المحامي المرتبط")]
        public string LinkedLawyerFullName { get; set; }

        // الأدوار (سلسلة نصية مفصولة بفاصلة لعرضها)
        [Display(Name = "الأدوار")]
        public string Roles { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? CreationDate { get; set; } // <--- تم التعديل هنا ليصبح DateTime?

    }
}
