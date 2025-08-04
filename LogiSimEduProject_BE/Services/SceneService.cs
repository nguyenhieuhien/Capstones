// File: Services/SceneService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
   

    public class SceneService : ISceneService
    {
        private readonly SceneRepository _repository;
        private readonly SceneOfWorkSpaceRepository _sceneWpRepo;

        public SceneService()
        {
            _repository = new SceneRepository();
            _sceneWpRepo = new SceneOfWorkSpaceRepository();
        }

        public async Task<List<Scene>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Scene>();
        }

        public async Task<Scene?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Create(Scene scene)
        {
            if (scene == null || string.IsNullOrWhiteSpace(scene.SceneName)) return 0;

            scene.Id = Guid.NewGuid();
            scene.IsActive = true;
            scene.CreatedAt = DateTime.UtcNow;
            scene.UpdatedAt = null;

            return await _repository.CreateAsync(scene);
        }

        public async Task<int> Update(Scene scene)
        {
            if (scene == null || scene.Id == Guid.Empty || string.IsNullOrWhiteSpace(scene.SceneName))
                return 0;

            var existing = await _repository.GetByIdAsync(scene.Id);
            if (existing == null) return 0;

            scene.UpdatedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(scene);
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            
            var scene = await _repository.GetByIdAsync(id);
            if (scene == null) return (false, "Scenario not found");

            var sceneList = await _sceneWpRepo.GetBySceneIdAsync(Guid.Parse(id));
            foreach (var item in sceneList)
            {
                scene.IsActive = false;
                scene.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(scene);
            }

            return (true, "Deleted successfully");
        }
    }
}