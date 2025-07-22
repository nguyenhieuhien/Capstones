using Repositories;
using Repositories.Models;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public class WorkspaceService : IWorkspaceService
    {
        private WorkspaceRepository _repository;
        private AccountOfWorkSpaceRepository _accountWp;
        public WorkspaceService()
        {
            _repository = new WorkspaceRepository();
            _accountWp = new AccountOfWorkSpaceRepository();
        }
        public async Task<int> Create(WorkSpace workspace)
        {
            workspace.Id = Guid.NewGuid();
            return await _repository.CreateAsync(workspace);
        }

        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                var workSpaceList = await _accountWp.GetByWorkspaceIdAsync(Guid.Parse(id));
                foreach (var workSpace in workSpaceList)
                {
                    await _accountWp.RemoveAsync(workSpace);
                }
                return await _repository.RemoveAsync(item);
            }

            return false;
        }

        public async Task<List<WorkSpace>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<WorkSpace> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(WorkSpace workspace)
        {
            return await _repository.UpdateAsync(workspace);
        }

    }
}
