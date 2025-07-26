using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Repositories
{
    public class OrderRepository
    {
        private readonly LogiSimEduContext _context;

        public OrderRepository(LogiSimEduContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Where(o => o.DeleteAt == null)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.DeleteAt == null);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.Id = Guid.NewGuid();
            order.CreatedAt = DateTime.UtcNow;
            order.OrderTime = DateTime.UtcNow;
            order.Status = 1; // pending mặc định
            order.IsActive = true;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        //public async Task<Order?> UpdateAsync(Order updatedOrder)
        //{
        //    var existing = await GetByIdAsync(updatedOrder.Id);
        //    if (existing == null) return null;

        //    existing.OrganizationId = updatedOrder.OrganizationId;
        //    existing.AccountId = updatedOrder.AccountId;
        //    existing.SubcriptionPlanId = updatedOrder.SubcriptionPlanId;
        //    existing.Description = updatedOrder.Description;
        //    existing.TotalPrice = updatedOrder.TotalPrice;
        //    existing.StartDate = updatedOrder.StartDate;
        //    existing.EndDate = updatedOrder.EndDate;
        //    existing.Status = updatedOrder.Status;
        //    existing.IsActive = updatedOrder.IsActive;
        //    existing.UpdatedAt = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();
        //    return existing;
        //}

        public async Task<bool> DeleteAsync(Guid id)
        {
            var order = await GetByIdAsync(id);
            if (order == null) return false;

            order.DeleteAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
