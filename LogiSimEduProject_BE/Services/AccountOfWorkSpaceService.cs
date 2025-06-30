using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAccountOfWorkSpaceService
    {
        Task<List<AccountOfWorkSpace>> GetAll();
        Task<AccountOfWorkSpace> GetById(string id);
        Task<int> Create(AccountOfWorkSpace accWs);
        Task<int> Update(AccountOfWorkSpace accWs);
        Task<bool> Delete(string id);
    }
    public class AccountOfWorkSpaceService : IAccountOfWorkSpaceService
    {
        private AccountOfWorkSpaceRepository _repository;

        public AccountOfWorkSpaceService()
        {
            _repository = new AccountOfWorkSpaceRepository();
        }
        public async Task<int> Create(AccountOfWorkSpace accWs)
        {
            accWs.Id = Guid.NewGuid();
            return await _repository.CreateAsync(accWs);
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

        public async Task<List<AccountOfWorkSpace>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<AccountOfWorkSpace> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(AccountOfWorkSpace accWs)
        {
            return await _repository.UpdateAsync(accWs);
        }
    }
}
