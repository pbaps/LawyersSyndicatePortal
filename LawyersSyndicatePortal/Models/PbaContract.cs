using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawyersSyndicatePortal.Models
{
    public class PbaContract
    {
        [Key]
        public int ContractId { get; set; }

        // تم ربط هذا الحقل بـ IdNumber في جدول Lawyer
        [ForeignKey("Lawyer")]
        [Display(Name = "رقم هوية المحامي")]
        public string LawyerId { get; set; }

        [Display(Name = "اسم الموظف مدخل البيانات")]
        public string EmployeeName { get; set; }

        [Display(Name = "رقم الإيصال")]
        public string ReceiptNumber { get; set; }

        [Display(Name = "اسم الملف")]
        public string FileName { get; set; }

        [Display(Name = "اسم المحامي")]
        public string LawyerName { get; set; }

        [Display(Name = "رقم عضوية المحامي")]
        public string LawyerMembershipNumber { get; set; }

        [Display(Name = "نوع السند")]
        public string DeedType { get; set; }

        [Display(Name = "اسم الطرف الأول")]
        public string FirstPartyName { get; set; }

        [Display(Name = "هوية الطرف الأول")]
        public string FirstPartyId { get; set; }

        [Display(Name = "اسم الطرف الثاني")]
        public string SecondPartyName { get; set; }

        [Display(Name = "هوية الطرف الثاني")]
        public string SecondPartyId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ العقد (تاريخ مصادقة النقابة)")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ContractAuthenticationDate { get; set; }

        [Display(Name = "اسم المصادق على العقد")]
        public string AuthenticatorName { get; set; }

        [Display(Name = "فرع النقابة")]
        public string SyndicateBranch { get; set; }

        // خاصية التنقل
        public virtual Lawyer Lawyer { get; set; }
    }
}
