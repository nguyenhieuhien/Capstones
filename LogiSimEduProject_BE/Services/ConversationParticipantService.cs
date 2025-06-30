using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IConversationParticipantService
    {
        Task<List<ConversationParticipant>> GetAll();
        Task<ConversationParticipant> GetById(string id);
        Task<int> Create(ConversationParticipant converPar);
        Task<int> Update(ConversationParticipant converPar);
        Task<bool> Delete(string id);
    }

    public class ConversationParticipantService : IConversationParticipantService
    {
        private ConversationParticipantRepository _repository;

        public ConversationParticipantService()
        {
            _repository = new ConversationParticipantRepository();
        }
        public async Task<int> Create(ConversationParticipant converPar)
        {
            converPar.Id = Guid.NewGuid();
            return await _repository.CreateAsync(converPar);
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

        public async Task<List<ConversationParticipant>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<ConversationParticipant> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(ConversationParticipant converPar)
        {
            return await _repository.UpdateAsync(converPar);
        }

    }
}
