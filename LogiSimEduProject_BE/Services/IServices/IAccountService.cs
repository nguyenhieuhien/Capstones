using Repositories.Models;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public interface IAccountService
    {
        // Auth
        Task<Account> Authenticate(string username, string password);
        string GenerateToken(Account account, IConfiguration config);

        // Password
        Task<(bool Success, string Message)> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword);
        Task<(bool Success, string Message)> SendResetPasswordEmail(string email);

        // Email Verification / Change
        Task<(bool Success, string Message)> RequestChangeEmail(string currentEmail, string newEmail, string password);
        Task<(bool Success, string Message)> ConfirmChangeEmail(string otp);
        Task<(bool Success, string Message)> VerifyEmailOtp(string otp);
        Task<(bool Success, string Message)> ResendVerifyOtp(string email);

        // CRUD
        Task<List<Account>> GetAll();
        Task<Account> GetById(string id);
        Task<int> Register(Account account);
        Task<int> Update(Account account);
        Task<bool> Delete(string id);

        // Search
        Task<List<Account>> Search(string username, string fullname, string email, string phone);

        // Ban / Unban
        Task<bool> BanAccount(string id);
        Task<bool> UnbanAccount(string id);
    }
}
