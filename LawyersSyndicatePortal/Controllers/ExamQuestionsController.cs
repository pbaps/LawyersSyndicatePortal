using LawyersSyndicatePortal.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Text;
using ClosedXML.Excel;
using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Utilities;
using static LawyersSyndicatePortal.Utilities.AppConstants;

namespace LawyersSyndicatePortal.Controllers
{
    // فئة مساعدة داخلية (Helper Class) لتخزين نتائج عملية الاستيراد.
    // تحتوي على قائمة بالأسئلة التي تم تحليلها وقائمة بالأخطاء التي حدثت.
    public class ImportResult
    {
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<string> Errors { get; set; } = new List<string>();
        public bool IsValid => Errors.Count == 0;
    }

    /// <summary>
    /// المتحكم (Controller) المسؤول عن إدارة الأسئلة والخيارات الخاصة بالامتحانات.
    /// يستخدم فلاتر التخويل والتسجيل لضمان أمان العمليات ومراقبتها.
    /// </summary>
    /// 
    [Authorize(Roles = "Admin")]
    public class ExamQuestionsController : Controller
    {
        // استخدم نمط حقن التبعية (Dependency Injection) لإدارة DbContext
        private readonly ApplicationDbContext db;

        // المُنشئ (Constructor) الذي يستقبل ApplicationDbContext.
        public ExamQuestionsController(ApplicationDbContext context)
        {
            this.db = context;
        }

        // المُنشئ الافتراضي (Default Constructor) لاستخدامه في حالة عدم وجود DI.
        public ExamQuestionsController()
        {
            this.db = new ApplicationDbContext();
        }

