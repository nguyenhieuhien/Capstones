using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly SubscriptionPlanRepository _repository;

        public SubscriptionPlanService(SubscriptionPlanRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SubscriptionPlan>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<SubscriptionPlan> GetById(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> Create(SubscriptionPlan plan)
        {
            plan.Id = Guid.NewGuid();
            plan.CreatedAt = DateTime.UtcNow;
            plan.IsActive = true;

            var result = await _repository.CreateAsync(plan);
            return result > 0 ? (true, "Created successfully") : (false, "Failed to create subscription plan");
        }

        public async Task<(bool Success, string Message)> Update(SubscriptionPlan plan)
        {
            plan.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(plan);
            return result > 0 ? (true, "Updated successfully") : (false, "Failed to update subscription plan");
        }

        public async Task<(bool Success, string Message)> Delete(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Subscription plan not found");

            item.DeleteAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(item); // soft delete
            return result > 0 ? (true, "Deleted successfully") : (false, "Failed to delete subscription plan");
        }
    }
}
