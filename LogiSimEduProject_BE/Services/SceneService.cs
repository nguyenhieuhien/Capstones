using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISceneService
    {
        Task<List<Scene>> GetAll();
        Task<Scene> GetById(string id);
        Task<int> Create(Scene scene);
        Task<int> Update(Scene scene);
        Task<bool> Delete(string id);
    }

    public class SceneService : ISceneService
    {
        private SceneRepository _repository;

        public SceneService()
        {
            _repository = new SceneRepository();
        }
        public async Task<int> Create(Scene scene)
        {
            return await _repository.CreateAsync(scene);
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

        public async Task<List<Scene>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Scene> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Scene scene)
        {
            return await _repository.UpdateAsync(scene);
        }

    }
}
