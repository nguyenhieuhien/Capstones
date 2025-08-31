using Repositories.Models;

namespace Services.IServices
{
    public interface ISubscriptionPlanService
    {
        Task<List<SubscriptionPlan>> GetAll();
        Task<List<SubscriptionPlan>> GetAllActive();
        Task<SubscriptionPlan> GetById(Guid id);
        Task<(bool Success, string Message)> Create(SubscriptionPlan plan);
        Task<(bool Success, string Message)> Update(SubscriptionPlan plan);
        Task<(bool Success, string Message)> Delete(Guid id);
    }
}
