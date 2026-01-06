using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System;
using System.Linq;

namespace LawyersSyndicatePortal.Utilities
{
    /// <summary>
    /// فئة لتخزين النصوص الثابتة المستخدمة في التطبيق لسهولة التعديل والترجمة.
    /// </summary>
    public static class AppConstants
    {
        public const string MultipleChoiceString = "خيار متعدد";
        public const string TextAnswerString = "إجابة نصية";
        public const string QuestionsWorksheetName = "نموذج الأسئلة";
        public const string YesString = "نعم";
        public const string NoString = "لا";
        public const string QuestionTextHeader = "نص السؤال";
        public const string QuestionTypeHeader = "نوع السؤال";
        public const string ScoreHeader = "الدرجة";
        public const string ManualGradingHeader = "يحتاج تصحيح يدوي";
        public const string DurationHeader = "مدة السؤال (بالثواني)";
        public const string Option1Header = "الخيار 1";
        public const string Option2Header = "الخيار 2";
        public const string Option3Header = "الخيار 3";
        public const string Option4Header = "الخيار 4";
        public const string CorrectAnswerHeader = "الخيار الصحيح";
        public const string ImportFileErrorMessage = "الرجاء تحديد ملف صالح.";
        public const string ImportParsingErrorMessage = "حدث خطأ أثناء الاستيراد. تأكد من أن تنسيق الملف صحيح.";
        public const string ImportSuccessMessage = "تم استيراد الأسئلة بنجاح.";
    }

    /// <summary>
    /// فئة مساعدة لقراءة أسماء العرض (Display Name) من Enums.
    /// </summary>
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                                 .GetMember(enumValue.ToString())
                                 .FirstOrDefault()
                                 ?.GetCustomAttribute<DisplayAttribute>()
                                 ?.GetName() ?? enumValue.ToString();
        }
    }
}
