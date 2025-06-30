using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IMessageService
    {
        Task<List<Message>> GetAll();
        Task<Message> GetById(string id);
        Task<int> Create(Message message);
        Task<int> Update(Message message);
        Task<bool> Delete(string id);
    }

    public class MessageService : IMessageService
    {
        private MessageRepository _repository;

        public MessageService()
        {
            _repository = new MessageRepository();
        }
        public async Task<int> Create(Message message)
        {
            message.Id = Guid.NewGuid();
            return await _repository.CreateAsync(message);
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

        public async Task<List<Message>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Message> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Message message)
        {
            return await _repository.UpdateAsync(message);
        }

    }
}
