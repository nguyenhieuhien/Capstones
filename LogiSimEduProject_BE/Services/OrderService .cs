using Repositories;
using Repositories.Models;
using Services.DTO.Order;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _repo;
        private readonly SubscriptionPlanRepository _subscriptionPlanRepo;

        public OrderService(OrderRepository repo, SubscriptionPlanRepository subscriptionPlanRepo)
        {
            _repo = repo;
            _subscriptionPlanRepo = subscriptionPlanRepo;
        }

        public Task<List<Order>> GetAllAsync() => _repo.GetAllAsync();

        public Task<Order?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);

        public async Task<Order> CreateAsync(OrderDTOCreate dto)
        {
            var plan = await _subscriptionPlanRepo.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null)
                throw new Exception("Invalid subscription plan.");

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths((int)plan.DurationInMonths);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                AccountId = dto.AccountId,
                SubcriptionPlanId = dto.SubscriptionPlanId,
                Description = $"Đăng ký: {plan.Name} - {plan.Description}",
                TotalPrice = plan.Price,
                StartDate = startDate,
                EndDate = endDate,
                Status = 0,               // PENDING
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                OrderTime = DateTime.UtcNow
            };

            return await _repo.CreateAsync(order);
        }
        public async Task<bool> UpdateStatusAsync(Guid orderId, int newStatus)
        {
            var order = await _repo.GetByIdAsync(orderId);
            if (order == null) return false;

            order.Status = newStatus;
            return await _repo.UpdateAsync(order); // Đảm bảo OrderRepository có phương thức UpdateAsync
        }


        public Task<bool> DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }

}
