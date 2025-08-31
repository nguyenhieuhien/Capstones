using Repositories.Models;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.IServices;
using Services.DTO.QuizSubmission;

namespace Services
{

    public class QuizSubmissionService : IQuizSubmissionService
    {
        private readonly QuizSubmissionRepository _submissionRepo;
        private readonly QuizSubmissionAnswerRepository _answerRepo;
        private readonly QuestionRepository _questionRepo;

        public QuizSubmissionService()
        {
            _submissionRepo = new QuizSubmissionRepository();
            _answerRepo = new QuizSubmissionAnswerRepository();
            _questionRepo = new QuestionRepository();
        }

        public async Task<List<QuizSubmission>> GetAllSubmissionByQuizId(Guid quizId)
        {
            return await _submissionRepo.GetByQuizIdAsync(quizId);
        }

        public async Task<List<QuizResultByClassDto>> GetLessonQuizSubmissionsGroupedByClass(Guid quizId)
        {
            var submissions = await _submissionRepo.GetLessonQuizSubmissions(quizId);
            if (submissions == null || submissions.Count == 0)
                return new List<QuizResultByClassDto>();

            // 1) Mỗi học viên (AccountId) chỉ giữ 1 bản nộp mới nhất cho quiz này
            var latestByStudent = submissions
                .GroupBy(s => s.AccountId)
                .Select(g => g
                    .OrderByDescending(x => x.SubmitTime)      // mới nhất theo thời gian nộp
                    .ThenByDescending(x => x.Id)               // tie-breaker nếu cùng SubmitTime
                    .First())
                .ToList();

            // Lấy courseId của quiz (tất cả bản ghi cùng quiz -> cùng course)
            var courseId = latestByStudent.FirstOrDefault()?.Quiz?.Lesson?.Topic?.CourseId;

            // 2) Group theo tên lớp trong course đó
            var grouped = latestByStudent
                .Select(s => new
                {
                    ClassName = s.Account?.AccountOfCourses?
                        .Where(aoc => courseId != null && aoc.CourseId == courseId)
                        .Select(aoc => aoc.Class?.ClassName)
                        .FirstOrDefault() ?? "Unassigned",
                    Row = new StudentQuizDTO
                    {
                        AccountId = s.AccountId,
                        FullName = s.Account?.FullName,
                        QuizId = s.QuizId,
                        QuizName = s.Quiz?.QuizName,
                        SubmitTime = s.SubmitTime,
                        TotalScore = s.TotalScore
                    }
                })
                .GroupBy(x => x.ClassName)
                .Select(g => new QuizResultByClassDto
                {
                    ClassName = g.Key,
                    Students = g.Select(x => x.Row)
                                 .OrderByDescending(r => r.SubmitTime) // tuỳ thích
                                 .ToList()
                })
                .OrderBy(g => g.ClassName)
                .ToList();

            return grouped;
        }

        public async Task<int> SubmitQuiz(Guid quizId, Guid accountId, List<(Guid questionId, Guid answerId)> answers)
        {
            var submission = new QuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                AccountId = accountId,
                IsActive = true,
                SubmitTime = DateTime.UtcNow,
            };

            await _submissionRepo.CreateAsync(submission);

            int correctCount = 0;

            foreach (var (questionId, answerId) in answers)
            {
                var question = await _questionRepo.GetByIdAsync(questionId.ToString());
                var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect == true);

                if (correctAnswer?.Id == answerId)
                    correctCount++;

                var submissionAnswer = new QuestionSubmission
                {
                    Id = Guid.NewGuid(),
                    QuizSubmissionId = submission.Id,
                    QuestionId = questionId,
                    IsActive = true,
                    SelectedAnswerId = answerId
                };

                await _answerRepo.CreateAsync(submissionAnswer);
            }
            var totalQuestions = answers.Count;
            var score = Math.Round((double)correctCount / totalQuestions * 10, 2);
            submission.TotalScore = score;

            await _submissionRepo.UpdateAsync(submission); 

            return correctCount;
        }
    }
}
