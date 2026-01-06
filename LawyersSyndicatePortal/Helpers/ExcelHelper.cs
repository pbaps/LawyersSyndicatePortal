using ClosedXML.Excel;
using LawyersSyndicatePortal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LawyersSyndicatePortal.Helpers
{
    public static class ExcelHelper
    {
        // Helper method to export questions to Excel
        public static MemoryStream ExportQuestionsToExcel(IEnumerable<Question> questions)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("الأسئلة");
                var currentRow = 1;

                // Add headers
                worksheet.Cell(currentRow, 1).Value = "نص السؤال";
                worksheet.Cell(currentRow, 2).Value = "نوع السؤال";
                worksheet.Cell(currentRow, 3).Value = "الدرجة";
                worksheet.Cell(currentRow, 4).Value = "مدة السؤال (بالثواني)";
                worksheet.Cell(currentRow, 5).Value = "يحتاج تصحيح يدوي";
                worksheet.Cell(currentRow, 6).Value = "نص الخيار";
                worksheet.Cell(currentRow, 7).Value = "هل الخيار صحيح";

                foreach (var question in questions)
                {
                    if (question.Options != null && question.Options.Any())
                    {
                        foreach (var option in question.Options)
                        {
                            currentRow++;
                            // Question data
                            worksheet.Cell(currentRow, 1).Value = question.Text;
                            worksheet.Cell(currentRow, 2).Value = GetQuestionTypeString(question.QuestionType); // Correct conversion
                            worksheet.Cell(currentRow, 3).Value = question.Score;
                            worksheet.Cell(currentRow, 4).Value = question.DurationSeconds;
                            worksheet.Cell(currentRow, 5).Value = question.RequiresManualGrading ? "نعم" : "لا";

                            // Option data
                            worksheet.Cell(currentRow, 6).Value = option.Text;
                            worksheet.Cell(currentRow, 7).Value = option.IsCorrect ? "نعم" : "لا";
                        }
                    }
                    else
                    {
                        // For questions with no options (e.g., TextAnswer)
                        currentRow++;
                        worksheet.Cell(currentRow, 1).Value = question.Text;
                        worksheet.Cell(currentRow, 2).Value = GetQuestionTypeString(question.QuestionType); // Correct conversion
                        worksheet.Cell(currentRow, 3).Value = question.Score;
                        worksheet.Cell(currentRow, 4).Value = question.DurationSeconds;
                        worksheet.Cell(currentRow, 5).Value = question.RequiresManualGrading ? "نعم" : "لا";
                    }
                }

                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return stream;
            }
        }

        // Helper method to import questions from Excel
        public static IEnumerable<Question> ImportQuestionsFromExcel(Stream stream, int examId)
        {
            var questionsToImport = new Dictionary<string, Question>();
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // Skip headers

                var lastQuestionText = "";
                foreach (var row in rows)
                {
                    var questionText = row.Cell(1).GetString().Trim();
                    var questionTypeString = row.Cell(2).GetString().Trim();
                    var optionText = row.Cell(6).GetString().Trim();
                    var isCorrectString = row.Cell(7).GetString().Trim();

                    Question question;
                    if (!string.IsNullOrEmpty(questionText))
                    {
                        lastQuestionText = questionText;
                        question = new Question();

                        // Explicitly parse string to QuestionType
                        QuestionType questionType;
                        if (!Enum.TryParse(GetQuestionTypeEnum(questionTypeString), out questionType))
                        {
                            throw new InvalidOperationException($"Invalid question type in row {row.RowNumber()}");
                        }
                        question.QuestionType = questionType;
                        question.Text = questionText;

                        decimal score;
                        if (!decimal.TryParse(row.Cell(3).GetString().Trim(), out score))
                        {
                            throw new InvalidOperationException($"Invalid score in row {row.RowNumber()}");
                        }
                        question.Score = score;

                        int duration;
                        var durationString = row.Cell(4).GetString().Trim();
                        if (!string.IsNullOrEmpty(durationString) && int.TryParse(durationString, out duration))
                        {
                            question.DurationSeconds = duration;
                        }
                        else
                        {
                            question.DurationSeconds = 0;
                        }

                        question.RequiresManualGrading = isCorrectString == "نعم";
                        question.ExamId = examId;
                        question.Options = new List<QuestionOption>();
                        questionsToImport[questionText] = question;
                    }
                    else if (!string.IsNullOrEmpty(lastQuestionText) && questionsToImport.ContainsKey(lastQuestionText))
                    {
                        question = questionsToImport[lastQuestionText];
                    }
                    else
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(optionText))
                    {
                        question.Options.Add(new QuestionOption
                        {
                            Text = optionText,
                            IsCorrect = isCorrectString == "نعم"
                        });
                    }
                }
            }

            return questionsToImport.Values;
        }

        // Helper to convert QuestionType to a display string
        private static string GetQuestionTypeString(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.MultipleChoice:
                    return "خيار متعدد";
                case QuestionType.TextAnswer:
                    return "إجابة نصية";
                default:
                    return "غير محدد";
            }
        }

        // Helper to convert display string to QuestionType enum
        private static string GetQuestionTypeEnum(string typeString)
        {
            switch (typeString)
            {
                case "خيار متعدد":
                    return "MultipleChoice";
                case "إجابة نصية":
                    return "TextAnswer";
                default:
                    return null;
            }
        }
    }
}
