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
        Task<CourseDTO?> GetById(Guid id);
        Task<List<Course>> GetAllByOrgId(Guid orgId);
        Task<string?> GetInstructorFullNameAsync(Guid courseId);
        Task<(bool Success, string Message, Guid? Id)> Create(Course course);
        Task<(bool Success, string Message)> Update(CourseDTO course);
        Task<(bool Success, string Message)> Delete(string id);
        Task<List<Course>> Search(string name, string description);
        Task<List<Course>> GetAllByWorkspaceId(Guid workspaceId);
    }
}
