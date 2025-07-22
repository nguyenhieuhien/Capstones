// File: Services/IAccountService.cs
using Repositories;
using Repositories.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Services.IServices;

namespace Services
{
    public class AccountService : IAccountService
    {
        private readonly AccountRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;

        public AccountService(AccountRepository repository, IMemoryCache cache, EmailService emailService)
        {
            _repository = repository;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<Account> Authenticate(string username, string password)
        {
            var account = await _repository.GetAccountByUserName(username);
            if (account == null) return null;

            var hasher = new PasswordHasher<Account>();
            var result = hasher.VerifyHashedPassword(account, account.Password, password);
            return result == PasswordVerificationResult.Success ? account : null;
        }

        private async Task SendEmailVerificationOTPAsync(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"verify_email_token_{otp}", email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(
                email,
                "Xác thực email - LogiSimEdu",
                $"<p>Mã xác thực email của bạn là: <strong>{otp}</strong>. Mã này sẽ hết hạn sau 10 phút.</p>"
            );
        }

        public async Task<(bool Success, string Message)> RegisterAdminAccountAsync(Account request)
        {
            var passwordHasher = new PasswordHasher<Account>();

            var existingAccount = await _repository.GetByEmailAsync(request.Email);
            if (existingAccount != null)
                return (false, "This email is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OrganizationId = request.OrganizationId,
                RoleId = 1,
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                IsEmailVerify = false,
                CreatedAt = DateTime.UtcNow,
                Password = passwordHasher.HashPassword(new Account(), request.Password)
            };

            var result = await _repository.CreateAsync(account);
            if (result <= 0)
                return (false, "Đăng ký thất bại");

            // Gọi hàm gửi OTP xác thực email
            await SendEmailVerificationOTPAsync(account.Email);

            return (true, "Tài khoản đã được tạo. Vui lòng kiểm tra email để lấy mã xác thực.");
        }

        public async Task<(bool Success, string Message)> RegisterOrganizationAdminAccountAsync(Account request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var existingAccount = await _repository.GetByEmailAsync(request.Email);
            if (existingAccount != null)
                return (false, "This email is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                OrganizationId = request.OrganizationId,
                RoleId = 2, // Organization_Admin
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.CreateAsync(account);
            if (result <= 0)
                return (false, "Tạo tài khoản quản trị tổ chức thất bại");

            // Gửi email chứa thông tin đăng nhập
            var emailBody = $@"
        <p>Chào {account.FullName},</p>
        <p>Bạn được chỉ định là quản trị viên tổ chức trên hệ thống LogiSimEdu.</p>
        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
        <p><strong>Mật khẩu:</strong> {rawPassword}</p>
        <p>Vui lòng đăng nhập và đổi mật khẩu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản quản trị tổ chức - LogiSimEdu", emailBody);

            return (true, "Tài khoản Organization_Admin đã được tạo và gửi email thành công.");
        }

        public async Task<(bool Success, string Message)> RegisterInstructorAccountAsync(Account request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var existingAccount = await _repository.GetByEmailAsync(request.Email);
            if (existingAccount != null)
                return (false, "This email is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                RoleId = 3,             // Instructor
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.CreateAsync(account);
            if (result <= 0)
                return (false, "Tạo tài khoản Instructor thất bại");

            var emailBody = $@"
        <p>Chào {account.FullName},</p>
        <p>Bạn đã được thêm vào tổ chức với vai trò <strong>Instructor</strong> trên hệ thống LogiSimEdu.</p>
        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
        <p><strong>Mật khẩu:</strong> {rawPassword}</p>
        <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản Giảng viên - LogiSimEdu", emailBody);

            return (true, "Tài khoản Instructor đã được tạo và gửi email thành công.");
        }

        public async Task<(bool Success, string Message)> RegisterStudentAccountAsync(Account request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var existingAccount = await _repository.GetByEmailAsync(request.Email);
            if (existingAccount != null)
                return (false, "This email is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                OrganizationId = request.OrganizationId,
                RoleId = 4, // Student
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _repository.CreateAsync(account);
            if (result <= 0)
                return (false, "Tạo tài khoản Student thất bại");

            var emailBody = $@"
        <p>Chào {account.FullName},</p>
        <p>Bạn đã được thêm vào tổ chức với vai trò <strong>Student</strong> trên hệ thống LogiSimEdu.</p>
        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
        <p><strong>Mật khẩu:</strong> {rawPassword}</p>
        <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản Học viên - LogiSimEdu", emailBody);

            return (true, "Tài khoản Student đã được tạo và gửi email thành công.");
        }

        public string GenerateToken(Account account, IConfiguration config)
        {
            var key = config["Jwt:Key"];
            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            string roleName = account.RoleId switch
            {
                1 => "Admin",
                2 => "Organization_Admin",
                3 => "Instructor",
                4 => "Student",
                _ => "Student"
            };

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, account.Email),
                new Claim("id", account.Id.ToString()),
                new Claim("username", account.UserName),
                new Claim(ClaimTypes.Role, roleName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public async Task<(bool Success, string Message)> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var account = await _repository.GetAccountByUserName(email);
            if (account == null) return (false, "Account not found.");

            var hasher = new PasswordHasher<Account>();
            var verify = hasher.VerifyHashedPassword(account, account.Password, currentPassword);
            if (verify != PasswordVerificationResult.Success)
                return (false, "Current password is incorrect.");

            var samePassword = hasher.VerifyHashedPassword(account, account.Password, newPassword);
            if (samePassword == PasswordVerificationResult.Success)
                return (false, "New password must not be the same as the old one.");

            account.Password = hasher.HashPassword(account, newPassword);
            account.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(account);
            return (true, "Password changed successfully.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            if (!_cache.TryGetValue(token, out string email) || string.IsNullOrEmpty(email))
                return (false, "Invalid or expired token.");

            var account = await _repository.GetByEmailAsync(email);
            if (account == null) return (false, "Account not found.");

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, newPassword);
            await _repository.UpdateAsync(account);
            _cache.Remove(token);

            return (true, "Password reset successfully.");
        }

        public async Task<(bool Success, string Message)> SendResetPasswordEmail(string email)
        {
            var account = await _repository.GetByEmailAsync(email);
            if (account == null) return (false, "Email does not exist.");

            var token = Guid.NewGuid().ToString();
            _cache.Set(token, email, TimeSpan.FromMinutes(30));
            var link = $"https://www.facebook.com/NguyenHieuHien.Profile?token={token}";

            await _emailService.SendEmailAsync(email, "Yêu cầu đặt lại mật khẩu - LogiSimEdu",
                $"<p>Nhấn vào liên kết sau để đặt lại mật khẩu:</p><a href='{link}'>{link}</a>");

            return (true, "Reset email sent.");
        }

        public async Task<(bool Success, string Message)> RequestChangeEmail(string currentEmail, string newEmail, string password)
        {
            if (currentEmail.Equals(newEmail, StringComparison.OrdinalIgnoreCase))
                return (false, "New email must be different from current email.");

            if (await _repository.GetByEmailAsync(newEmail) != null)
                return (false, "Email already in use.");

            var user = await _repository.GetByEmailAsync(currentEmail);
            if (user == null) return (false, "User not found.");

            var result = new PasswordHasher<Account>().VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Success)
                return (false, "Current password is incorrect.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"change_email_token_{otp}", new { OldEmail = currentEmail, NewEmail = newEmail }, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(newEmail, "Mã xác nhận đổi email - LogiSimEdu",
                $"<p>Mã xác thực để đổi email là: <strong>{otp}</strong>. Mã này sẽ hết hạn sau 10 phút.</p>");

            return (true, "OTP sent to new email.");
        }

        public async Task<(bool Success, string Message)> ConfirmChangeEmail(string otp)
        {
            if (!_cache.TryGetValue($"change_email_token_{otp}", out dynamic data) || data == null)
                return (false, "Invalid or expired OTP.");

            var user = await _repository.GetByEmailAsync((string)data.OldEmail);
            if (user == null) return (false, "User not found.");

            user.Email = data.NewEmail;
            user.IsEmailVerify = true;
            await _repository.UpdateAsync(user);
            _cache.Remove($"change_email_token_{otp}");

            return (true, "Email updated successfully.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailOtp(string otp)
        {
            if (!_cache.TryGetValue($"verify_email_token_{otp}", out string email) || string.IsNullOrEmpty(email))
                return (false, "Invalid or expired OTP.");

            var user = await _repository.GetByEmailAsync(email);
            if (user == null) return (false, "User not found.");

            user.IsEmailVerify = true;
            await _repository.UpdateAsync(user);
            _cache.Remove($"verify_email_token_{otp}");

            return (true, "Email verified successfully.");
        }

        public async Task<(bool Success, string Message)> ResendVerifyOtp(string email)
        {
            var user = await _repository.GetByEmailAsync(email);
            if (user == null || user.IsEmailVerify == true)
                return (false, "Invalid or already verified account.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"verify_email_token_{otp}", email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(email, "Gửi lại mã xác thực - LogiSimEdu",
                $"<p>Mã xác thực mới của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong 10 phút.</p>");

            return (true, "OTP resent successfully.");
        }

        public async Task<List<Account>> GetAll() => await _repository.GetAll();
        public async Task<Account> GetById(string id) => await _repository.GetById(id);
        public async Task<int> Register(Account account) { account.Id = Guid.NewGuid(); return await _repository.CreateAsync(account); }
        public async Task<int> Update(Account account) => await _repository.UpdateAsync(account);
        public async Task<bool> Delete(string id) { var acc = await _repository.GetById(id); return await _repository.RemoveAsync(acc); }
        public async Task<List<Account>> Search(string username, string fullname, string email, string phone) => await _repository.Search(username, fullname, email, phone);

        public async Task<bool> BanAccount(string id)
        {
            var account = await _repository.GetById(id);
            if (account == null || account.IsActive == false) return false;
            account.IsActive = false;
            await _repository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> UnbanAccount(string id)
        {
            var account = await _repository.GetById(id);
            if (account == null || account.IsActive == true) return false;
            account.IsActive = true;
            await _repository.UpdateAsync(account);
            return true;
        }


    }
}
