using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAccountOfClassService
    {
        Task<List<AccountOfClass>> GetAll();
        Task<AccountOfClass> GetById(string id);
        Task<int> Create(AccountOfClass acCl);
        Task<int> Update(AccountOfClass acCl);
        Task<bool> Delete(string id);
    }

    public class AccountOfClassService : IAccountOfClassService
    {
        private AccountOfClassRepository _repository;

        public AccountOfClassService()
        {
            _repository = new AccountOfClassRepository();
        }
        public async Task<int> Create(AccountOfClass acCl)
        {
            return await _repository.CreateAsync(acCl);
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

        public async Task<List<AccountOfClass>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<AccountOfClass> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(AccountOfClass acCl)
        {
            return await _repository.UpdateAsync(acCl);
        }
    }
}
