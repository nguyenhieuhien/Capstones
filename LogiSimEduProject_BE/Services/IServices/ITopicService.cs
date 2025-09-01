using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ITopicService
    {
        Task<List<Topic>> GetAll();
        Task<Topic> GetById(string id);
        Task<List<Topic>> GetTopicsByCourseId(Guid courseId);
        Task<List<Topic>> GetProcessTopicsByCourseId(Guid courseId, Guid? accountId = null, int? progressStatus = null);
        Task<int> Create(Topic topic);
        Task<int> Update(Topic topic);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
