//using Repositories;
//using Repositories.Models;
//using Services.IServices;
//using Services.SubscriptionPlan;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Services
//{
//    public class SubscriptionPlanService : ISubscriptionPlanService
//    {
//        private readonly SubscriptionPlanRepository _repo;

//        public SubscriptionPlanService()
//        {
//            _repo = new SubscriptionPlanRepository();
//        }

//        //public async Task<List<SubscriptionPlanDTO>> GetAllAsync()
//        //{
//        //    var list = await _repo.GetAllAsync();
//        //    return list.Select(p => new SubscriptionPlanDTO
//        //    {
//        //        Id = p.Id,
//        //        Name = p.Name,
//        //        Price = p.Price,
//        //        DurationInMonths = p.DurationInMonths,
//        //        MaxWorkSpaces = p.MaxWorkSpaces,
//        //        Description = p.Description,
//        //        IsActive = p.IsActive
//        //    }).ToList();
//        //}

//        //public async Task<SubscriptionPlanDTO> GetByIdAsync(Guid id)
//        //{
//        //    var p = await _repo.GetByIdAsync(id);
//        //    if (p == null) return null;
//        //    return new SubscriptionPlanDTO
//        //    {
//        //        Id = p.Id,
//        //        Name = p.Name,
//        //        Price = p.Price,
//        //        DurationInMonths = p.DurationInMonths,
//        //        MaxWorkSpaces = p.MaxWorkSpaces,
//        //        Description = p.Description,
//        //        IsActive = p.IsActive
//        //    };
//        //}

//        public async Task<int> CreateAsync(SubscriptionPlanDTO dto)
//        {
//            var plan = new SubscriptionPlanDTO
//            {
//                Id = Guid.NewGuid(),
//                Name = dto.Name,
//                Price = dto.Price,
//                DurationInMonths = dto.DurationInMonths,
//                MaxWorkSpaces = dto.MaxWorkSpaces,
//                Description = dto.Description,
//                IsActive = dto.IsActive,
//                //Created_At = DateTime.UtcNow
//            };
//            return await _repo.CreateAsync(plan);
//        }

//        public async Task<int> UpdateAsync(SubscriptionPlanDTO dto)
//        {
//            var plan = await _repo.GetByIdAsync(dto.Id);
//            if (plan == null) return 0;

//            plan.Name = dto.Name;
//            plan.Price = dto.Price;
//            plan.DurationInMonths = dto.DurationInMonths;
//            plan.MaxWorkSpaces = dto.MaxWorkSpaces;
//            plan.Description = dto.Description;
//            plan.IsActive = dto.IsActive;
//            plan.Updated_At = DateTime.UtcNow;

//            return await _repo.UpdateAsync(plan);
//        }

//        public async Task<bool> DeleteAsync(Guid id)
//        {
//            var plan = await _repo.GetByIdAsync(id);
//            if (plan == null) return false;
//            return await _repo.DeleteAsync(id);
//        }

//        public Task<List<SubscriptionPlanDTO>> GetAllAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<SubscriptionPlanDTO> GetByIdAsync(Guid id)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
