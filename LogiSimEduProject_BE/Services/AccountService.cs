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
        //Task<Account> GetOrCreateGoogleAccountAsync(string email, string fullName);
        Task<List<Account>> GetAll();
        Task<Account> GetById(string id);
        Task<int> Register(Account account);
        Task<int> Update(Account account);
        Task<bool> Delete(string id);
        Task<List<Account>> Search(string username, string fullname, string email, string phone);
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

        public async Task<int> Register(Account account)
        {
            var studentRole = await _repository.GetRoleByNameAsync("student");
            if (studentRole == null)
                throw new Exception("Role 'student' does not exist");

            account.RoleId = studentRole.Id;

            return await _repository.CreateAsync(account);
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

        public async Task<Account> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Account>> Search(string username, string fullname, string email, string phone)
        {
            return await _repository.Search(username, fullname, email, phone);
        }

        public async Task<int> Update(Account account)
        {
            return await _repository.UpdateAsync(account);
        }

        //public async Task<Account> GetOrCreateGoogleAccountAsync(string email, string fullName)
        //{
        //    var account = await _repository.GetByEmailAsync(email);

        //    if (account == null)
        //    {
        //        account = new Account
        //        {
        //            Id = Guid.NewGuid(),
        //            Email = email,
        //            FullName = fullName,
        //            UserName = email,
        //            Password = "", // Không cần thiết
        //            RoleId = new Guid("PUT-DEFAULT-ROLE-ID-HERE"),
        //            CreatedAt = DateTime.UtcNow,
        //            IsActive = true
        //        };

        //        await _repository.InsertAsync(account);
        //    }

        //    return account;
        //}

    }
}
