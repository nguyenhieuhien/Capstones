// File: Services/IAccountOfWorkSpaceService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
 
    public class AccountOfWorkSpaceService : IAccountOfWorkSpaceService
    {
        private readonly AccountOfWorkSpaceRepository _repository;

        public AccountOfWorkSpaceService(AccountOfWorkSpaceRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AccountOfWorkSpace>> GetAll() => await _repository.GetAll();

        public async Task<AccountOfWorkSpace> GetById(string id) => await _repository.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> Create(AccountOfWorkSpace accWs)
        {
            accWs.Id = Guid.NewGuid();
            accWs.IsActive = true;
            accWs.CreatedAt = DateTime.UtcNow;
            var result = await _repository.CreateAsync(accWs);
            return result > 0 ? (true, "Created successfully") : (false, "Failed to create record");
        }

        public async Task<(bool Success, string Message)> Update(AccountOfWorkSpace accWs)
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
