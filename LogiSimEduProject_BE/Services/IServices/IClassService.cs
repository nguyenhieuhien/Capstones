using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IClassService
    {
        Task<List<Class>> GetAll();
        Task<Class?> GetById(string id);
        Task<List<Class>> GetAllClassByCourseId(Guid courseId);
        Task<List<Class>> GetAllClassByInstructorId(Guid accountId);
        Task<Class> GetClassByAccountAndCourseAsync(Guid accountId, Guid courseId);
        Task<(bool Success, string Message, Guid? Id)> Create(Class _class);
        Task<(bool Success, string Message)> Update(Class _class);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
