using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using LawyersSyndicatePortal.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity;
using LawyersSyndicatePortal.ViewModels;
using LawyersSyndicatePortal.Filters;

namespace LawyersSyndicatePortal.Controllers
{
    // المتحكم الخاص بإدارة أخذ الامتحانات للمستخدمين (المحامين)
    [Authorize]
    public class ExamTakerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [PermissionAuthorizationFilter("عرض الامتحانات المتاحة", "صلاحية عرض قائمة بجميع الامتحانات المتاحة للمستخدم")]
        [AuditLog("عرض صفحة الامتحانات المتاحة", "عرض صفحة الامتحانات المتاحة")]
        public async Task<ActionResult> Index()
        {
            // الحصول على معرف المستخدم الحالي
            var userId = User.Identity.GetUserId();
            // الحصول على تفاصيل المستخدم من قاعدة البيانات
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                // إذا لم يكن لدى المستخدم رقم محامي مرتبط، يمكن توجيهه لصفحة تخبره بعدم وجود اختبارات.
                return View("NoExamAvailable");
            }

            // استرجاع جميع الاختبارات التي يمكن للمستخدم حضورها
            // (حيث CanAttend == true)
            var allExams = await db.ExamAttendees
                .Include(ea => ea.Exam)
                .Where(ea => ea.LawyerIdNumber == user.LinkedLawyerIdNumber && ea.CanAttend)
                .Select(ea => ea.Exam)
                .ToListAsync();

            // استرجاع نتائج الاختبارات المكتملة للمستخدم باستخدام رقم المحامي الصحيح
            var userResults = await db.ExamResults
                .Where(r => r.Lawyer.IdNumber == user.LinkedLawyerIdNumber)
                .ToListAsync();

            // تحديد الاختبارات المكتملة:
            // 1. تلك التي لها نتيجة مسجلة في جدول ExamResults.
            var completedExams = allExams
                .Where(e => userResults.Any(r => r.ExamId == e.Id))
                .ToList();

            // تحديد الاختبارات المنتهية صلاحيتها:
            // 2. تلك التي انتهى تاريخها ولم يقم المستخدم بأدائها.
            var now = DateTime.Now;
            // تم تحديث الشرط ليعكس أن ExamDateTime من النوع غير القابل للقيمة الفارغة
            var expiredExams = allExams
                .Where(e => e.ExamDateTime.AddMinutes(e.DurationMinutes) < now && !completedExams.Any(ce => ce.Id == e.Id))
                .ToList();

            // دمج الاختبارات المكتملة والمنتهية في قائمة واحدة
            var combinedCompletedExams = completedExams.Union(expiredExams).ToList();

            // تحديد الاختبارات المتاحة حاليًا
            // وهي الاختبارات التي لم تكتمل ولم تنته صلاحيتها
            var activeExams = allExams
                .Where(e => !combinedCompletedExams.Any(ce => ce.Id == e.Id))
                .ToList();

            // إنشاء نموذج العرض الجديد وإرساله إلى الـ View
            var viewModel = new ExamListViewModel
            {
                ActiveExams = activeExams,
                CompletedExams = combinedCompletedExams
            };

