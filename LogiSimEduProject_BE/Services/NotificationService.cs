// File: Services/NotificationService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    

    public class NotificationService : INotificationService
    {
        private readonly NotificationRepository _repository;

        public NotificationService()
        {
            _repository = new NotificationRepository();
        }

        public async Task<List<Notification>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Notification?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Notification notification)
        {
            try
            {
                notification.Id = Guid.NewGuid();
                notification.CreatedAt = DateTime.UtcNow;
                notification.IsActive = true;

                var result = await _repository.CreateAsync(notification);
                if (result > 0)
                    return (true, "Notification created successfully", notification.Id);
                return (false, "Failed to create notification", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> Update(Notification notification)
        {
            try
            {
                notification.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(notification);
                if (result > 0)
                    return (true, "Notification updated successfully");
                return (false, "Failed to update notification");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var notification = await _repository.GetByIdAsync(id);
                if (notification == null)
                    return (false, "Notification not found");

                var result = await _repository.RemoveAsync(notification);
                if (result)
                    return (true, "Notification deleted successfully");
                return (false, "Failed to delete notification");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
