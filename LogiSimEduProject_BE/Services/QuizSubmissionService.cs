using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuizSubmissionService
    {
        Task<double> SubmitQuizAsync(QuizSubmission submission, List<QuizSubmissionAnswer> answers);
    }

    public class QuizSubmissionService : IQuizSubmissionService
    {
        private readonly QuizSubmissionRepository _repository;

        public QuizSubmissionService()
        {
            _repository = new QuizSubmissionRepository();
        }

        public async Task<double> SubmitQuizAsync(QuizSubmission submission, List<QuizSubmissionAnswer> answers)
        {
            await _repository.CreateAsync(submission);
            await _repository.AddSubmissionAnswers(answers);

            var correctAnswers = await _repository.GetCorrectAnswersByQuiz(submission.QuizId);

            int correctCount = answers.Count(sa =>
                correctAnswers.Any(ca => ca.QuestionId == sa.QuestionId && ca.Id == sa.AnswerId));

            int totalQuestions = correctAnswers.Select(c => c.QuestionId).Distinct().Count();

            double score = totalQuestions == 0 ? 0 : (double)correctCount / totalQuestions * 10;
            submission.ScoreObtained = (int)Math.Round(score, 2);

            await _repository.UpdateAsync(submission);

            return (double)(submission.ScoreObtained ?? 0);
        }
    }
}
