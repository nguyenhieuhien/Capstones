// File: Services/ICourseService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
  

    public class CourseService : ICourseService
    {
        private readonly CourseRepository _repository;

        public CourseService(CourseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Course>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Course>();
        }

        public async Task<Course?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Course course)
        {
            if (course == null || string.IsNullOrWhiteSpace(course.CourseName))
                return (false, "Invalid course data", null);

            course.Id = Guid.NewGuid();
            course.CreatedAt = DateTime.UtcNow;
            course.IsActive = true;

            var result = await _repository.CreateAsync(course);
            return result > 0
                ? (true, "Course created successfully", course.Id)
                : (false, "Failed to create course", null);
        }

        public async Task<(bool Success, string Message)> Update(Course course)
        {
            if (course == null || course.Id == Guid.Empty || string.IsNullOrWhiteSpace(course.CourseName))
                return (false, "Invalid update data");

            course.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(course);
            return result > 0 ? (true, "Updated successfully") : (false, "Update failed");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Course not found");

            var result = await _repository.RemoveAsync(item);
            return result ? (true, "Deleted successfully") : (false, "Delete failed");
        }

        public async Task<List<Course>> Search(string name, string description)
        {
            return await _repository.Search(name, description);
        }
    }
}
