using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IAccountService
    {
        Task<Account> Authenticate(string email, string password);
        Task<List<Account>> GetAll();
    }

    public class AccountService : IAccountService
    {
        private readonly AccountRepository _repository;

        public AccountService()
        {
            _repository = new AccountRepository();
        }

        public async Task<Account> Authenticate(string email, string password)
        {
            return await _repository.GetUserAccount(email, password);
        }

        public async Task<List<Account>> GetAll()
        {
            return await _repository.GetAll();
        }

    }
}
