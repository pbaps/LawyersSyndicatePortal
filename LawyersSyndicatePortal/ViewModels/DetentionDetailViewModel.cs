// LawyersSyndicatePortal\ViewModels\DetentionDetailViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc; // لـ SelectListItem
using System.Collections.Generic; // لـ IEnumerable<SelectListItem>

namespace LawyersSyndicatePortal.ViewModels
{
    public class DetentionDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "هل تعرض المحامي للاعتقال؟")]
        public bool WasDetained { get; set; }

        [StringLength(100)]
        [Display(Name = "مدة الاعتقال")]
        public string DetentionDuration { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ بدء الاعتقال")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DetentionStartDate { get; set; }

        [Display(Name = "هل ما زال معتقلاً؟")]
        public bool IsStillDetained { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الإفراج (إن تم الإفراج)")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ReleaseDate { get; set; }

        [StringLength(255)]
        [Display(Name = "نوع الاعتقال")]
        public string DetentionType { get; set; }

        [StringLength(500)]
        [Display(Name = "مكان الاعتقال")]
        public string DetentionLocation { get; set; }

        // خصائص لـ DropDownLists
        public IEnumerable<SelectListItem> DetentionDurations { get; set; }
        public IEnumerable<SelectListItem> DetentionTypes { get; set; }
        public IEnumerable<SelectListItem> DetentionLocations { get; set; }
    }
}