            return View(viewModel);
        }

        [PermissionAuthorizationFilter("عدم توفر امتحان", "صلاحية عرض صفحة عدم توفر الامتحانات")]
        [AuditLog("عرض صفحة عدم توفر امتحان", "عرض صفحة عدم توفر امتحان")]
        public ActionResult NoExamAvailable()
        {
            return View();
        }

        /// <summary>
        /// الإجراء الجديد لعرض صفحة تفاصيل الاختبار قبل البدء الفعلي.
        /// هذا الإجراء سيقوم بجلب جميع المعلومات اللازمة للعرض مثل اسم الاختبار،
        /// التعليمات، المدة، درجة النجاح، وعدد الأسئلة.
        /// </summary>
        /// <param name="examId">معرف الاختبار.</param>
        /// <returns>صفحة View تعرض تفاصيل الاختبار.</returns>
        [PermissionAuthorizationFilter("عرض تفاصيل الامتحان", "صلاحية عرض تفاصيل امتحان محدد قبل البدء")]
        [AuditLog("عرض تفاصيل الامتحان", "عرض تفاصيل الامتحان")]
        public async Task<ActionResult> ExamDetails(int examId)
        {
            // التحقق من وجود الاختبار
            var exam = await db.Exams.FindAsync(examId);
            if (exam == null)
            {
                return HttpNotFound();
            }

            // جلب عدد الأسئلة الكلي للاختبار
            var totalQuestionCount = await db.Questions
                                                 .Where(q => q.ExamId == examId)
                                                 .CountAsync();

            // إنشاء ViewModel جديد يحتوي على البيانات اللازمة للصفحة
            var viewModel = new ExamDetailsViewModel
            {
                Exam = exam,
                TotalQuestions = totalQuestionCount
            };

            // إرجاع الـ View مع ViewModel الجديد
            return View(viewModel);
        }

        /// <summary>
        /// الإجراء الذي يبدأ الاختبار فعليًا.
        /// </summary>
        [PermissionAuthorizationFilter("بدء الاختبار", "صلاحية بدء الامتحان وأخذ أول سؤال")]
        [AuditLog("بدء اختبار", "بدء اختبار")]
        public async Task<ActionResult> TakeExam(int examId)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Lawyer not linked.");
            }

            var examAttendee = await db.ExamAttendees
                                                 .Include(ea => ea.Exam)
                                                 .SingleOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == user.LinkedLawyerIdNumber);

            if (examAttendee == null || !examAttendee.CanAttend)
            {
                return HttpNotFound();
            }

            if (examAttendee.IsCompleted)
            {
                return RedirectToAction("ExamCompleted", new { id = examId });
            }

            if (!examAttendee.StartTime.HasValue)
            {
                examAttendee.StartTime = DateTime.Now;
                await db.SaveChangesAsync();
            }

            var lawyer = await db.Lawyers.SingleOrDefaultAsync(l => l.IdNumber == user.LinkedLawyerIdNumber);

            if (lawyer == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Lawyer not found.");
            }

            var answeredQuestionIds = await db.Answers
                                                     .Where(a => a.ExamAttendeeId == examAttendee.Id)
                                                     .Select(a => a.QuestionId)
                                                     .ToListAsync();

            var question = await db.Questions
                                         .Include(q => q.Options)
                                         .Where(q => q.ExamId == examId && !answeredQuestionIds.Contains(q.Id))
                                         .OrderBy(q => q.Id)
                                         .FirstOrDefaultAsync();

            if (question == null)
            {
                return RedirectToAction("SubmitExam", new { examId = examId });
            }

            var answeredQuestionCount = answeredQuestionIds.Count;
            var totalQuestionCount = await db.Questions.CountAsync(q => q.ExamId == examId);
            var timePassed = (DateTime.Now - examAttendee.StartTime.Value).TotalSeconds;
            var timeRemaining = (examAttendee.Exam.DurationMinutes * 60) - timePassed;

            if (timeRemaining < 0)
            {
                return RedirectToAction("SubmitExam", new { examId = examId });
            }

            var viewModel = new ExamViewModel
            {
                Exam = examAttendee.Exam,
                Question = question,
                CurrentQuestionNumber = answeredQuestionCount + 1,
                TotalQuestions = totalQuestionCount,
                TimeRemainingSeconds = (int)timeRemaining,
                Lawyer = lawyer
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("تسليم الإجابة", "صلاحية إرسال إجابة سؤال واحد في الامتحان")]
        [AuditLog("إرسال إجابة سؤال", "إرسال إجابة سؤال")]
        public async Task<ActionResult> SubmitAnswer(int examId, int questionId, int? selectedOptionId, string userAnswer = null)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var examAttendee = await db.ExamAttendees.SingleOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == user.LinkedLawyerIdNumber);

            if (examAttendee == null)
            {
                return HttpNotFound();
            }

            var question = await db.Questions.SingleOrDefaultAsync(q => q.Id == questionId && q.ExamId == examId);
            if (question == null)
            {
                return HttpNotFound();
            }

            var existingAnswer = await db.Answers.SingleOrDefaultAsync(a => a.ExamAttendeeId == examAttendee.Id && a.QuestionId == questionId);
            if (existingAnswer != null)
            {
                return RedirectToAction("TakeExam", new { examId = examId });
            }

            var isCorrect = false;
            var answerText = "";

            // التحقق من نوع الإجابة
            if (selectedOptionId.HasValue)
            {
                // إذا كانت إجابة اختيار من متعدد
                var selectedOption = await db.QuestionOptions.SingleOrDefaultAsync(o => o.Id == selectedOptionId.Value);
                if (selectedOption == null)
                {
                    return HttpNotFound();
                }
                isCorrect = selectedOption.IsCorrect;
                answerText = selectedOption.Text;
            }
            else if (!string.IsNullOrEmpty(userAnswer))
            {
                // إذا كانت إجابة نصية
                isCorrect = false; // الإجابات النصية عادة لا تُصحح تلقائياً
                answerText = userAnswer;
            }
            else
            {
                // لا يوجد اختيار أو إجابة نصية
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No answer provided.");
            }

            var answer = new Answer
            {
                ExamAttendeeId = examAttendee.Id,
                QuestionId = questionId,
                IsCorrect = isCorrect,
                UserAnswer = answerText
            };

            db.Answers.Add(answer);
            await db.SaveChangesAsync();

            return RedirectToAction("TakeExam", new { examId = examId });
        }

        [PermissionAuthorizationFilter("تسليم الامتحان", "صلاحية إنهاء الامتحان وتسليم الإجابات")]
        [AuditLog("تسليم الامتحان", "تسليم الامتحان")]
        public async Task<ActionResult> SubmitExam(int examId)
        {
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null || string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var examAttendee = await db.ExamAttendees
                                                 .Include(ea => ea.Exam)
                                                 .SingleOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == user.LinkedLawyerIdNumber);

            if (examAttendee == null)
            {
                return HttpNotFound();
            }

            var totalQuestions = await db.Questions.CountAsync(q => q.ExamId == examId);
            var answeredQuestionsCount = await db.Answers.CountAsync(a => a.ExamAttendeeId == examAttendee.Id);

            examAttendee.IsCompleted = true;
            await db.SaveChangesAsync();

            // *** تم تعديل هذه السطر ليتوافق مع اسم المعلمة في دالة ExamCompleted ***
            return RedirectToAction("ExamCompleted", new { id = examId });
        }

        /*
                 [HttpPost]
                 [ValidateAntiForgeryToken]
                 public async Task<ActionResult> GradeManualQuestion(int examAttendeeId, int questionId, decimal score)
                 {
                     // يجب إضافة تحقق هنا للتأكد من أن المستخدم الحالي لديه صلاحية المصحح
                     // على سبيل المثال: if (!User.IsInRole("Proctor")) { return new HttpStatusCodeResult(403); }

                     try
                     {
                         // 1. جلب الإجابة التي سيتم تصحيحها
                         var answerToGrade = await db.Answers
                             .Include(a => a.Question)
                             .SingleOrDefaultAsync(a => a.ExamAttendeeId == examAttendeeId && a.QuestionId == questionId);

                         if (answerToGrade == null)
                         {
                             return HttpNotFound();
                         }

                         // 2. تحديث درجة الإجابة الفردية
                         answerToGrade.IsCorrect = (score > 0); // يمكن أن تعتمد على منطق آخر
                         answerToGrade.AchievedScore = score;
                         answerToGrade.IsGradingComplete = true; // يتم تحديثها لتشير إلى أن هذا السؤال تم تصحيحه يدوياً

                         // 3. جلب كل إجابات الطالب لإعادة حساب المجموع الكلي
                         var allAnswers = await db.Answers
                             .Where(a => a.ExamAttendeeId == examAttendeeId)
                             .ToListAsync();

                         // 4. إعادة حساب الدرجة الإجمالية للملتحق بالاختبار
                         var totalAchievedScore = allAnswers.Sum(a => a.AchievedScore);

                         // 5. جلب النتيجة النهائية للطالب
                         var examResult = await db.ExamResults.SingleOrDefaultAsync(r => r.ExamAttendeeId == examAttendeeId);

                         if (examResult == null)
                         {
                             return HttpNotFound();
                         }

                         // 6. التحقق مما إذا تم الانتهاء من تصحيح جميع الأسئلة اليدوية
                         bool allManualQuestionsGraded = !allAnswers.Any(a => !a.IsGradingComplete && !a.Question.IsAutoGraded);

                         // 7. تحديث النتيجة النهائية في جدول ExamResult
                         examResult.TotalScoreAchieved = totalAchievedScore;

                         if (allManualQuestionsGraded)
                         {
                             examResult.IsGradingComplete = true;
                             // قم بحساب حالة النجاح بناءً على الدرجة النهائية
                             var passPercentage = (double)examResult.PassPercentage / 100;
                             examResult.IsPassed = (totalAchievedScore >= (examResult.TotalExamScore * (decimal)passPercentage));
                         }

                         await db.SaveChangesAsync();

                         // يمكن إرجاع JSON هنا للتحديث الديناميكي للواجهة
                         return Json(new { success = true, newScore = totalAchievedScore, isComplete = examResult.IsGradingComplete });
                     }
                     catch (Exception ex)
                     {
                         // يمكنك تسجيل الخطأ هنا
                         return Json(new { success = false, message = ex.Message });
                     }
                 }
         */
        // 🎯 تم تحديث هذه الدالة لإضافة حسابات جديدة وعرضها في صفحة النتائج
        [PermissionAuthorizationFilter("عرض نتيجة الامتحان", "صلاحية عرض صفحة نتائج الامتحان المكتمل")]
        [AuditLog("عرض صفحة نتيجة الامتحان", "عرض صفحة نتيجة الامتحان")]
        public async Task<ActionResult> ExamCompleted(int id)
        {
            // الحصول على معرف المستخدم الحالي
            var userId = User.Identity.GetUserId();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            // التحقق من وجود المستخدم وربط رقم المحامي
            if (user == null || string.IsNullOrEmpty(user.LinkedLawyerIdNumber))
            {
                return HttpNotFound();
            }

            // البحث عن بيانات حضور الامتحان للمحامي المحدد
            // 🎯 تم تحديث الكود لإضافة Include(ea => ea.Lawyer)
            var examAttendee = await db.ExamAttendees
                .Include(ea => ea.Exam)
                .Include(ea => ea.ExamResult)
                .Include(ea => ea.Lawyer) // هذا السطر هو الحل!
                .SingleOrDefaultAsync(ea => ea.ExamId == id && ea.LawyerIdNumber == user.LinkedLawyerIdNumber);

            // التحقق من وجود بيانات الحضور وأن الامتحان قد اكتمل
            if (examAttendee == null || !examAttendee.IsCompleted)
            {
                return HttpNotFound();
            }

            // الحصول على النتيجة الحالية، إن وجدت
            ExamResult existingResult = examAttendee.ExamResult;

            // إذا لم تكن هناك نتيجة سابقة، قم بإنشاء واحدة وحساب الدرجات الأولية
            if (existingResult == null)
            {
                // جلب جميع إجابات المستخدم لهذا الامتحان
                var answeredQuestions = await db.Answers
                    .Include(a => a.Question)
                    .Where(a => a.ExamAttendeeId == examAttendee.Id)
                    .ToListAsync();

                // فصل الأسئلة إلى فئتين: يتم تصحيحها آليًا وتلك التي تحتاج إلى تصحيح يدوي
                var autoGradedAnswers = answeredQuestions.Where(a =>
                    a.Question.QuestionType == QuestionType.MultipleChoice).ToList();

                var manualGradedAnswers = answeredQuestions.Where(a =>
                    a.Question.QuestionType == QuestionType.TextAnswer).ToList();

                // حساب الدرجة الأولية بناءً على الأسئلة التي يتم تصحيحها آليًا
                var correctAutoAnswers = autoGradedAnswers.Count(a => a.IsCorrect);
                var totalAutoGradedQuestions = autoGradedAnswers.Count;

                decimal initialScore = 0;
                if (totalAutoGradedQuestions > 0)
                {
                    // حساب الدرجة الأولية من الأسئلة ذات الاختيار من متعدد
                    // يتم توزيع الدرجة الكلية على كل الأسئلة بالتساوي
                    var pointsPerQuestion = examAttendee.Exam.MaxScore / (decimal)examAttendee.Exam.Questions.Count;
                    initialScore = correctAutoAnswers * pointsPerQuestion;
                }

                // تحديد ما إذا كان التصحيح اليدوي مطلوبًا
                bool needsManualGrading = manualGradedAnswers.Any();

                // إنشاء كائن نتيجة امتحان جديد في قاعدة البيانات
                var newExamResult = new ExamResult
                {
                    ExamAttendeeId = examAttendee.Id,
                    ExamId = id,
                    LawyerIdNumber = user.LinkedLawyerIdNumber,
                    TotalExamScore = examAttendee.Exam.MaxScore,
                    TotalScoreAchieved = initialScore,
                    IsPassed = false,
                    IsGradingComplete = !needsManualGrading,
                    PassPercentage = examAttendee.Exam.PassingScorePercentage
                };

                db.ExamResults.Add(newExamResult);
                await db.SaveChangesAsync();
                existingResult = newExamResult;
            }

            // 🎯 حسابات إضافية جديدة لعرض تفاصيل أكثر دقة
            // جلب إجمالي عدد الأسئلة لكل نوع في الامتحان
            var totalMultipleChoiceQuestions = await db.Questions.CountAsync(q => q.ExamId == id && q.QuestionType == QuestionType.MultipleChoice);
            var totalTextQuestions = await db.Questions.CountAsync(q => q.ExamId == id && q.QuestionType == QuestionType.TextAnswer);

            // جلب إجمالي عدد الأسئلة التي أجاب عليها المستخدم
            var totalAnsweredQuestions = await db.Answers.CountAsync(a => a.ExamAttendeeId == examAttendee.Id);

            // جلب عدد الأسئلة التي تم الإجابة عليها بشكل صحيح
            var correctAnswersCount = await db.Answers.CountAsync(a => a.ExamAttendeeId == examAttendee.Id && a.IsCorrect);

            // جلب إجمالي الأسئلة في الاختبار
            var totalQuestions = await db.Questions.CountAsync(q => q.ExamId == id);

            // إنشاء وإرجاع نموذج العرض (ViewModel)
            var viewModel = new ExamResultViewModel
            {
                Exam = examAttendee.Exam,
                ExamAttendee = examAttendee, // إضافة كائن ExamAttendee إلى النموذج
                TotalScoreAchieved = (double)existingResult.TotalScoreAchieved,
                TotalExamScore = (double)existingResult.TotalExamScore,
                IsGradingComplete = existingResult.IsGradingComplete,

                // 🎯 إضافة الخصائص الجديدة للنموذج
                TotalQuestions = totalQuestions,
                CorrectAnswersCount = correctAnswersCount,
                TotalAnsweredQuestions = totalAnsweredQuestions,
                TotalMultipleChoiceQuestions = totalMultipleChoiceQuestions,
                TotalTextQuestions = totalTextQuestions
            };

            // حساب النسبة المئوية للنجاح وتحديث حالة النجاح
            viewModel.IsPassed = viewModel.PassPercentage >= (double)examAttendee.Exam.PassingScorePercentage;

            return View(viewModel);
        }

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
