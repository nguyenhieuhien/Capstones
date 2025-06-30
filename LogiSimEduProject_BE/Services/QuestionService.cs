using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IQuestionService
    {
        Task<List<Question>> GetAll();
        Task<Question> GetById(string id);
        Task<int> Create(Question question);
        Task<int> Update(Question question);
        Task<bool> Delete(string id);
    }

    public class QuestionService : IQuestionService
    {
        private QuestionRepository _repository;

        public QuestionService()
        {
            _repository = new QuestionRepository();
        }
        public async Task<int> Create(Question question)
        {
            question.Id = Guid.NewGuid();
            return await _repository.CreateAsync(question);
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

        public async Task<List<Question>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Question> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Question question)
        {
            return await _repository.UpdateAsync(question);
        }

    }
}
