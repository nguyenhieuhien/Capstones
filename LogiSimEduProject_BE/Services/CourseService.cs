// File: Services/ICourseService.cs
using Repositories;
using Repositories.Models;
using Services.DTO.Course;
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

        public async Task<CourseDTO?> GetById(Guid id)
        {
            var course = await _repository.GetCourseByIdAsync(id);

            if (course == null)
                return null;

            // Mapping sang DTO (có thể dùng AutoMapper hoặc thủ công)
            return new CourseDTO
            {
                Id = course.Id,
                CategoryId = course.CategoryId,
                WorkSpaceId = course.WorkSpaceId,
                InstructorId = course.InstructorId,
                CourseName = course.CourseName,
                Description = course.Description,
                RatingAverage = course.RatingAverage,
                ImgUrl = course.ImgUrl,
                InstructorFullName = course.Instructor.FullName // <-- thay vì Id
            };
        }

        public async Task<List<Course>> GetAllByOrgId(Guid orgId)
        {
            return await _repository.GetAllByOrgId(orgId);
        }

        public async Task<string?> GetInstructorFullNameAsync(Guid courseId)
        {
            var fullName = await _repository.GetInstructorFullNameByCourseIdAsync(courseId);

            if (string.IsNullOrEmpty(fullName))
            {
                // Nếu muốn 404 thì throw, nếu không thì return null
                throw new KeyNotFoundException("Instructor not found for the given CourseId");
            }

            return fullName;
        }

        public async Task<List<Course>> GetAllByWorkspaceId(Guid workspaceId)
        {
            return await _repository.GetAllByWorkspaceId(workspaceId) ?? new List<Course>();
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

        public async Task<(bool Success, string Message)> Update(CourseDTO dto)
        {
            var course = await _repository.GetCourseByIdAsync(dto.Id);
            if (course == null)
                return (false, "Course not found");

            course.CourseName = dto.CourseName;
            course.Description = dto.Description;
            course.RatingAverage = dto.RatingAverage;
            course.CategoryId = dto.CategoryId;
            course.WorkSpaceId = dto.WorkSpaceId;
            course.InstructorId = dto.InstructorId;
            course.ImgUrl = dto.ImgUrl;
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

            item.IsActive = false;
            item.DeleteAt = DateTime.UtcNow;

            await _repository.UpdateAsync(item);
            return (true, "Deleted successfully");
        }

        public async Task<List<Course>> Search(string name, string description)
        {
            return await _repository.Search(name, description);
        }
    }
}
