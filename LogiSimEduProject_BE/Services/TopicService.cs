using Repositories;
using Repositories.Models;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public class TopicService : ITopicService
    {
        private TopicRepository _repository;

        public TopicService()
        {
            _repository = new TopicRepository();
        }
        public async Task<int> Create(Topic topic)
        {
            topic.Id = Guid.NewGuid();
            return await _repository.CreateAsync(topic);
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                item.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(item);
                return (true, "Deleted successfully");
            }

            return (false, "Topic not found");
        }

        public async Task<List<Topic>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Topic> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Topic>> GetTopicsByCourseId(Guid courseId)
        {
            return await _repository.GetTopicsByCourseIdAsync(courseId);
        }

        public async Task<List<Topic>> GetProcessTopicsByCourseId(Guid courseId, Guid? accountId = null, int? progressStatus = null)
        {
            return await _repository.GetProcessTopicsByCourseIdAsync(courseId, accountId, progressStatus);
        }

        public async Task<int> Update(Topic topic)
        {
            return await _repository.UpdateAsync(topic);
        }

    }
}
