using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAnswerService
    {
        Task<List<Answer>> GetAll();
        Task<Answer> GetById(string id);
        Task<int> Create(Answer answer);
        Task<int> Update(Answer answer);
        Task<bool> Delete(string id);
    }

    public class AnswerService : IAnswerService
    {
        private AnswerRepository _repository;

        public AnswerService()
        {
            _repository = new AnswerRepository();
        }
        public async Task<int> Create(Answer answer)
        {
            return await _repository.CreateAsync(answer);
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

        public async Task<List<Answer>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Answer> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Answer answer)
        {
            return await _repository.UpdateAsync(answer);
        }

    }
}
