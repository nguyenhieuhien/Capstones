using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ICourseService
    {
        Task<List<Course>> GetAll();
        Task<Course?> GetById(string id);
        Task<List<Course>> GetAllByOrgId(Guid orgId);
        Task<(bool Success, string Message, Guid? Id)> Create(Course course);
        Task<(bool Success, string Message)> Update(Course course);
        Task<(bool Success, string Message)> Delete(string id);
        Task<List<Course>> Search(string name, string description);
        Task<List<Course>> GetAllByWorkspaceId(Guid workspaceId);
    }
}
