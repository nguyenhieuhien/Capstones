using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.DTO.Object;
using Services.IServices;
using Microsoft.EntityFrameworkCore;
namespace Services
{

    public class ObjectService : IObjectService
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

        public async Task<List<ObjectGetDto>> GetAllByScenarioIdAsync(Guid scenarioId)
        {
            var objects = await _dbContext.ObjectModels
                .Include(o => o.Methods)
                    .ThenInclude(m => m.Scripts)
                .Where(o => o.ScenarioId == scenarioId && o.IsActive == true)
                .ToListAsync();

            var result = objects.Select(o => new ObjectGetDto
            {
                Id = o.Id,
                ObjectName = o.ObjectName,
                Description = o.Description,
                MethodGets = o.Methods
                    .Where(m => m.IsActive == true)
                    .Select(m => new MethodGetDto
                    {
                        Id = m.Id,
                        MethodName = m.MethodName,
                        Description = m.Description,
                        ScriptGets = m.Scripts
                            .Where(s => s.IsActive == true)
                            .Select(s => new ScriptGetDto
                            {
                                Id = s.Id,
                                ScriptName = s.ScriptName,
                                Code = s.Code
                            }).ToList()
                    }).ToList()
            }).ToList();

            return result;
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

        public async Task<List<ObjectModel>> UpdateManyByScenarioAsync(Guid scenarioId, ObjectUpdateListDto dto)
        {
            var existingObjects = await _dbContext.ObjectModels
        .Include(o => o.Methods)
        .ThenInclude(m => m.Scripts)
        .Where(o => o.ScenarioId == scenarioId && o.IsActive == true)
        .ToListAsync();

            foreach (var objDto in dto.ObjectUpdates)
            {
                // Tìm object
                var obj = existingObjects.FirstOrDefault(o => o.Id == objDto.Id);
                if (obj == null) continue; // Không có thì bỏ qua

                // Update Object
                obj.ObjectName = objDto.ObjectName;
                obj.Description = objDto.Description;
                obj.UpdatedAt = DateTime.UtcNow;

                _dbContext.Entry(obj).State = EntityState.Modified;

                // Update Methods
                foreach (var methodDto in objDto.MethodUpdates)
                {
                    var method = obj.Methods.FirstOrDefault(m => m.Id == methodDto.Id);
                    if (method == null) continue; // Không có thì bỏ qua

                    method.MethodName = methodDto.MethodName;
                    method.Description = methodDto.Description;
                    method.UpdatedAt = DateTime.UtcNow;

                    _dbContext.Entry(method).State = EntityState.Modified;

                    // Update Scripts
                    foreach (var scriptDto in methodDto.ScriptUpdates)
                    {
                        var script = method.Scripts.FirstOrDefault(s => s.Id == scriptDto.Id);
                        if (script == null) continue; // Không có thì bỏ qua

                        script.ScriptName = scriptDto.ScriptName;
                        script.Code = scriptDto.Code;
                        script.UpdatedAt = DateTime.UtcNow;

                        _dbContext.Entry(script).State = EntityState.Modified;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            return existingObjects;
        }

        public async Task<bool> SoftDeleteObjectAsync(Guid objectId)
        {
            var obj = await _dbContext.ObjectModels
                .Include(o => o.Methods)
                    .ThenInclude(m => m.Scripts)
                .FirstOrDefaultAsync(o => o.Id == objectId);

            if (obj == null || obj.IsActive == false)
                return false;

            // Soft delete object
            obj.IsActive = false;
            obj.DeleteAt = DateTime.Now;
            _dbContext.Entry(obj).State = EntityState.Modified;

            // Soft delete methods
            foreach (var method in obj.Methods)
            {
                method.IsActive = false;
                method.DeleteAt = DateTime.Now;
                _dbContext.Entry(method).State = EntityState.Modified;

                // Soft delete scripts
                foreach (var script in method.Scripts)
                {
                    script.IsActive = false;
                    script.DeleteAt = DateTime.Now;
                    _dbContext.Entry(script).State = EntityState.Modified;
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
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
