using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface INotificationService
    {
        Task<List<Notification>> GetAll();
        Task<Notification?> GetById(string id);
        Task<(bool Success, string Message, Guid? Id)> Create(Notification notification);
        Task<(bool Success, string Message)> Update(Notification notification);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
