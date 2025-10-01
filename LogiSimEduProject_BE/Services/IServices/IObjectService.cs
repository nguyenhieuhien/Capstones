using Repositories.Models;
using Services.DTO.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IObjectService
    {
        Task<List<ObjectModel>> GetAll();
        Task<ObjectModel?> GetById(string id);
        Task<List<ObjectGetDto>> GetAllByScenarioIdAsync(Guid scenarioId);
        Task<List<ObjectModel>> CreateObjectsAsync(List<ObjectDto> dtos);
        Task<List<ObjectModel>> UpdateManyByScenarioAsync(Guid scenarioId, ObjectUpdateListDto dto);
        Task<bool> SoftDeleteObjectAsync(Guid objectId);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
