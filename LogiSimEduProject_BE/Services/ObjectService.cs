using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DTO.Object;
namespace Services
{

    public class ObjectService
    {
        private readonly ObjectRepository _objectRepo;
        private readonly LogiSimEduContext _dbContext;
        public ObjectService(ObjectRepository objectRepo, LogiSimEduContext dbContext)
        {
            _dbContext = dbContext;
            _objectRepo = objectRepo;
        }

        public async Task<List<ObjectModel>> GetAll()
        {
            return await _objectRepo.GetAll();
        }

        public async Task<ObjectModel?> GetById(string id)
        {
            return await _objectRepo.GetByIdAsync(id);
        }

        public async Task<List<ObjectModel>> CreateObjectsAsync(List<ObjectDto> dtos)
        {
            var objects = dtos.Select(dto => new ObjectModel
            {
                Id = Guid.NewGuid(),
                ScenarioId = dto.ScenarioId,
                ObjectName = dto.ObjectName,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Methods = dto.Methods?.Select(m => new Method
                {
                    Id = Guid.NewGuid(),
                    MethodName = m.MethodName,
                    Description = m.Description,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Scripts = m.Scripts?.Select(s => new Script
                    {
                        Id = Guid.NewGuid(),
                        ScriptName = s.ScriptName,
                        Code = s.Code,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }).ToList()
                }).ToList()
            }).ToList();

            await _dbContext.ObjectModels.AddRangeAsync(objects);
            await _dbContext.SaveChangesAsync();

            return objects;
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var item = await _objectRepo.GetByIdAsync(id);
                if (item == null)
                    return (false, "Object not found");

                item.IsActive = false;
                item.DeleteAt = DateTime.UtcNow;

                await _objectRepo.UpdateAsync(item);
                return (true, "Object deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
