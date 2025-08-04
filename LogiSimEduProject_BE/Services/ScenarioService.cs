// File: Services/ScenarioService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
  

    public class ScenarioService : IScenarioService
    {
        private readonly ScenarioRepository _repository;

        public ScenarioService()
        {
            _repository = new ScenarioRepository();
        }

        public async Task<List<Scenario>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Scenario>();
        }

        public async Task<Scenario?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Create(Scenario scenario)
        {
            if (scenario == null || string.IsNullOrWhiteSpace(scenario.ScenarioName))
                return 0;

            scenario.Id = Guid.NewGuid();
            scenario.IsActive = true;
            scenario.CreatedAt = DateTime.UtcNow;
            scenario.UpdatedAt = null;

            return await _repository.CreateAsync(scenario);
        }

        public async Task<int> Update(Scenario scenario)
        {
            if (scenario == null || scenario.Id == Guid.Empty || string.IsNullOrWhiteSpace(scenario.ScenarioName))
                return 0;

            var existing = await _repository.GetByIdAsync(scenario.Id);
            if (existing == null) return 0;

            scenario.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(scenario);
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {

            var scenario = await _repository.GetByIdAsync(id);
            if (scenario == null) return (false, "Scenario not found");

            scenario.IsActive = false;
            scenario.DeleteAt = DateTime.UtcNow;

            await _repository.UpdateAsync(scenario);
            return (true, "Deleted successfully");
        }
    }
}
