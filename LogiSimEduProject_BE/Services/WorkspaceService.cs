using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IWorkspaceService
    {
        Task<List<Workspace>> GetAll();
        Task<Workspace> GetById(string id);
        Task<int> Create(Workspace workspace);
        Task<int> Update(Workspace workspace);
        Task<bool> Delete(string id);
    }

    public class WorkspaceService : IWorkspaceService
    {
        private WorkspaceRepository _repository;

        public WorkspaceService()
        {
            _repository = new WorkspaceRepository();
        }
        public async Task<int> Create(Workspace workspace)
        {
            return await _repository.CreateAsync(workspace);
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

        public async Task<List<Workspace>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Workspace> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Workspace workspace)
        {
            return await _repository.UpdateAsync(workspace);
        }

    }
}
