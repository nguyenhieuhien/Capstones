using Repositories;
using Repositories.Models;
using System.Threading.Tasks; // Task<>
using Microsoft.AspNetCore.Identity;
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
        Task<(bool Success, string Message)> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword);

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

        public async Task<Account?> Authenticate(string email, string password)
        {
            var account = await _repository.GetAccountByEmail(email);
            if (account == null)
            {
                Console.WriteLine($"[Auth] Không tìm thấy tài khoản với email: {email}");
                return null;
            }

            var hasher = new PasswordHasher<Account>();
            try
            {
                var result = hasher.VerifyHashedPassword(account, account.Password, password);
                Console.WriteLine($"[Auth] So sánh kết quả: {result}");

                return result == PasswordVerificationResult.Success ? account : null;
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"[Auth] Lỗi Format mật khẩu: {ex.Message}");
                return null;
            }
        }


        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string newPassword)
        {
            var account = await _repository.GetAccountByEmail(email);
            if (account == null)
                return (false, "Tài khoản không tồn tại.");

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, newPassword);

            await _repository.UpdateAsync(account);
            return (true, "Đặt lại mật khẩu thành công.");
        }


        public async Task<(bool Success, string Message)> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var account = await _repository.GetAccountByEmail(email);
            if (account == null)
                return (false, "Account not found.");

            var hasher = new PasswordHasher<Account>();

            // Kiểm tra mật khẩu hiện tại
            var verify = hasher.VerifyHashedPassword(account, account.Password, currentPassword);
            if (verify != PasswordVerificationResult.Success)
                return (false, "Current password is incorrect.");

            // Kiểm tra mật khẩu mới có trùng không
            var samePassword = hasher.VerifyHashedPassword(account, account.Password, newPassword);
            if (samePassword == PasswordVerificationResult.Success)
                return (false, "New password must not be the same as the old one.");

            // Cập nhật mật khẩu
            account.Password = hasher.HashPassword(account, newPassword);
            account.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(account);
            return (true, "Password changed successfully.");
        }



        public async Task<List<Account>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<int> Register(Account account)
        {

            var studentRole = await _repository.GetRoleByNameAsync("Student");
            if (studentRole == null)
                throw new Exception("Role 'Student' does not exist");
            account.Id = Guid.NewGuid();
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
