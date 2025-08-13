using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services.DTO.Account;
using Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services
{
    public class AccountService : IAccountService
    {
        private readonly AccountRepository _accountRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly IMemoryCache _cache;
        private readonly EmailService _emailService;

        public AccountService(AccountRepository accountRepository,OrganizationRepository organizationRepository, IMemoryCache cache, EmailService emailService)
        {
            _accountRepository = accountRepository;
            _organizationRepository = organizationRepository;// Assuming this is how you instantiate it
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<Account> Authenticate(string username, string password)
        {
            var account = await _accountRepository.GetAccountByUserName(username);
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

        // Admin: tạo tài khoản + gửi OTP xác minh email
        public async Task<(bool Success, string Message)> RegisterAdminAccountAsync(AccountDTOCreateAd dto)
        {
            // Validate email/username
            if (await _accountRepository.GetByEmailAsync(dto.Email) is not null)
                return (false, "This email is already in use.");
            //if (!string.IsNullOrWhiteSpace(dto.UserName) &&
            //    await _repository.GetByUserNameAsync(dto.UserName) is not null)
            //    return (false, "This username is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                RoleId = 1, // Admin
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                IsActive = true,
                IsEmailVerify = false,
                CreatedAt = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, dto.Password);

            var result = await _accountRepository.CreateAsync(account);
            if (result <= 0) return (false, "Đăng ký thất bại");

            await SendEmailVerificationOTPAsync(account.Email);

            return (true, "Tài khoản đã được tạo. Vui lòng kiểm tra email để lấy mã xác thực.");
        }

        // Organization Admin: tạo tài khoản + gửi email chứa mật khẩu (giữ theo yêu cầu)
        public async Task<(bool Success, string Message)> RegisterOrganizationAdminAccountAsync(AccountDTOCreateOg dto)
        {
            // Validate email/username
            if (await _accountRepository.GetByEmailAsync(dto.Email) is not null)
                return (false, "This email is already in use.");
            //if (!string.IsNullOrWhiteSpace(dto.UserName) &&
            //    await _repository.GetByUserNameAsync(dto.UserName) is not null)
            //    return (false, "This username is already in use.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                RoleId = 2, // Organization_Admin
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, dto.Password);

            var result = await _accountRepository.CreateAsync(account);
            if (result <= 0) return (false, "Tạo tài khoản quản trị tổ chức thất bại");

            var emailBody = $@"
<p>Chào {account.FullName},</p>
<p>Bạn được chỉ định là quản trị viên tổ chức trên hệ thống LogiSimEdu.</p>
<p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
<p><strong>Mật khẩu:</strong> {dto.Password}</p>
<p>Vui lòng đăng nhập và đổi mật khẩu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản quản trị tổ chức - LogiSimEdu", emailBody);

            return (true, "Tài khoản Organization_Admin đã được tạo và gửi email thành công.");
        }


        public async Task<(bool Success, string Message)> RegisterInstructorAccountAsync(AccountDTOCreate dto)
        {
            // 0) Check Organization
            var org = await _organizationRepository.GetByIdAsync(dto.OrganizationId);
            if (org == null)
                return (false, "Organization không tồn tại.");
            if (org.IsActive != true)
                return (false, "Organization chưa được kích hoạt, vui lòng thanh toán.");

            // 1) Validate email/username
            if (await _accountRepository.GetByEmailAsync(dto.Email) is not null)
                return (false, "This email is already in use.");
            //if (!string.IsNullOrWhiteSpace(dto.UserName) &&
            //    await _repository.GetByUserNameAsync(dto.UserName) is not null)
            //    return (false, "This username is already in use.");

            // 2) Tạo account
            var account = new Account
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                RoleId = 3, // Instructor
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, dto.Password);

            var result = await _accountRepository.CreateAsync(account);
            if (result <= 0)
                return (false, "Tạo tài khoản Instructor thất bại");

            // Gửi email kèm mật khẩu (theo yêu cầu bạn giữ)
            var emailBody = $@"
<p>Chào {account.FullName},</p>
<p>Bạn đã được thêm vào tổ chức với vai trò <strong>Instructor</strong> trên hệ thống LogiSimEdu.</p>
<p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
<p><strong>Mật khẩu:</strong> {dto.Password}</p>
<p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản Giảng viên - LogiSimEdu", emailBody);

            return (true, "Tài khoản Instructor đã được tạo và gửi email thành công.");
        }



        public async Task<(bool Success, string Message)> RegisterStudentAccountAsync(AccountDTOCreate dto)
        {
            // 0) Check Organization
            var org = await _organizationRepository.GetByIdAsync(dto.OrganizationId);
            if (org == null)
                return (false, "Organization không tồn tại.");
            if (org.IsActive != true)
                return (false, "Organization chưa được kích hoạt, vui lòng thanh toán.");

            // 1) Validate email/username
            if (await _accountRepository.GetByEmailAsync(dto.Email) is not null)
                return (false, "This email is already in use.");
            //if (!string.IsNullOrWhiteSpace(dto.UserName) &&
            //    await _repository.GetByUserNameAsync(dto.UserName) is not null)
            //    return (false, "This username is already in use.");

            // 2) Tạo account
            var account = new Account
            {
                Id = Guid.NewGuid(),
                OrganizationId = dto.OrganizationId,
                FullName = dto.FullName,
                UserName = dto.UserName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                RoleId = 4, // Student
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, dto.Password);

            var result = await _accountRepository.CreateAsync(account);
            if (result <= 0)
                return (false, "Tạo tài khoản Student thất bại");

            // 3) Gửi email kèm mật khẩu
            var emailBody = $@"
<p>Chào {account.FullName},</p>
<p>Bạn đã được thêm vào tổ chức với vai trò <strong>Student</strong> trên hệ thống LogiSimEdu.</p>
<p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
<p><strong>Mật khẩu:</strong> {dto.Password}</p>
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
            var account = await _accountRepository.GetAccountByUserName(email);
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

            await _accountRepository.UpdateAsync(account);
            return (true, "Password changed successfully.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string token, string newPassword)
        {
            if (!_cache.TryGetValue(token, out string email) || string.IsNullOrEmpty(email))
                return (false, "Invalid or expired token.");

            var account = await _accountRepository.GetByEmailAsync(email);
            if (account == null) return (false, "Account not found.");

            var hasher = new PasswordHasher<Account>();
            account.Password = hasher.HashPassword(account, newPassword);
            await _accountRepository.UpdateAsync(account);
            _cache.Remove(token);

            return (true, "Password reset successfully.");
        }

        public async Task<(bool Success, string Message)> SendResetPasswordEmail(string email)
        {
            var account = await _accountRepository.GetByEmailAsync(email);
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

            if (await _accountRepository.GetByEmailAsync(newEmail) != null)
                return (false, "Email already in use.");

            var user = await _accountRepository.GetByEmailAsync(currentEmail);
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
                
            var user = await _accountRepository.GetByEmailAsync((string)data.OldEmail);
            if (user == null) return (false, "User not found.");

            user.Email = data.NewEmail;
            user.IsEmailVerify = true;
            await _accountRepository.UpdateAsync(user);
            _cache.Remove($"change_email_token_{otp}");

            return (true, "Email updated successfully.");
        }

        public async Task<(bool Success, string Message)> VerifyEmailOtp(string otp)
        {
            if (!_cache.TryGetValue($"verify_email_token_{otp}", out string email) || string.IsNullOrEmpty(email))
                return (false, "Invalid or expired OTP.");

            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null) return (false, "User not found.");

            user.IsEmailVerify = true;
            await _accountRepository.UpdateAsync(user);
            _cache.Remove($"verify_email_token_{otp}");

            return (true, "Email verified successfully.");
        }

        public async Task<(bool Success, string Message)> ResendVerifyOtp(string email)
        {
            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null || user.IsEmailVerify == true)
                return (false, "Invalid or already verified account.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"verify_email_token_{otp}", email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(email, "Gửi lại mã xác thực - LogiSimEdu",
                $"<p>Mã xác thực mới của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong 10 phút.</p>");

            return (true, "OTP resent successfully.");
        }

        public async Task<List<Account>> GetAll() => await _accountRepository.GetAll();
        public async Task<Account> GetById(string id) => await _accountRepository.GetById(id);

        public async Task<List<Account>> GetAllByOrgId(Guid orgId)
        {
            return await _accountRepository.GetAllByOrgId(orgId);
        }

        public async Task<int> Register(Account account) { account.Id = Guid.NewGuid(); return await _accountRepository.CreateAsync(account); }
        public async Task<int> Update(Account account) => await _accountRepository.UpdateAsync(account);
        public async Task<bool> Delete(string id) { var acc = await _accountRepository.GetById(id); return await _accountRepository.RemoveAsync(acc); }
        public async Task<List<Account>> Search(string username, string fullname, string email, string phone) => await _accountRepository.Search(username, fullname, email, phone);

        public async Task<bool> BanAccount(string id)
        {
            var account = await _accountRepository.GetById(id);
            if (account == null || account.IsActive == false) return false;
            account.IsActive = false;
            await _accountRepository.UpdateAsync(account);
            return true;
        }

        public async Task<bool> UnbanAccount(string id)
        {
            var account = await _accountRepository.GetById(id);
            if (account == null || account.IsActive == true) return false;
            account.IsActive = true;
            await _accountRepository.UpdateAsync(account);
            return true;
        }


    }
}
