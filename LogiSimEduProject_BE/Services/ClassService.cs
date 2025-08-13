// File: Services/IClassService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
 

    public class ClassService : IClassService
    {
        private readonly ClassRepository _repository;

        public ClassService(ClassRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Class>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Class>();
        }

        public async Task<Class?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Class> GetClassByAccountAndCourseAsync(Guid accountId, Guid courseId)
        {
            return await _repository.GetClassByAccountAndCourseAsync(accountId, courseId);
        }

        public async Task<List<Class>> GetAllClassByCourseId(Guid courseId)
        {
            if (courseId == Guid.Empty) return new List<Class>();
            return await _repository.GetByCourseAsync(courseId) ?? new List<Class>();
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Class _class)
        {
            if (_class == null || string.IsNullOrWhiteSpace(_class.ClassName))
                return (false, "Invalid class data", null);

            _class.Id = Guid.NewGuid();
            _class.CreatedAt = DateTime.UtcNow;
            _class.IsActive = true;

            var result = await _repository.CreateAsync(_class);
            return result > 0
                ? (true, "Class created successfully", _class.Id)
                : (false, "Failed to create class", null);
        }

        public async Task<(bool Success, string Message)> Update(Class _class)
        {
            if (_class == null || _class.Id == Guid.Empty || string.IsNullOrWhiteSpace(_class.ClassName))
                return (false, "Invalid update data");

            _class.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(_class);
            return result > 0 ? (true, "Updated successfully") : (false, "Update failed");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Class not found");

            item.IsActive = false;
            item.DeleteAt = DateTime.UtcNow;

            await _repository.UpdateAsync(item);
            return (true, "Deleted successfully");
        }
    }
}
