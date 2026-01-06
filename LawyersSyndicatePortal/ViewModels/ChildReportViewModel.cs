// LawyersSyndicatePortal\ViewModels\ChildReportViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace LawyersSyndicatePortal.ViewModels
{
    public class ChildReportViewModel
    {
        [Display(Name = "اسم المحامي")]
        public string LawyerFullName { get; set; }

        [Display(Name = "رقم هوية المحامي")]
        public string LawyerIdNumber { get; set; }

        [Display(Name = "اسم الابن/الابنة")]
        public string ChildName { get; set; }

        [Display(Name = "رقم هوية الابن")]
        public string ChildIdNumber { get; set; }

        [Display(Name = "تاريخ الميلاد")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; } // تم تصحيح النوع إلى DateTime?

        [Display(Name = "الجنس")]
        public string Gender { get; set; }

        [Display(Name = "العمر (سنوات)")]
        public int Age
        {
            get
            {
                if (DateOfBirth == default(DateTime))
                {
                    return 0;
                }

                // حساب العمر بالسنوات
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;

                // التحقق مما إذا كان يوم الميلاد لم يأت بعد في السنة الحالية
                if (DateOfBirth.Date > today.AddYears(-age))
                {
                    age--;
                }
                return age;
            }
        }


        // الخصائص الجديدة
        [Display(Name = "محافظة التواجد حاليًا")]
        public string CurrentGovernorate { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string OriginalGovernorate { get; set; }

        [Display(Name = "الحالة المهنية")]
        public string ProfessionalStatus { get; set; }
    }
}
