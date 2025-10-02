using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services.DTO.Account;
using Services.IServices;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Drawing;


using OfficeOpenXml;
using QuestPDF.Infrastructure;
using OfficeOpenXml.Style;
using Microsoft.EntityFrameworkCore;

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
            if (result != PasswordVerificationResult.Success) return null;

            // Nếu login thành công nhưng email chưa verify
            if (account.IsEmailVerify == false)
            {
                await SendEmailVerificationOTPAsync(account.Email);
            }

            return account;
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
                //Phone = dto.Phone,
                //Address = dto.Address,
                //Gender = dto.Gender,
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
                //Phone = dto.Phone,
                //Address = dto.Address,
                //Gender = dto.Gender,
                IsActive = true,
                IsEmailVerify = false,
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

            return (true, "Organization_Admin account has been created and email sent successfully.");
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
                //Phone = dto.Phone,
                //Address = dto.Address,
                //Gender = dto.Gender,
                RoleId = 3, // Instructor
                IsActive = true,
                IsEmailVerify = false,
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
                //Phone = dto.Phone,
                //Address = dto.Address,
                //Gender = dto.Gender,
                RoleId = 4, // Student
                IsActive = true,
                IsEmailVerify = false,
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

        public async Task<(int SuccessCount, List<string> Successes, List<string> Errors)> ImportInstructorAccountsAsync(IFormFile excelFile, Guid organizationId)
        {
            return await ImportAccountsFromExcelAsync(excelFile, 3, "Instructor", organizationId);
        }

        public async Task<(int SuccessCount, List<string> Successes, List<string> Errors)> ImportStudentAccountsAsync(IFormFile excelFile, Guid organizationId)
        {
            return await ImportAccountsFromExcelAsync(excelFile, 4, "Student", organizationId);
        }

        private async Task<(int SuccessCount, List<string> Successes, List<string> Errors)> ImportAccountsFromExcelAsync(IFormFile excelFile, int roleId, string roleName, Guid organizationId)
        {

            var errors = new List<string>();
            var successes = new List<string>();
            int successCount = 0;

            using var stream = new MemoryStream();
            await excelFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets.First();
            int rowCount = worksheet.Dimension.Rows;

            var org = await _organizationRepository.GetByIdAsync(organizationId);
            if (org == null)
                return (0, new List<string>(), new List<string> { "Organization does not exist." });

            if (org.IsActive != true)
                return (0, new List<string>(), new List<string> { "Organization is not activated yet, please checkout." });

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var fullName = worksheet.Cells[row, 1].Text;
                    var userName = worksheet.Cells[row, 2].Text;
                    var email = worksheet.Cells[row, 3].Text;
                    var password = worksheet.Cells[row, 4].Text;

                    if (await _accountRepository.GetByEmailAsync(email) is not null)
                    {
                        errors.Add($"Row {row}: Email {email} has existed");
                        continue;
                    }

                    var account = new Account
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = organizationId,
                        FullName = fullName,
                        UserName = userName,
                        Email = email,
                        RoleId = roleId,
                        IsActive = true,
                        IsEmailVerify = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    var hasher = new PasswordHasher<Account>();
                    account.Password = hasher.HashPassword(account, password);

                    var result = await _accountRepository.CreateAsync(account);
                    if (result > 0)
                    {
                        successCount++;
                        successes.Add($"Import thành công cho email {email}");

                        var emailBody = $@"
                        <p>Chào {account.FullName},</p>
                        <p>Bạn đã được thêm vào tổ chức với vai trò <strong>{roleName}</strong> trên hệ thống LogiSimEdu.</p>
                        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
                        <p><strong>Mật khẩu:</strong> {password}</p>
                        <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

                        await _emailService.SendEmailAsync(account.Email, $"Tài khoản {roleName} - LogiSimEdu", emailBody);
                    }
                    else
                    {
                        errors.Add($"Row {row}: Lỗi khi lưu account");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            return (successCount, successes, errors);
        }

        public async Task<byte[]> ExportStudentsToExcelAsync(Guid organizationId)
        {
            var students = await _accountRepository.GetAccountsByRoleAsync(organizationId, "Student");
            return GenerateExcelFile(students, "Students");
        }

        public async Task<byte[]> ExportInstructorsToExcelAsync(Guid organizationId)
        {
            var instructors = await _accountRepository.GetAccountsByRoleAsync(organizationId, "Instructor");
            return GenerateExcelFile(instructors, "Instructors");
        }

        private byte[] GenerateExcelFile(List<Account> accounts, string sheetName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Header
            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Role";
            worksheet.Cells[1, 5].Value = "Phone";
            worksheet.Cells[1, 6].Value = "Gender";
            worksheet.Cells[1, 7].Value = "Address";

            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data
            for (int i = 0; i < accounts.Count; i++)
            {
                var acc = accounts[i];
                worksheet.Cells[i + 2, 1].Value = acc.Id.ToString();
                worksheet.Cells[i + 2, 2].Value = acc.FullName;
                worksheet.Cells[i + 2, 3].Value = acc.Email;
                worksheet.Cells[i + 2, 4].Value = acc.Role?.Name;
                worksheet.Cells[i + 2, 5].Value = acc.Phone;
                worksheet.Cells[i + 2, 6].Value = acc.GenderNavigation?.Name ?? "";
                worksheet.Cells[i + 2, 7].Value = acc.Address;
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
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
                expires: DateTime.UtcNow.AddMonths(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        public async Task<(bool Success, string Message)> ChangePasswordAsync(string email, string currentPassword, string newPassword)
        {
            var account = await _accountRepository.GetByEmailAsync(email);
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
            var link = $"https://capstone-flexsim-fe.vercel.app/reset-password?token={token}";

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
