// File: Services/IEnrollmentRequestService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    
    public class EnrollmentRequestService : IEnrollmentRequestService
    {
        private readonly EnrollmentRequestRepository _repository;

        public EnrollmentRequestService(EnrollmentRequestRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AccountOfCourse>> GetAll()
        {
            return await _repository.GetAll() ?? new List<AccountOfCourse>();
        }

        public async Task<AccountOfCourse?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetById(id);
        }

        public async Task<List<AccountOfCourse>> GetByCourseId(string courseId)
        {
            return await _repository.GetByCourseId(courseId);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(AccountOfCourse request)
        {
            if (request == null || request.AccountId == Guid.Empty || request.CourseId == Guid.Empty)
                return (false, "Invalid request data", null);

            request.Id = Guid.NewGuid();
            request.Status = 1;
            request.CreatedAt = DateTime.UtcNow;

            var result = await _repository.CreateAsync(request);
            return result > 0
                ? (true, "Enrollment request created", request.Id)
                : (false, "Failed to create enrollment request", null);
        }

        public async Task<(bool Success, string Message)> Update(AccountOfCourse request)
        {
            if (request == null || request.Id == Guid.Empty)
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(request);
            return result > 0 ? (true, "Request updated successfully") : (false, "Failed to update request");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetById(id);
            if (item == null)
                return (false, "Request not found");

            var result = await _repository.RemoveAsync(item);
            return result ? (true, "Deleted successfully") : (false, "Delete failed");
        }
    }
}
