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
        Task<Account> GetOrCreateGoogleAccountAsync(string email, string fullName);
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

        public async Task<Account> GetOrCreateGoogleAccountAsync(string email, string fullName)
        {
            var account = await _repository.GetByEmailAsync(email);

            if (account == null)
            {
                account = new Account
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    FullName = fullName,
                    UserName = email,
                    Password = "", // Không cần thiết
                    RoleId = new Guid("PUT-DEFAULT-ROLE-ID-HERE"),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _repository.InsertAsync(account);
            }

            return account;
        }

    }
}
