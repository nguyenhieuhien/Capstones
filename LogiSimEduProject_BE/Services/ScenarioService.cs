using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IScenarioService
    {
        Task<List<Scenario>> GetAll();
        Task<Scenario> GetById(string id);
        Task<int> Create(Scenario scenario);
        Task<int> Update(Scenario scenario);
        Task<bool> Delete(string id);
    }

    public class ScenarioService : IScenarioService
    {
        private ScenarioRepository _repository;

        public ScenarioService()
        {
            _repository = new ScenarioRepository();
        }
        public async Task<int> Create(Scenario scenario)
        {
            scenario.Id = Guid.NewGuid();
            return await _repository.CreateAsync(scenario);
        }

        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                return await _repository.RemoveAsync(item);
            }

            return false;
        }

        public async Task<List<Scenario>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Scenario> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Scenario scenario)
        {
            return await _repository.UpdateAsync(scenario);
        }

    }
}
