using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISceneOfWorkSpaceService
    {
        Task<List<SceneOfWorkSpace>> GetAll();
        Task<SceneOfWorkSpace> GetById(string id);
        Task<int> Create(SceneOfWorkSpace scWs);
        Task<int> Update(string id, SceneOfWorkSpace scWs);
        Task<bool> Delete(string id);
    }

    public class SceneOfWorkSpaceService : ISceneOfWorkSpaceService
    {
        private SceneOfWorkSpaceRepository _repository;

        public SceneOfWorkSpaceService()
        {
            _repository = new SceneOfWorkSpaceRepository();
        }
        public async Task<int> Create(SceneOfWorkSpace scWs)
        { 
            var entity = new SceneOfWorkSpace
            {
                Id = Guid.NewGuid(),
                SceneId = scWs.SceneId,
                WorkSpaceId = scWs.WorkSpaceId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            return await _repository.CreateAsync(entity);
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

        public async Task<List<SceneOfWorkSpace>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<SceneOfWorkSpace> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(string id, SceneOfWorkSpace scWs)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return 0;

            entity.SceneId = scWs.SceneId;
            entity.WorkSpaceId = scWs.WorkSpaceId;
            entity.UpdatedAt = DateTime.UtcNow;

            return await _repository.UpdateAsync(scWs);
        }
    }
}
