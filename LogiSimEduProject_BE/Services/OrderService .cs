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
            // 1. Lấy plan từ repository
            var plan = await _subscriptionPlanRepo.GetByIdAsync(dto.SubscriptionPlanId);
            if (plan == null)
                throw new Exception("Invalid subscription plan.");

            // 2. Tính ngày kết thúc
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths((int)plan.DurationInMonths);

            // 3. Tạo Order
            var order = new Order
            {
                OrganizationId = dto.OrganizationId,
                AccountId = dto.AccountId,
                SubcriptionPlanId = dto.SubscriptionPlanId,
                Description = $"Đăng ký: {plan.Name} - {plan.Description}",
                TotalPrice = plan.Price, // lấy đúng giá theo plan
                StartDate = startDate,
                EndDate = endDate,
                Status = 1,        // pending
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                OrderTime = DateTime.UtcNow
            };

            return await _repo.CreateAsync(order);
        }


        //public async Task<Order?> UpdateAsync(Guid id, OrderDTOUpdate dto)
        //{
        //    var existing = await _repo.GetByIdAsync(id);
        //    if (existing == null) return null;

        //    existing.OrganizationId = dto.OrganizationId;
        //    existing.AccountId = dto.AccountId;
        //    existing.SubcriptionPlanId = dto.SubscriptionPlanId;
        //    existing.Description = dto.Description;
        //    existing.TotalPrice = dto.TotalPrice;
        //    existing.StartDate = dto.StartDate;
        //    existing.EndDate = dto.EndDate;
        //    existing.Status = dto.Status;
        //    existing.IsActive = dto.IsActive;

        //    return await _repo.UpdateAsync(existing);
        //}

        public Task<bool> DeleteAsync(Guid id) => _repo.DeleteAsync(id);
    }

}
