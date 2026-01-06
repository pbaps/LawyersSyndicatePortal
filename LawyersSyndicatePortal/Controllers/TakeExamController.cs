using LawyersSyndicatePortal.Filters;
using LawyersSyndicatePortal.Models;
using LawyersSyndicatePortal.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LawyersSyndicatePortal.Controllers
{
    [Authorize]
    public class TakeExamController : Controller
    {
        private ApplicationDbContext _context;

        public TakeExamController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        // GET: TakeExam/TakeExam?examId=5&questionId=1
        [PermissionAuthorizationFilter("عرض الاختبار", "صلاحية خوض الاختبار")]
        public async Task<ActionResult> TakeExam(int examId, int questionId)
        {
            var userId = User.Identity.GetUserId();
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var lawyerIdNumber = currentUser?.LinkedLawyerIdNumber;

            var examAttendee = await _context.ExamAttendees
                .Include(ea => ea.Exam)
                .Include(ea => ea.Lawyer)
                .FirstOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == lawyerIdNumber && !ea.IsCompleted);

            if (examAttendee == null || examAttendee.StartTime == null)
            {
                return RedirectToAction("ExamCompleted", new { examId = examId });
            }

            var endTime = examAttendee.StartTime.Value.AddMinutes(examAttendee.Exam.DurationMinutes);
            if (System.DateTime.Now > endTime)
            {
                examAttendee.IsCompleted = true;
                await _context.SaveChangesAsync();
                return RedirectToAction("ExamCompleted", new { examId = examId });
            }

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId && q.ExamId == examId);

            if (question == null)
            {
                return HttpNotFound();
            }

            var allQuestions = await _context.Questions
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            var currentQuestionIndex = allQuestions.FindIndex(q => q.Id == questionId) + 1;
            var totalQuestions = allQuestions.Count;

            var viewModel = new ExamViewModel
            {
                Exam = examAttendee.Exam,
                Question = question,
                CurrentQuestionNumber = currentQuestionIndex,
                TotalQuestions = totalQuestions,
                Lawyer = examAttendee.Lawyer,
                TimeRemainingSeconds = (int)(endTime - System.DateTime.Now).TotalSeconds,
            };

            return View(viewModel);
        }

        // POST: TakeExam/SubmitAnswer
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorizationFilter("إدارة الاختبارات", "صلاحية خوض الاختبار")]
        [AuditLog("إضافة", "تسجيل إجابة على سؤال في اختبار")]
        public async Task<ActionResult> SubmitAnswer(int examId, int questionId, string userAnswer, int? selectedOptionId)
        {
            var userId = User.Identity.GetUserId();
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var lawyerIdNumber = currentUser?.LinkedLawyerIdNumber;

            var examAttendee = await _context.ExamAttendees
                .Include(ea => ea.Exam)
                .FirstOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == lawyerIdNumber && !ea.IsCompleted);

            if (examAttendee == null)
            {
                return RedirectToAction("ExamCompleted", new { examId = examId });
            }

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
            {
                return HttpNotFound();
            }

            var answer = new Answer
            {
                ExamAttendeeId = examAttendee.Id,
                QuestionId = question.Id,
                UserAnswer = userAnswer,
                IsCorrect = false,
                AcquiredScore = 0,
                IsGraded = !question.RequiresManualGrading
            };

            if (!question.RequiresManualGrading)
            {
                // تم تصحيح طريقة المقارنة هنا
                if (question.QuestionType.ToString().Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase))
                {
                    var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                    if (correctOption != null && selectedOptionId.HasValue && selectedOptionId.Value == correctOption.Id)
                    {
                        answer.IsCorrect = true;
                        answer.AcquiredScore = question.Score;
                    }
                }
                // تم تصحيح طريقة المقارنة هنا
                else if (question.QuestionType.ToString().Equals("TextAnswer", StringComparison.OrdinalIgnoreCase))
                {
                    answer.IsGraded = false;
                }
            }

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            var allQuestions = await _context.Questions
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.Id)
                .ToListAsync();

            var currentQuestionIndex = allQuestions.FindIndex(q => q.Id == questionId);
            if (currentQuestionIndex < allQuestions.Count - 1)
            {
                var nextQuestionId = allQuestions[currentQuestionIndex + 1].Id;
                return RedirectToAction("TakeExam", new { examId = examId, questionId = nextQuestionId });
            }
            else
            {
                examAttendee.IsCompleted = true;
                examAttendee.EndTime = System.DateTime.Now;
                await _context.SaveChangesAsync();

                var examResult = await CalculateExamResult(examId, lawyerIdNumber);

                return RedirectToAction("ExamCompleted", new { examId = examId });
            }
        }

        // GET: TakeExam/ExamCompleted/5
        [PermissionAuthorizationFilter("إدارة الاختبارات", "صلاحية عرض نتائج الاختبار")]
        public async Task<ActionResult> ExamCompleted(int examId)
        {
            var userId = User.Identity.GetUserId();
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var lawyerIdNumber = currentUser?.LinkedLawyerIdNumber;

            var examAttendee = await _context.ExamAttendees.FirstOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == lawyerIdNumber);

            if (examAttendee == null || !examAttendee.IsCompleted)
            {
                return RedirectToAction("Index", "Exams");
            }

            var examResult = await _context.ExamResults
                                    .Include(er => er.Exam)
                                    .Include(er => er.Lawyer)
                                    .FirstOrDefaultAsync(er => er.ExamId == examId && er.LawyerIdNumber == lawyerIdNumber);

            if (examResult == null)
            {
                examResult = await CalculateExamResult(examId, lawyerIdNumber);
            }

            return View(examResult);
        }

        // دالة مساعدة لحساب نتيجة الاختبار
        private async Task<ExamResult> CalculateExamResult(int examId, string lawyerIdNumber)
        {
            var examAttendee = await _context.ExamAttendees.FirstOrDefaultAsync(ea => ea.ExamId == examId && ea.LawyerIdNumber == lawyerIdNumber);

            if (examAttendee == null)
                return null;

            var exam = await _context.Exams.FindAsync(examId);
            var answers = await _context.Answers
                                    .Include(a => a.Question)
                                    .Where(a => a.ExamAttendeeId == examAttendee.Id)
                                    .ToListAsync();

            var totalScoreAchieved = answers.Sum(a => a.AcquiredScore ?? 0);
            var totalExamScore = answers.Sum(a => a.Question.Score);
            var isGradingComplete = answers.All(a => a.IsGraded);
            var passPercentage = (totalScoreAchieved / totalExamScore) * 100;
            var isPassed = passPercentage >= exam.PassingScorePercentage;

            var examResult = new ExamResult
            {
                ExamAttendeeId = examAttendee.Id,
                ExamId = exam.Id,
                LawyerIdNumber = lawyerIdNumber,
                TotalExamScore = totalExamScore,
                TotalScoreAchieved = totalScoreAchieved,
                PassPercentage = passPercentage,
                IsPassed = isPassed,
                IsGradingComplete = isGradingComplete
            };

            var existingResult = await _context.ExamResults.FirstOrDefaultAsync(er => er.ExamAttendeeId == examAttendee.Id);
            if (existingResult == null)
            {
                _context.ExamResults.Add(examResult);
            }
            else
            {
                _context.Entry(existingResult).CurrentValues.SetValues(examResult);
            }

            await _context.SaveChangesAsync();
            return examResult;
        }
    }
}
