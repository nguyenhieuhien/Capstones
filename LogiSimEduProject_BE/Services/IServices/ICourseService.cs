using Repositories.Models;
using Services.DTO.Course;
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
        Task<List<Course>> GetCoursesByInstructorId(Guid instructorId);
        Task<List<Course>> GetCoursesByCategoryId(Guid categoryId);
        Task<Course?> GetById(Guid id);
        Task<List<Course>> GetAllByOrgId(Guid orgId);
        Task<string?> GetInstructorFullNameAsync(Guid courseId);
        Task<(bool Success, string Message, Guid? Id)> Create(Course course);
        Task<(bool Success, string Message)> Update(Course course);
        Task<(bool Success, string Message)> Delete(string id);
        Task<List<Course>> Search(string name, string description);
        Task<List<Course>> GetAllByWorkspaceId(Guid workspaceId);
        Task<List<Course>> GetAllByCategoryId(Guid categoryId);
    }
}
