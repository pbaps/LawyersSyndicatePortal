// المسار: LawyersSyndicatePortal\ViewModels\LawyerDetailedReportViewModel.cs
using System.Collections.Generic;
using LawyersSyndicatePortal.Models; // تأكد من وجود مساحة الاسم هذه لموديل Lawyer
using System.Web.Mvc; // لـ SelectListItem
using System.ComponentModel.DataAnnotations; // لـ Display
using System.Linq; // لـ Linq operations
using System; // إضافة هذا السطر لاستخدام DateTime
using System.Reflection; // لاستخدام Reflection لجلب قيم الخصائص ديناميكياً

namespace LawyersSyndicatePortal.ViewModels
{
    public class LawyerDetailedReportViewModel
    {
        // قائمة المحامين التي سيتم عرضها بناءً على البحث
        public List<Lawyer> Lawyers { get; set; }

        // معلمات البحث/التصفية التي سيتم استخدامها من قبل المستخدم
        [Display(Name = "الحالة المهنية")]
        public string SelectedProfessionalStatus { get; set; }

        [Display(Name = "الجنس")]
        public string SelectedGender { get; set; }

        [Display(Name = "المحافظة الحالية")]
        public string SelectedCurrentGovernorate { get; set; }

        [Display(Name = "المحافظة الأصلية")]
        public string SelectedOriginalGovernorate { get; set; }

        [Display(Name = "الحد الأدنى لعمر الطفل")]
        public int? MinChildAge { get; set; }

        [Display(Name = "الحد الأقصى لعمر الطفل")]
        public int? MaxChildAge { get; set; }

        [Display(Name = "نوع ملكية المكتب")]
        public string SelectedOfficePropertyType { get; set; }

        [Display(Name = "نوع ضرر المكتب")]
        public string SelectedOfficeDamageType { get; set; }

        // خصائص جديدة للفلاتر الإضافية
        [Display(Name = "الحالة الاجتماعية")]
        public string SelectedMaritalStatus { get; set; }

        [Display(Name = "هل تعرض للاعتقال؟")]
        public bool? SelectedWasDetained { get; set; }

        [Display(Name = "هل تعرض المنزل لأضرار؟")]
        public bool? SelectedHasHomeDamage { get; set; }

        [Display(Name = "نوع ضرر المنزل")]
        public string SelectedHomeDamageType { get; set; }

        [Display(Name = "هل تعرض أحد أفراد العائلة للإصابة؟")]
        public bool? SelectedHasFamilyMembersInjured { get; set; }

        [Display(Name = "حالة المحامي الصحية")]
        public string SelectedLawyerCondition { get; set; }

        [Display(Name = "هل يمارس المحاماة الشرعية؟")]
        public bool? SelectedPracticesShariaLaw { get; set; }

        [Display(Name = "نوع المساعدة المستلمة")]
        public string SelectedAidType { get; set; }

        [Display(Name = "نوع الاعتقال")]
        public string SelectedDetentionType { get; set; }

        [Display(Name = "مكان الاعتقال")]
        public string SelectedDetentionLocation { get; set; }

        [Display(Name = "هل يعلم بوجود شهداء من الزملاء؟")]
        public bool? FilterByMartyrColleagues { get; set; }

        [Display(Name = "هل يعلم بوجود معتقلين من الزملاء؟")]
        public bool? FilterByDetainedColleagues { get; set; }

        [Display(Name = "هل يعلم بوجود مصابين من الزملاء؟")]
        public bool? FilterByInjuredColleagues { get; set; }

        [Display(Name = "هل استلم مساعدات من النقابة؟")]
        public bool? FilterByReceivedAid { get; set; }


        // خيارات القوائم المنسدلة لواجهة المستخدم
        public IEnumerable<SelectListItem> ProfessionalStatuses { get; set; }
        public IEnumerable<SelectListItem> Genders { get; set; }
        public IEnumerable<SelectListItem> Governorates { get; set; }
        public IEnumerable<SelectListItem> OfficePropertyTypes { get; set; }
        public IEnumerable<SelectListItem> OfficeDamageTypes { get; set; }
        public IEnumerable<SelectListItem> MaritalStatuses { get; set; }
        public IEnumerable<SelectListItem> YesNoOptions { get; set; }
        public IEnumerable<SelectListItem> HomeDamageTypes { get; set; }
        public IEnumerable<SelectListItem> LawyerConditions { get; set; }
        public IEnumerable<SelectListItem> AidTypes { get; set; }
        public IEnumerable<SelectListItem> DetentionTypes { get; set; }
        public IEnumerable<SelectListItem> DetentionLocations { get; set; }


        // خاصية لتحديد ما إذا كان المستخدم يريد تصدير البيانات إلى Excel
        public bool ExportToExcel { get; set; }


