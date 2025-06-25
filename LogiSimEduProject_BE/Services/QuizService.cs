using Repositories;
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
        Task<int> Update(Quiz quiz);
        Task<bool> Delete(string id);
    }

    public class QuizService : IQuizService
    {
        private QuizRepository _repository;

        public QuizService()
        {
            _repository = new QuizRepository();
        }
        public async Task<int> Create(Quiz quiz)
        {
            return await _repository.CreateAsync(quiz);
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