        // GET: ExamQuestions
        // يعرض الصفحة الرئيسية للأسئلة الخاصة بامتحان معين.
        [PermissionAuthorizationFilter("عرض أسئلة الاختبار", "صلاحية عرض جميع الأسئلة الخاصة بامتحان معين")]
        [AuditLog("عرض", "عرض أسئلة الاختبار")]
        public async Task<ActionResult> Index(int? examId)
        {
            if (examId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // جلب الأسئلة المرتبطة بالامتحان المحدد مع خياراتها
            var questions = await db.Questions
                                    .Include(q => q.Options)
                                    .Where(q => q.ExamId == examId)
                                    .ToListAsync();

            // حفظ معرف الامتحان وعنوانه في ViewBag لتسهيل استخدامه في الصفحة
            ViewBag.ExamId = examId;
            ViewBag.ExamTitle = (await db.Exams.FindAsync(examId))?.Title;

            return View(questions);
        }

        // GET: ExamQuestions/Create
        // يعرض صفحة لإنشاء سؤال جديد لامتحان معين.
        [PermissionAuthorizationFilter("إنشاء سؤال جديد", "صلاحية عرض صفحة إنشاء سؤال جديد")]
        [AuditLog("إنشاء", "عرض صفحة إنشاء سؤال جديد")]
        public ActionResult Create(int examId)
        {
            ViewBag.ExamId = examId;
            return View();
        }

        // POST: ExamQuestions/Create
        // يستقبل البيانات من النموذج ويحفظ السؤال الجديد في قاعدة البيانات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ سؤال جديد", "صلاحية حفظ سؤال جديد مع خياراته")]
        [AuditLog("حفظ", "حفظ سؤال جديد")]
        public async Task<ActionResult> Create(int examId,
            [Bind(Include = "Text,QuestionType,Score,DurationSeconds")] Question question,
            IEnumerable<QuestionOption> options,
            int? correctOptionIndex)
        {
            // هذا الشرط يتحقق مما إذا كان النموذج صالحًا قبل محاولة الحفظ
            if (ModelState.IsValid)
            {
                question.ExamId = examId;

                // في حالة السؤال النصي (المقالي)، يتم تعيين خاصية RequiresManualGrading إلى true
                if (question.QuestionType == QuestionType.TextAnswer)
                {
                    question.RequiresManualGrading = true;
                }
                else
                {
                    question.RequiresManualGrading = false;
                }

                db.Questions.Add(question);

                try
                {
                    // حفظ التغييرات الآن للحصول على Question.Id
                    await db.SaveChangesAsync();

                    // إذا كان نوع السؤال خيار متعدد، نقوم بحفظ الخيارات
                    if (question.QuestionType == QuestionType.MultipleChoice)
                    {
                        if (options != null)
                        {
                            var optionsList = options.ToList();
                            // تأكد من وجود قيمة للفهرس الصحيح
                            if (correctOptionIndex.HasValue)
                            {
                                for (int i = 0; i < optionsList.Count; i++)
                                {
                                    var option = optionsList[i];
                                    option.QuestionId = question.Id;
                                    // تعيين IsCorrect بناءً على الفهرس المرسل
                                    option.IsCorrect = (i == correctOptionIndex.Value);
                                    db.QuestionOptions.Add(option);
                                }
                            }
                        }
                    }
                    // في حالة الأسئلة النصية، لا يوجد خيارات أو فهرس صحيح، لذلك لا يتم تنفيذ أي شيء هنا
                    else if (question.QuestionType == QuestionType.TextAnswer)
                    {
                        // لا توجد خيارات مرتبطة بسؤال نصي، لذلك لا يوجد شيء لحفظه هنا.
                    }

                    await db.SaveChangesAsync();
                    return RedirectToAction("Index", new { examId = examId });
                }
                catch (DbUpdateException ex)
                {
                    // معالجة الأخطاء التي قد تحدث أثناء الحفظ في قاعدة البيانات
                    TempData["ErrorMessage"] = "حدث خطأ في قاعدة البيانات أثناء حفظ السؤال: " + ex.Message;
                }
            }
            else
            {
                // إذا كان ModelState غير صالح، قم بجمع رسائل الأخطاء وعرضها
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = "حدث خطأ في إدخال البيانات: " + string.Join(" | ", errors);
            }

            ViewBag.ExamId = examId;
            return View(question);
        }
        // GET: ExamQuestions/Edit/5
 
        // GET: ExamQuestions/Edit/5
        // يعرض صفحة لتعديل سؤال موجود.
        [PermissionAuthorizationFilter("تعديل سؤال", "صلاحية عرض صفحة تعديل سؤال موجود")]
        [AuditLog("تعديل", "عرض صفحة تعديل سؤال")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // جلب السؤال مع خياراته لتعديله
            var question = await db.Questions
                                    .Include(q => q.Options)
                                    .SingleOrDefaultAsync(q => q.Id == id);
            if (question == null)
            {
                return HttpNotFound();
            }
            // تهيئة قائمة الخيارات إذا كانت فارغة لتجنب الأخطاء
            if (question.QuestionType == QuestionType.MultipleChoice && (question.Options == null || !question.Options.Any()))
            {
                question.Options = new List<QuestionOption>();
            }
            ViewBag.ExamId = question.ExamId;
            return View(question);
        }

        // POST: ExamQuestions/Edit/5
        // يستقبل التغييرات من النموذج ويحفظها في قاعدة البيانات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("حفظ تعديلات السؤال", "صلاحية حفظ التغييرات على سؤال موجود")]
        [AuditLog("حفظ", "حفظ تعديلات السؤال")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Text,QuestionType,Score,DurationSeconds,RequiresManualGrading,ExamId")] Question question, IEnumerable<QuestionOption> options, int? correctOptionIndex)
        {
            if (ModelState.IsValid)
            {
                var existingQuestion = await db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == question.Id);
                if (existingQuestion == null)
                {
                    return HttpNotFound();
                }

                // تحديث الخصائص الأساسية للسؤال.
                existingQuestion.Text = question.Text;
                existingQuestion.QuestionType = question.QuestionType;
                existingQuestion.Score = question.Score;
                existingQuestion.DurationSeconds = question.DurationSeconds;
                existingQuestion.RequiresManualGrading = question.QuestionType == QuestionType.TextAnswer;

                // حذف جميع الخيارات القديمة قبل إضافة الخيارات الجديدة لضمان التزامن
                db.QuestionOptions.RemoveRange(existingQuestion.Options);
                existingQuestion.Options.Clear(); // تأكد من تفريغ القائمة المحلية

                if (existingQuestion.QuestionType == QuestionType.MultipleChoice)
                {
                    if (options != null)
                    {
                        var optionsList = options.ToList();
                        if (correctOptionIndex.HasValue)
                        {
                            for (int i = 0; i < optionsList.Count; i++)
                            {
                                var option = optionsList[i];
                                option.QuestionId = existingQuestion.Id;
                                option.IsCorrect = (i == correctOptionIndex.Value);
                                existingQuestion.Options.Add(option); // أضف الخيارات الجديدة إلى الكائن
                            }
                        }
                    }
                }

                // حفظ التغييرات دفعة واحدة
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تحديث السؤال بنجاح.";
                return RedirectToAction("Index", new { examId = existingQuestion.ExamId });
            }

            // إضافة رسالة خطأ إذا كان النموذج غير صالح
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["ErrorMessage"] = "حدث خطأ في إدخال البيانات: " + string.Join(" | ", errors);

            ViewBag.ExamId = question.ExamId;
            return View(question);
        }

        // GET: ExamQuestions/Delete/5
        // يعرض صفحة تأكيد حذف سؤال.
        [PermissionAuthorizationFilter("حذف سؤال", "صلاحية عرض صفحة تأكيد حذف سؤال")]
        [AuditLog("حذف", "عرض صفحة تأكيد حذف سؤال")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = await db.Questions.FindAsync(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: ExamQuestions/Delete/5
        // يؤكد عملية الحذف ويحذف السؤال من قاعدة البيانات.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تأكيد حذف سؤال", "صلاحية حذف سؤال من قاعدة البيانات")]
        [AuditLog("حذف", "تأكيد حذف سؤال")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Question question = await db.Questions.FindAsync(id);
            int examId = question.ExamId; // حفظ الـ ExamId قبل الحذف
            db.Questions.Remove(question);
            await db.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حذف السؤال بنجاح.";
            return RedirectToAction("Index", new { examId = examId });
        }

        // GET: ExamQuestions/ExportToExcel/5
        // تصدير الأسئلة إلى ملف Excel باستخدام مكتبة ClosedXML.
        [PermissionAuthorizationFilter("تصدير أسئلة الاختبار", "صلاحية تصدير أسئلة امتحان إلى ملف إكسل")]
        [AuditLog("تصدير", "تصدير أسئلة الاختبار إلى إكسل")]
        public async Task<FileResult> ExportToExcel(int examId)
        {
            var questions = await db.Questions
                                    .Include(q => q.Options)
                                    .Where(q => q.ExamId == examId)
                                    .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("الأسئلة");
                var currentRow = 1;

                // استخدام الثوابت (Constants) لعنوان الأعمدة
                worksheet.Cell(currentRow, 1).Value = QuestionTextHeader;
                worksheet.Cell(currentRow, 2).Value = QuestionTypeHeader;
                worksheet.Cell(currentRow, 3).Value = ScoreHeader;
                worksheet.Cell(currentRow, 4).Value = ManualGradingHeader;
                worksheet.Cell(currentRow, 5).Value = DurationHeader;
                worksheet.Cell(currentRow, 6).Value = Option1Header;
                worksheet.Cell(currentRow, 7).Value = Option2Header;
                worksheet.Cell(currentRow, 8).Value = Option3Header;
                worksheet.Cell(currentRow, 9).Value = Option4Header;
                worksheet.Cell(currentRow, 10).Value = CorrectAnswerHeader;

                var headerRow = worksheet.Row(currentRow);
                headerRow.Style.Font.Bold = true;
                headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                foreach (var question in questions)
                {
                    currentRow++;

                    worksheet.Cell(currentRow, 1).Value = question.Text;
                    // استخدام GetDisplayName لاستخراج الاسم المعروض من Enum
                    worksheet.Cell(currentRow, 2).Value = question.QuestionType.GetDisplayName();
                    worksheet.Cell(currentRow, 3).Value = question.Score;
                    worksheet.Cell(currentRow, 4).Value = question.RequiresManualGrading ? YesString : NoString;
                    worksheet.Cell(currentRow, 5).Value = question.DurationSeconds;

                    if (question.QuestionType == QuestionType.MultipleChoice && question.Options != null)
                    {
                        var orderedOptions = question.Options.OrderBy(o => o.Id).ToList();

                        for (int i = 0; i < orderedOptions.Count; i++)
                        {
                            var option = orderedOptions[i];
                            worksheet.Cell(currentRow, 6 + i).Value = option.Text;
                        }

                        var correctAnswer = orderedOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctAnswer != null)
                        {
                            worksheet.Cell(currentRow, 10).Value = correctAnswer.Text;
                        }
                    }
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = $"ExamQuestions_{examId}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // GET: ExamQuestions/DownloadImportTemplate
        // تنزيل نموذج فارغ لملف الاستيراد لمساعدة المستخدمين.
        [PermissionAuthorizationFilter("تنزيل نموذج استيراد", "صلاحية تنزيل نموذج لملف استيراد الأسئلة")]
        [AuditLog("تنزيل", "تنزيل نموذج استيراد أسئلة")]
        public FileResult DownloadImportTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                // استخدام الثابت (Constant) لاسم ورقة العمل
                var worksheet = workbook.Worksheets.Add(QuestionsWorksheetName);

                // استخدام الثوابت (Constants) لعنوان الأعمدة
                worksheet.Cell(1, 1).Value = QuestionTextHeader;
                worksheet.Cell(1, 2).Value = QuestionTypeHeader;
                worksheet.Cell(1, 3).Value = ScoreHeader;
                worksheet.Cell(1, 4).Value = ManualGradingHeader;
                worksheet.Cell(1, 5).Value = DurationHeader;
                worksheet.Cell(1, 6).Value = Option1Header;
                worksheet.Cell(1, 7).Value = Option2Header;
                worksheet.Cell(1, 8).Value = Option3Header;
                worksheet.Cell(1, 9).Value = Option4Header;
                worksheet.Cell(1, 10).Value = CorrectAnswerHeader;

                // إضافة أمثلة توضيحية للمساعدة في فهم التنسيق
                worksheet.Cell(2, 1).Value = "مثال: أي من الكواكب التالية هو الأقرب إلى الشمس؟";
                worksheet.Cell(2, 2).Value = MultipleChoiceString;
                worksheet.Cell(2, 3).Value = 10;
                worksheet.Cell(2, 4).Value = NoString;
                worksheet.Cell(2, 5).Value = 60;
                worksheet.Cell(2, 6).Value = "المريخ";
                worksheet.Cell(2, 7).Value = "الزهرة";
                worksheet.Cell(2, 8).Value = "عطارد";
                worksheet.Cell(2, 9).Value = "المشتري";
                worksheet.Cell(2, 10).Value = "عطارد";

                worksheet.Cell(3, 1).Value = "مثال: اكتب الإجابة القانونية للسؤال التالي...";
                worksheet.Cell(3, 2).Value = TextAnswerString;
                worksheet.Cell(3, 3).Value = 20;
                worksheet.Cell(3, 4).Value = YesString;
                worksheet.Cell(3, 5).Value = 180;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    var fileName = "Questions_Import_Template.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        // POST: ExamQuestions/ImportFromExcel
        // استيراد الأسئلة من ملف Excel وتحليلها وحفظها في قاعدة البيانات.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("استيراد أسئلة من ملف إكسل", "صلاحية استيراد أسئلة جديدة إلى الامتحان")]
        [AuditLog("استيراد", "استيراد أسئلة من ملف إكسل")]
        public async Task<ActionResult> ImportFromExcel(int examId, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                TempData["ErrorMessage"] = ImportFileErrorMessage;
                return RedirectToAction("Index", new { examId });
            }

            try
            {
                using (var workbook = new XLWorkbook(file.InputStream))
                {
                    var result = ParseQuestionsFromExcel(workbook, examId);

                    if (!result.IsValid)
                    {
                        TempData["ErrorMessage"] = string.Join(" | ", result.Errors);
                        return RedirectToAction("Index", new { examId });
                    }

                    db.Questions.AddRange(result.Questions);
                    await db.SaveChangesAsync();

                    TempData["SuccessMessage"] = ImportSuccessMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ImportParsingErrorMessage + ex.Message;
            }

            return RedirectToAction("Index", new { examId });
        }

        // دالة خاصة لتحليل ورقة عمل Excel واستخراج الأسئلة منها.
        private ImportResult ParseQuestionsFromExcel(XLWorkbook workbook, int examId)
        {
            var result = new ImportResult();
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // تجاهل صف الرأس

            foreach (var row in rows)
            {
                var rowResult = ParseSingleQuestionRow(row, examId);
                if (rowResult.IsValid)
                {
                    result.Questions.Add(rowResult.Questions.Single());
                }
                else
                {
                    result.Errors.AddRange(rowResult.Errors);
                }
            }
            return result;
        }

        // دالة خاصة لتحليل صف واحد من ملف Excel وتحويله إلى كائن سؤال.
        private ImportResult ParseSingleQuestionRow(IXLRow row, int examId)
        {
            var result = new ImportResult();
            try
            {
                var questionText = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(questionText))
                {
                    return result; // تجاهل الصفوف الفارغة تماماً
                }

                var questionTypeString = row.Cell(2).GetString().Trim();
                var scoreString = row.Cell(3).GetString().Trim();
                var manualGradingString = row.Cell(4).GetString().Trim();
                var durationString = row.Cell(5).GetString().Trim();

                var question = new Question();
                question.ExamId = examId;
                question.Text = questionText;

                // التحقق من نوع السؤال
                if (questionTypeString == MultipleChoiceString)
                {
                    question.QuestionType = QuestionType.MultipleChoice;
                }
                else if (questionTypeString == TextAnswerString)
                {
                    question.QuestionType = QuestionType.TextAnswer;
                }
                else
                {
                    result.Errors.Add($"نوع السؤال غير صالح في الصف {row.RowNumber()}.");
                    return result;
                }

                // التحقق من الدرجة
                if (int.TryParse(scoreString, out int score))
                {
                    question.Score = score;
                }
                else
                {
                    result.Errors.Add($"درجة السؤال غير صالحة في الصف {row.RowNumber()}.");
                    return result;
                }

                // التحقق من مدة السؤال (اختياري)
                if (!string.IsNullOrEmpty(durationString) && int.TryParse(durationString, out int duration))
                {
                    question.DurationSeconds = duration;
                }
                else
                {
                    question.DurationSeconds = 0;
                }

                question.RequiresManualGrading = manualGradingString.Equals(YesString, StringComparison.OrdinalIgnoreCase);
                question.Options = new List<QuestionOption>();

                if (question.QuestionType == QuestionType.MultipleChoice)
                {
                    var option1Text = row.Cell(6).GetString().Trim();
                    var option2Text = row.Cell(7).GetString().Trim();
                    var option3Text = row.Cell(8).GetString().Trim();
                    var option4Text = row.Cell(9).GetString().Trim();
                    var correctAnswerText = row.Cell(10).GetString().Trim();

                    var optionTexts = new List<string> { option1Text, option2Text, option3Text, option4Text }
                                                     .Where(o => !string.IsNullOrEmpty(o)).ToList();

                    foreach (var optText in optionTexts)
                    {
                        var option = new QuestionOption
                        {
                            Text = optText,
                            IsCorrect = optText.Equals(correctAnswerText, StringComparison.OrdinalIgnoreCase)
                        };
                        question.Options.Add(option);
                    }

                    if (question.Options.Count(o => o.IsCorrect) != 1)
                    {
                        result.Errors.Add($"يجب أن يكون هناك خيار صحيح واحد فقط في السؤال في الصف {row.RowNumber()}.");
                        return result;
                    }
                }

                result.Questions.Add(question);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"حدث خطأ أثناء معالجة الصف {row.RowNumber()}: {ex.Message}");
            }

            return result;
        }

        // دالة لتنظيف الموارد غير المستخدمة بعد انتهاء عمل المتحكم.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
