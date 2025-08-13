using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ICourseProgressService
    {
        Task<List<CourseProgress>> GetAll();
        Task<CourseProgress?> GetById(string id);
        Task<CourseProgress?> GetByAccIdandCourseId(Guid accId, Guid courseId);
        Task<(bool Success, string Message, Guid? Id)> Create(CourseProgress courseProgress);
        Task<(bool Success, string Message)> Update(CourseProgress courseProgress);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
