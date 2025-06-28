using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IRoleService
    {
        Task<List<Role>> GetAll();
        Task<Role> GetById(string id);
        Task<int> Create(Role role);
        Task<int> Update(Role role);
        Task<bool> Delete(string id);
    }

    public class RoleService : IRoleService
    {
        private RoleRepository _repository;

        public RoleService()
        {
            _repository = new RoleRepository();
        }
        public async Task<int> Create(Role role)
        {
            return await _repository.CreateAsync(role);
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

        public async Task<List<Role>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Role> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Role role)
        {
            return await _repository.UpdateAsync(role);
        }

    }
}
