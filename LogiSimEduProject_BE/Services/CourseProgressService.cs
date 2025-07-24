using Repositories.Models;
using Repositories;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CourseProgressService : ICourseProgressService
    {
        private readonly CourseProgressRepository _repository;

        public CourseProgressService(CourseProgressRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CourseProgress>> GetAll()
        {
            return await _repository.GetAll() ?? new List<CourseProgress>();
        }

        public async Task<CourseProgress?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(CourseProgress request)
        {
            try
            {
                var courseProgress = new CourseProgress
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.AccountId,
                    CourseId = request.CourseId,
                    ProgressPercent = request.ProgressPercent,
                    Status = request.Status,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.CreateAsync(courseProgress);
                if (result > 0)
                    return (true, "Course Progress created successfully", courseProgress.Id);
                return (false, "Failed to create course Progress", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> Update(CourseProgress request)
        {
            try
            {
                var courseProgress = new CourseProgress
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.AccountId,
                    CourseId = request.CourseId,
                    ProgressPercent = request.ProgressPercent,
                    Status = request.Status,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await _repository.UpdateAsync(courseProgress);
                if (result > 0)
                    return (true, "Course Progress updated successfully");
                return (false, "Failed to update course Progress");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Course Progress not found");

            var result = await _repository.RemoveAsync(item);
            return result ? (true, "Deleted successfully") : (false, "Delete failed");
        }
    }
}
