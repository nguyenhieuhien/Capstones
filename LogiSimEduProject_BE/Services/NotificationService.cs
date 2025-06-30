using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetAll();
        Task<Notification> GetById(string id);
        Task<int> Create(Notification notification);
        Task<int> Update(Notification notification);
        Task<bool> Delete(string id);
    }

    public class NotificationService : INotificationService
    {
        private NotificationRepository _repository;

        public NotificationService()
        {
            _repository = new NotificationRepository();
        }
        public async Task<int> Create(Notification notification)
        {
            notification.Id = Guid.NewGuid();
            return await _repository.CreateAsync(notification);
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

        public async Task<List<Notification>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Notification> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Notification notification)
        {
            return await _repository.UpdateAsync(notification);
        }

    }
}
