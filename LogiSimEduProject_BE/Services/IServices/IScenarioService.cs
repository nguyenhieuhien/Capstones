using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IScenarioService
    {
        Task<List<Scenario>> GetAll();
        Task<Scenario?> GetById(string id);
        Task<List<Scenario>> GetAllByOrgId(Guid orgId);
        Task<int> Create(Scenario scenario);
        Task<int> Update(Scenario scenario);
        Task<(bool Success, string Message)> Delete(string id);
    }
}
