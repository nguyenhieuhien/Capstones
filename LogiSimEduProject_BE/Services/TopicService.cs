using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ITopicService
    {
        Task<List<Topic>> GetAll();
        Task<Topic> GetById(string id);
        Task<int> Create(Topic topic);
        Task<int> Update(Topic topic);
        Task<bool> Delete(string id);
    }

    public class TopicService : ITopicService
    {
        private TopicRepository _repository;

        public TopicService()
        {
            _repository = new TopicRepository();
        }
        public async Task<int> Create(Topic topic)
        {
            return await _repository.CreateAsync(topic);
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

        public async Task<List<Topic>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Topic> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Topic topic)
        {
            return await _repository.UpdateAsync(topic);
        }

    }
}
