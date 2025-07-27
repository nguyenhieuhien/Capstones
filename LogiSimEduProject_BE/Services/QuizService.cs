// File: Services/QuizService.cs
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.DTO.Answer;
using Services.DTO.Question;
using Services.DTO.QuizSubmission;
using Services.IServices;

namespace Services
{
    

    public class QuizService : IQuizService
    {
        private readonly QuizRepository _repository;
        private readonly QuestionRepository _questionRepo;
        private readonly AnswerRepository _answerRepo;
        private readonly LogiSimEduContext _dbContext;

        public QuizService(LogiSimEduContext dbContext)
        {
            _repository = new QuizRepository();
            _questionRepo = new QuestionRepository();
            _answerRepo = new AnswerRepository();
            _dbContext = dbContext;
        }

        public async Task<List<Quiz>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Quiz>();
        }

        public async Task<Quiz?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<QuestionWithAnswersDTO>> GetQuestionsWithAnswersByQuizId(Guid quizId)
        {
            var questions = await _questionRepo.GetQuestionsWithAnswersByQuizId(quizId);

            var result = questions.Select(q => new QuestionWithAnswersDTO
            {
                Id = q.Id,
                Description = q.Description,
                Answers = q.Answers.Select(a => new AnswerDTO
                {
                    Id = a.Id,
                    Description = a.Description
                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Quiz quiz)
        {
            try
            {
                quiz.Id = Guid.NewGuid();
                quiz.CreatedAt = DateTime.UtcNow;
                quiz.IsActive = true;
                var result = await _repository.CreateAsync(quiz);
                if (result > 0)
                    return (true, "Quiz created successfully", quiz.Id);
                return (false, "Failed to create quiz", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> CreateFullQuiz(Quiz dto)
        {
            try
            {
                dto.Id = Guid.NewGuid();
                dto.CreatedAt = DateTime.UtcNow;
                dto.IsActive = true;

                foreach (var question in dto.Questions)
                {
                    question.Id = Guid.NewGuid();
                    question.QuizId = dto.Id;
                    question.IsActive = true;
                    question.CreatedAt = DateTime.UtcNow;

                    int correctCount = question.Answers.Count(a => a.IsCorrect == true);
                    if (correctCount != 1)
                        return (false, "Each question must have exactly one correct answer.");

                    foreach (var answer in question.Answers)
                    {
                        answer.Id = Guid.NewGuid();
                        answer.QuestionId = question.Id;
                        answer.IsActive = true;
                        answer.CreatedAt = DateTime.UtcNow;
                    }
                }

                await _dbContext.Quizzes.AddAsync(dto);
                await _dbContext.SaveChangesAsync();

                return (true, "Quiz with questions and answers created successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<QuizReviewDTO>> GetQuizReview(Guid accountId, Guid quizId)
        {
            var questions = await _repository.GetQuestionsWithAnswersByQuizId(quizId);
            var submissions = await _repository.GetQuestionSubmissions(accountId, quizId);

            var result = questions.Select(q => new QuizReviewDTO
            {
                QuestionId = q.Id,
                QuestionDescription = q.Description,
                Answers = q.Answers.Select(a => new AnswerReviewDTO
                {
                    AnswerId = a.Id,
                    Description = a.Description,
                    IsCorrect = a.IsCorrect ?? false
                }).ToList(),
                SelectedAnswerId = submissions.FirstOrDefault(s => s.QuestionId == q.Id)?.SelectedAnswerId
            }).ToList();

            return result;
        }

        public async Task<(bool Success, string Message)> Update(Quiz quiz)
        {
            try
            {
                quiz.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(quiz);
                if (result > 0)
                    return (true, "Quiz updated successfully");
                return (false, "Failed to update quiz");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null)
                    return (false, "Quiz not found");

                var result = await _repository.RemoveAsync(item);
                if (result)
                    return (true, "Quiz deleted successfully");
                return (false, "Failed to delete quiz");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
