using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using System;

namespace Repositories
{
    public class SubscriptionPlanRepository
    {
        private readonly LogiSimEduContext _context;

        public SubscriptionPlanRepository(LogiSimEduContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAll()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.DeleteAt == null) // loại bỏ các bản ghi đã soft delete
                .ToListAsync();
        }
        public async Task<List<SubscriptionPlan>> GetAllActiveAsync()
        {
            return await _context.SubscriptionPlans
                .Where(p => p.DeleteAt == null && p.IsActive == true) // loại bỏ các bản ghi đã soft delete
                .ToListAsync();
        }


        public async Task<SubscriptionPlan> GetByIdAsync(Guid id)
        {
            return await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Id == id && p.DeleteAt == null);
        }


        public async Task<int> CreateAsync(SubscriptionPlan plan)
        {
            _context.SubscriptionPlans.Add(plan);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(SubscriptionPlan plan)
        {
            _context.SubscriptionPlans.Update(plan);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> RemoveAsync(SubscriptionPlan plan)
        {
            _context.SubscriptionPlans.Remove(plan);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
