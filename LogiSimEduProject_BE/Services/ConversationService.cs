using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IConversationService
    {
        Task<List<Conversation>> GetAll();
        Task<Conversation> GetById(string id);
        Task<int> Create(Conversation conversation);
        Task<int> Update(Conversation conversation);
        Task<bool> Delete(string id);

    }

    public class ConversationService : IConversationService
    {
        private ConversationRepository _repository;

        public ConversationService()
        {
            _repository = new ConversationRepository();
        }
        public async Task<int> Create(Conversation conversation)
        {
            conversation.Id = Guid.NewGuid();
            return await _repository.CreateAsync(conversation);
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

        public async Task<List<Conversation>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Conversation> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Conversation conversation)
        {
            return await _repository.UpdateAsync(conversation);
        }
    }
}
