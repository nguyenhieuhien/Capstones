using Repositories.Models;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.IServices;

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
