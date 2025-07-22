//using Microsoft.EntityFrameworkCore;
//using Repositories.DBContext;
//using Repositories.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Repositories
//{
//    public class SubscriptionPlanRepository
//    {
//        private readonly LogiSimEduContext _context = new LogiSimEduContext();

//        public async Task<List<SubscriptionPlan>> GetAllAsync()
//        {
//            return await _context.SubscriptionPlans.ToListAsync();
//        }

//        public async Task<SubscriptionPlan> GetByIdAsync(Guid id)
//        {
//            return await _context.SubscriptionPlans.FindAsync(id);
//        }

//        public async Task<int> CreateAsync(SubscriptionPlan plan)
//        {
//            _context.SubscriptionPlans.Add(plan);
//            return await _context.SaveChangesAsync();
//        }

//        public async Task<int> UpdateAsync(SubscriptionPlan plan)
//        {
//            _context.SubscriptionPlans.Update(plan);
//            return await _context.SaveChangesAsync();
//        }

//        public async Task<bool> DeleteAsync(Guid id)
//        {
//            var plan = await _context.SubscriptionPlans.FindAsync(id);
//            if (plan == null) return false;
//            //plan.Delete_At = DateTime.UtcNow;
//            return await _context.SaveChangesAsync() > 0;
//        }
//    }
//}