        // -------------------------------------------------------------------
        // إضافة خصائص جديدة لاختيار الأعمدة
        public List<ColumnOption> AvailableColumns { get; set; }
        public List<string> SelectedColumnNames { get; set; } // لتخزين أسماء الأعمدة المختارة

        public LawyerDetailedReportViewModel()
        {
            Lawyers = new List<Lawyer>();
            SelectedColumnNames = new List<string>();

            // تهيئة الأعمدة المتاحة مع تحديد افتراضي
            AvailableColumns = new List<ColumnOption>
            {
                // Lawyer Basic Info
                new ColumnOption { Name = "IdNumber", DisplayName = "رقم الهوية", IsSelected = true },
                new ColumnOption { Name = "FullName", DisplayName = "اسم المحامي", IsSelected = true },
                new ColumnOption { Name = "ProfessionalStatus", DisplayName = "الحالة المهنية", IsSelected = true },
                new ColumnOption { Name = "IsTrainee", DisplayName = "متدرب؟", IsSelected = false },
                new ColumnOption { Name = "MembershipNumber", DisplayName = "رقم العضوية", IsSelected = false },
                new ColumnOption { Name = "PracticeStartDate", DisplayName = "تاريخ بدء المزاولة", IsSelected = false },
                new ColumnOption { Name = "TrainingStartDate", DisplayName = "تاريخ بدء التدريب", IsSelected = false },
                new ColumnOption { Name = "TrainerLawyerName", DisplayName = "اسم المدرب", IsSelected = false },

                // Personal Details
                new ColumnOption { Name = "PersonalDetails.Gender", DisplayName = "الجنس", IsSelected = true },
                new ColumnOption { Name = "PersonalDetails.EmailAddress", DisplayName = "البريد الإلكتروني", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.OriginalGovernorate", DisplayName = "المحافظة الأصلية", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.CurrentGovernorate", DisplayName = "المحافظة الحالية", IsSelected = true },
                new ColumnOption { Name = "PersonalDetails.AccommodationType", DisplayName = "طبيعة السكن", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.FullAddress", DisplayName = "العنوان الكامل", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.MobileNumber", DisplayName = "رقم الجوال", IsSelected = true },
                new ColumnOption { Name = "PersonalDetails.AltMobileNumber1", DisplayName = "جوال احتياطي 1", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.AltMobileNumber2", DisplayName = "جوال احتياطي 2", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.WhatsAppNumber", DisplayName = "رقم الواتساب", IsSelected = false },
                new ColumnOption { Name = "PersonalDetails.LandlineNumber", DisplayName = "رقم الهاتف الأرضي", IsSelected = false },

                // Family Details
                new ColumnOption { Name = "FamilyDetails.MaritalStatus", DisplayName = "الحالة الاجتماعية", IsSelected = false },
                new ColumnOption { Name = "FamilyDetails.NumberOfSpouses", DisplayName = "عدد الزوجات", IsSelected = false },
                new ColumnOption { Name = "FamilyDetails.HasChildren", DisplayName = "لديه أبناء؟", IsSelected = false },
                new ColumnOption { Name = "FamilyDetails.NumberOfChildren", DisplayName = "عدد الأبناء", IsSelected = false },
                new ColumnOption { Name = "FamilyDetails.SpousesCount", DisplayName = "عدد الزوجات المسجلين", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "FamilyDetails.ChildrenCount", DisplayName = "عدد الأبناء المسجلين", IsSelected = false }, // خاصية مخصصة

                // Health Status
                new ColumnOption { Name = "HealthStatuses.LawyerCondition", DisplayName = "الحالة الصحية", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.InjuryDetails", DisplayName = "تفاصيل الإصابة", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.TreatmentNeeded", DisplayName = "العلاج المطلوب", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.LawyerDiagnosis", DisplayName = "تشخيص المحامي", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.HasFamilyMembersInjured", DisplayName = "أفراد عائلة مصابون؟", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.NumberOfFamilyMembersInjured", DisplayName = "عدد المصابين من العائلة", IsSelected = false },
                new ColumnOption { Name = "HealthStatuses.FamilyMemberInjuriesCount", DisplayName = "عدد إصابات العائلة المسجلة", IsSelected = false }, // خاصية مخصصة

                // Office Details
                new ColumnOption { Name = "OfficeDetails.OfficeName", DisplayName = "اسم المكتب", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.OfficeAddress", DisplayName = "عنوان المكتب", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.PropertyType", DisplayName = "نوع العقار", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.PropertyStatus", DisplayName = "حالة العقار", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.HasPartners", DisplayName = "لديه شركاء؟", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.NumberOfPartners", DisplayName = "عدد الشركاء", IsSelected = false },
                new ColumnOption { Name = "OfficeDetails.PartnersCount", DisplayName = "عدد الشركاء المسجلين", IsSelected = false }, // خاصية مخصصة

                // Home Damages
                new ColumnOption { Name = "HomeDamages.HasHomeDamage", DisplayName = "المنزل متضرر؟", IsSelected = false },
                new ColumnOption { Name = "HomeDamages.DamageType", DisplayName = "نوع ضرر المنزل", IsSelected = false },
                new ColumnOption { Name = "HomeDamages.DamageDetails", DisplayName = "تفاصيل ضرر المنزل", IsSelected = false },

                // Office Damages
                new ColumnOption { Name = "OfficeDamages.HasOfficeDamage", DisplayName = "المكتب متضرر؟", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "OfficeDamages.DamageType", DisplayName = "نوع ضرر المكتب", IsSelected = false },
                new ColumnOption { Name = "OfficeDamages.DamageDetails", DisplayName = "تفاصيل ضرر المكتب", IsSelected = false },

                // Detention Details
                new ColumnOption { Name = "DetentionDetails.WasDetained", DisplayName = "تعرض للاعتقال؟", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.DetentionDuration", DisplayName = "مدة الاعتقال", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.DetentionStartDate", DisplayName = "تاريخ بدء الاعتقال", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.IsStillDetained", DisplayName = "لا يزال معتقلاً؟", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.ReleaseDate", DisplayName = "تاريخ الإفراج", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.DetentionType", DisplayName = "نوع الاعتقال", IsSelected = false },
                new ColumnOption { Name = "DetentionDetails.DetentionLocation", DisplayName = "مكان الاعتقال", IsSelected = false },

                // Colleague Info
                new ColumnOption { Name = "ColleagueInfos.KnowsOfMartyrColleagues", DisplayName = "يعلم بوجود شهداء؟", IsSelected = false },
                new ColumnOption { Name = "ColleagueInfos.HasMartyrs", DisplayName = "لديه شهداء في سجلاته؟", IsSelected = false },
                new ColumnOption { Name = "ColleagueInfos.MartyrColleaguesCount", DisplayName = "عدد الشهداء المسجلين", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "ColleagueInfos.DetainedColleaguesCount", DisplayName = "عدد المعتقلين المسجلين", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "ColleagueInfos.KnowsOfDetainedColleagues", DisplayName = "يعلم بوجود معتقلين؟", IsSelected = false },
                new ColumnOption { Name = "ColleagueInfos.HasDetained", DisplayName = "لديه معتقلين في سجلاته؟", IsSelected = false },
                new ColumnOption { Name = "ColleagueInfos.InjuredColleaguesCount", DisplayName = "عدد المصابين المسجلين", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "ColleagueInfos.KnowsOfInjuredColleagues", DisplayName = "يعلم بوجود مصابين؟", IsSelected = false },
                new ColumnOption { Name = "ColleagueInfos.HasInjured", DisplayName = "لديه مصابين في سجلاته؟", IsSelected = false },

                // General Info
                new ColumnOption { Name = "GeneralInfos.PracticesShariaLaw", DisplayName = "يمارس الشرعية؟", IsSelected = false },
                new ColumnOption { Name = "GeneralInfos.ShariaLawPracticeStartDate", DisplayName = "تاريخ بدء الشرعية", IsSelected = false },
                new ColumnOption { Name = "GeneralInfos.ReceivedAidFromSyndicate", DisplayName = "استلم مساعدات من النقابة؟", IsSelected = false },
                new ColumnOption { Name = "GeneralInfos.ReceivedAidsCount", DisplayName = "عدد المساعدات المستلمة", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "GeneralInfos.AidType", DisplayName = "نوع المساعدة المستلمة (أول مساعدة)", IsSelected = false }, // خاصية مخصصة
                new ColumnOption { Name = "GeneralInfos.ReceivedAidDate", DisplayName = "تاريخ استلام المساعدة (أول مساعدة)", IsSelected = false }, // خاصية مخصصة

                // Lawyer Attachments
                new ColumnOption { Name = "LawyerAttachmentsCount", DisplayName = "عدد المرفقات", IsSelected = false } // خاصية مخصصة
            };

            // تعيين الأعمدة المختارة افتراضياً لتكون تلك التي IsSelected = true
            SelectedColumnNames = AvailableColumns.Where(c => c.IsSelected).Select(c => c.Name).ToList();
        }

        // دالة مساعدة لجلب قيمة الخاصية ديناميكياً
        public object GetPropertyValue(Lawyer lawyer, string propertyPath)
        {
            if (lawyer == null || string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            // التعامل مع الخصائص المخصصة التي ليست جزءاً مباشراً من موديل Lawyer
            switch (propertyPath)
            {
                case "FamilyDetails.SpousesCount":
                    return lawyer.FamilyDetails?.FirstOrDefault()?.Spouses?.Count ?? 0;
                case "FamilyDetails.ChildrenCount":
                    return lawyer.FamilyDetails?.FirstOrDefault()?.Children?.Count ?? 0;
                case "HealthStatuses.FamilyMemberInjuriesCount":
                    return lawyer.HealthStatuses?.FirstOrDefault()?.FamilyMemberInjuries?.Count ?? 0;
                case "OfficeDetails.PartnersCount":
                    return lawyer.OfficeDetails?.FirstOrDefault()?.Partners?.Count ?? 0;
                case "OfficeDamages.HasOfficeDamage":
                    return (lawyer.OfficeDamages?.Any() ?? false) ? "نعم" : "لا";
                case "ColleagueInfos.MartyrColleaguesCount":
                    return lawyer.ColleagueInfos?.FirstOrDefault()?.MartyrColleagues?.Count ?? 0;
                case "ColleagueInfos.DetainedColleaguesCount":
                    return lawyer.ColleagueInfos?.FirstOrDefault()?.DetainedColleagues?.Count ?? 0;
                case "ColleagueInfos.InjuredColleaguesCount":
                    return lawyer.ColleagueInfos?.FirstOrDefault()?.InjuredColleagues?.Count ?? 0;
                case "GeneralInfos.ReceivedAidsCount":
                    return lawyer.GeneralInfos?.FirstOrDefault()?.ReceivedAids?.Count ?? 0;
                case "GeneralInfos.AidType":
                    return lawyer.GeneralInfos?.FirstOrDefault()?.ReceivedAids?.FirstOrDefault()?.AidType ?? "لا يوجد";
                case "GeneralInfos.ReceivedAidDate":
                    var receivedAidDate = lawyer.GeneralInfos?.FirstOrDefault()?.ReceivedAids?.FirstOrDefault()?.ReceivedDate;
                    return receivedAidDate.HasValue ? receivedAidDate.Value.ToString("yyyy-MM-dd") : "لا يوجد";
                case "LawyerAttachmentsCount":
                    return lawyer.LawyerAttachments?.Count ?? 0;
            }

            // التعامل مع الخصائص العادية والمتداخلة باستخدام Reflection
            object valueToProcess = lawyer;
            var parts = propertyPath.Split('.');

            foreach (var part in parts)
            {
                if (valueToProcess == null)
                {
                    return null;
                }

                PropertyInfo prop = valueToProcess.GetType().GetProperty(part);
                if (prop == null)
                {
                    // إذا لم يتم العثور على الخاصية، حاول البحث في الـ ICollection الأول
                    var collectionProperty = valueToProcess.GetType().GetProperties()
                                                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                                                     p.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>) &&
                                                                     p.PropertyType.GetGenericArguments().Any() &&
                                                                     p.PropertyType.GetGenericArguments()[0].Name == part);

                    if (collectionProperty != null)
                    {
                        var collection = collectionProperty.GetValue(valueToProcess) as System.Collections.IEnumerable;
                        if (collection != null)
                        {
                            valueToProcess = collection.Cast<object>().FirstOrDefault();
                            continue;
                        }
                    }
                    return null;
                }
                valueToProcess = prop.GetValue(valueToProcess);
            }

            // تنسيق القيم
            if (valueToProcess is DateTime dateTimeValue)
            {
                return dateTimeValue.ToString("yyyy-MM-dd");
            }
            else if (valueToProcess is DateTime?) // التحقق من النوع العام DateTime (يشمل DateTime?)
            {
                DateTime? nullableDateTimeValue = valueToProcess as DateTime?; // تحويل آمن إلى DateTime?
                return nullableDateTimeValue.HasValue ? nullableDateTimeValue.Value.ToString("yyyy-MM-dd") : "لا يوجد";
            }
            else if (valueToProcess is bool boolValue)
            {
                return boolValue ? "نعم" : "لا";
            }
            else if (valueToProcess is bool?) // التحقق من النوع العام bool (يشمل bool?)
            {
                bool? nullableBoolValue = valueToProcess as bool?; // تحويل آمن إلى bool?
                return nullableBoolValue.HasValue ? (nullableBoolValue.Value ? "نعم" : "لا") : "لا يوجد";
            }
            else if (valueToProcess != null)
            {
                return valueToProcess.ToString();
            }

            return "لا يوجد";
        }
    }

    // كلاس مساعد لتمثيل خيار العمود
    public class ColumnOption
    {
        public string Name { get; set; } // اسم الخاصية في موديل Lawyer أو الموديلات المرتبطة
        public string DisplayName { get; set; } // الاسم المعروض للمستخدم
        public bool IsSelected { get; set; } // هل تم تحديد هذا العمود للعرض؟
    }
}
