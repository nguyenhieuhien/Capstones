using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuizService
    {
        Task<List<Quiz>> GetAll();
        Task<Quiz> GetById(string id);
        Task<int> Create(Quiz quiz);
        Task<int> CreateFullQuiz(Quiz dto);
        Task<int> Update(Quiz quiz);
        Task<bool> Delete(string id);
    }

    public class QuizService : IQuizService
    {
        private QuizRepository _repository;
        private QuestionRepository _questionRepo;
        private AnswerRepository _answerRepo;
        private LogiSimEduContext _dbContext;
        public QuizService(LogiSimEduContext dbContext)
        {
            _repository = new QuizRepository();
            _questionRepo = new QuestionRepository();
            _answerRepo = new AnswerRepository();
            _dbContext = dbContext;
        }
        public async Task<int> Create(Quiz quiz)
        {
            quiz.Id = Guid.NewGuid();
            return await _repository.CreateAsync(quiz);
        }

        public async Task<int> CreateFullQuiz(Quiz dto)
        {
            dto.Id = Guid.NewGuid();
            dto.CreatedAt = DateTime.UtcNow;

            foreach (var question in dto.Questions)
            {
                question.Id = Guid.NewGuid();
                question.QuizId = dto.Id;
                question.CreatedAt = DateTime.UtcNow;

                int correctCount = question.Answers.Count(a => a.IsAnswerCorrect == true);
                if (correctCount != 1)
                    throw new Exception("Each question must have exactly one correct answer.");

                foreach (var answer in question.Answers)
                {
                    answer.Id = Guid.NewGuid();
                    answer.QuestionId = question.Id;
                    answer.CreatedAt = DateTime.UtcNow;
                }
            }

            // Gắn quiz + liên kết navigation là đủ
            await _dbContext.Quizzes.AddAsync(dto);

            // Gọi SaveChanges đúng 1 lần duy nhất
            await _dbContext.SaveChangesAsync();

            return 1;
        }

        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                return await _repository.RemoveAsync(item);
            }

            return false;
        }

        public async Task<List<Quiz>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Quiz> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Quiz quiz)
        {
            return await _repository.UpdateAsync(quiz);
        }

    }
}
