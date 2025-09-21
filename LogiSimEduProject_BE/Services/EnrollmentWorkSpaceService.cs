// File: Services/IAccountOfWorkSpaceService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
 
    public class EnrollmentWorkSpaceService : IEnrollmentWorkSpaceService
    {
        private readonly EnrollmentWorkSpaceRepository _repository;

        public EnrollmentWorkSpaceService(EnrollmentWorkSpaceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<EnrollmentWorkSpace>> GetAll() => await _repository.GetAll();

        public async Task<EnrollmentWorkSpace> GetById(string id) => await _repository.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> Create(EnrollmentWorkSpace accWs)
        {
            accWs.Id = Guid.NewGuid();
            accWs.IsActive = true;
            accWs.CreatedAt = DateTime.UtcNow;
            var result = await _repository.CreateAsync(accWs);
            return result > 0 ? (true, "Created successfully") : (false, "Failed to create record");
        }

        public async Task<(bool Success, string Message)> Update(EnrollmentWorkSpace accWs)
        {
            accWs.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(accWs);
            return result > 0 ? (true, "Updated successfully") : (false, "Update failed");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Item not found");

            var success = await _repository.RemoveAsync(item);
            return success ? (true, "Deleted successfully") : (false, "Failed to delete");
        }
    }
}
