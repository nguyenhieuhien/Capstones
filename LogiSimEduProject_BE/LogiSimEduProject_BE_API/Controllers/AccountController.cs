using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using LogiSimEduProject_BE_API.Controllers.Request.AccountRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountService _accountService;
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly AccountRepository _accountRepository;
        public AccountController(IConfiguration config, IAccountService accountService, EmailService emailService, IMemoryCache memoryCache, AccountRepository accountRepository)
        {
            _config = config;
            _accountService = accountService;
            _emailService = emailService;
            _cache = memoryCache;
            _accountRepository = accountRepository;
        }


        //[Authorize(Roles = "Admin,Organization_Admin")]
        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all accounts", Description = "Retrieve a list of all registered accounts")]
        public async Task<IEnumerable<Account>> Get()
        {
            return await _accountService.GetAll();
        }



        //[Authorize(Roles = "Admin,Organization_Admin")]
        [HttpGet("get_account/{id}")]
        [SwaggerOperation(Summary = "Get account by ID", Description = "Retrieve detailed account information using the account ID")]
        public async Task<Account> Get(string id)
        {
            return await _accountService.GetById(id);
       }



        [HttpPost("verify_email")]
        [SwaggerOperation(Summary = "Verify email via OTP", Description = "Verify user's email address using the OTP sent via email")]

        public async Task<IActionResult> VerifyEmailOtp([FromBody] string otp)
        {
            if (!_cache.TryGetValue($"verify_email_token_{otp}", out string? email) || string.IsNullOrEmpty(email))
                return BadRequest("Mã xác thực không hợp lệ hoặc đã hết hạn.");

            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null)
                return BadRequest("Không tìm thấy tài khoản.");

            user.IsEmailVerify = true;
            await _accountService.Update(user);
            _cache.Remove($"verify_email_token_{otp}");

            return Ok("Xác thực email thành công.");
        }


        [HttpPost("confirm_change_email")]
        [SwaggerOperation(Summary = "Confirm email change via OTP", Description = "Verify and update new email using a one-time password (OTP)")]
        public async Task<IActionResult> ConfirmChangeEmailOtp([FromBody] string otp)
        {
            if (!_cache.TryGetValue($"change_email_token_{otp}", out dynamic? data) || data == null)
                return BadRequest("Mã OTP không hợp lệ hoặc đã hết hạn.");

            var user = await _accountRepository.GetByEmailAsync((string)data.OldEmail);
            if (user == null)
                return BadRequest("Không tìm thấy người dùng.");

            user.Email = data.NewEmail;
            user.IsEmailVerify = true;
            await _accountService.Update(user);
            _cache.Remove($"change_email_token_{otp}");

            return Ok("Email đã được cập nhật thành công.");
        }




        [HttpPost("login")]
        [SwaggerOperation(Summary = "Login Account", Description = "Login by username and password to generate JWT token")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid login data");

            var account = await _accountService.Authenticate(request.UserName, request.Password);

            if (account == null)
                return Unauthorized("Invalid username or password");
            if (!(account.IsActive ?? false))
                return Unauthorized("Tài khoản đã bị khóa vui lòng liên hệ admin.");
            if (!(account.IsEmailVerify ?? false))
                return Unauthorized("Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác thực.");
            if (!(account.IsActive ?? false))
                return Unauthorized("Tài khoản đã bị khóa vui lòng liên hệ admin.");


            var token = GenerateJSONWebToken(account);

            return Ok(new
            {
                token,
                user = new
                {
                    account.Id,
                    account.UserName,
                    account.Email
                }
            });
        }


        //[HttpPost("register-admin-account")]
        //[SwaggerOperation(Summary = "Register new admin account", Description = "Create a new admin account and send OTP for email verification")]

        //public async Task<IActionResult> RegisterAdminAccount(AccountDTOCreate request)
        //{

        //    var passwordHasher = new PasswordHasher<Account>();

        //    var account = new Account
        //    {
        //        OrganizationId = null,
        //        RoleId = 1,
        //        UserName = request.UserName,
        //        FullName = request.FullName,
        //        Email = request.Email,
        //        Phone = request.Phone,
        //        IsActive = true,
        //        IsEmailVerify = false,
        //        CreatedAt = DateTime.UtcNow,
        //        Password = passwordHasher.HashPassword(new Account(), request.Password)
        //    };

        //    var result = await _accountService.Register(account);
        //    if (result <= 0)
        //        return BadRequest("Đăng ký thất bại");

        //    // Tạo mã OTP và lưu email theo mã
        //    var otp = new Random().Next(100000, 999999).ToString();
        //    _cache.Set($"verify_email_token_{otp}", account.Email, TimeSpan.FromMinutes(10));

        //    await _emailService.SendEmailAsync(
        //        account.Email,
        //        "Xác thực email - LogiSimEdu",
        //        $"<p>Mã xác thực email của bạn là: <strong>{otp}</strong>. Mã này sẽ hết hạn sau 10 phút.</p>"
        //    );

        //    return Ok("Tài khoản đã được tạo. Vui lòng kiểm tra email để lấy mã xác thực.");
        //}


        //[Authorize(Roles = "Admin")]
        [HttpPost("register-organization-admin-account")]
        public async Task<IActionResult> RegisterOrganizationAdmin([FromBody] AccountDTOCreateOg request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var account = new Account
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                OrganizationId = request.OrganizationId,
                RoleId = 2,
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Tạo tài khoản quản trị tổ chức thất bại");

            var emailBody = $@"
            <p>Chào {account.FullName},</p>
            <p>Bạn được chỉ định là quản trị viên tổ chức trên hệ thống LogiSimEdu.</p>
            <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
            <p><strong>Mật khẩu:</strong> {rawPassword}</p>
            <p>Vui lòng đăng nhập và đổi mật khẩu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản quản trị tổ chức - LogiSimEdu", emailBody);

            return Ok("Tài khoản Organization_Admin đã được tạo và gửi email thành công.");
        }



        //[Authorize(Roles = "Organization_Admin")]
        [HttpPost("register-instructor-account")]
        public async Task<IActionResult> RegisterInstructor([FromBody] AccountDTOCreateOg request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var account = new Account
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                OrganizationId = request.OrganizationId,
                RoleId = 3,
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Tạo tài khoản Instructor thất bại");

            var emailBody = $@"
        <p>Chào {account.FullName},</p>
        <p>Bạn đã được thêm vào tổ chức với vai trò <strong>Instructor</strong> trên hệ thống LogiSimEdu.</p>
        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
        <p><strong>Mật khẩu:</strong> {rawPassword}</p>
        <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản Giảng viên - LogiSimEdu", emailBody);

            return Ok("Tài khoản Instructor đã được tạo và gửi email thành công.");
        }



        //[Authorize(Roles = "Organization_Admin")]
        [HttpPost("register-student-account")]
        public async Task<IActionResult> RegisterStudent([FromBody] AccountDTOCreateOg request)
        {
            var passwordHasher = new PasswordHasher<Account>();
            var rawPassword = request.Password;

            var account = new Account
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                Phone = request.Phone,
                Password = passwordHasher.HashPassword(null, rawPassword),
                OrganizationId = request.OrganizationId,
                RoleId = 4,
                IsActive = true,
                IsEmailVerify = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Tạo tài khoản Student thất bại");

            var emailBody = $@"
        <p>Chào {account.FullName},</p>
        <p>Bạn đã được thêm vào tổ chức với vai trò <strong>Student</strong> trên hệ thống LogiSimEdu.</p>
        <p><strong>Tên đăng nhập:</strong> {account.UserName}</p>
        <p><strong>Mật khẩu:</strong> {rawPassword}</p>
        <p>Vui lòng đăng nhập và đổi mật khẩu sau khi sử dụng lần đầu để đảm bảo an toàn.</p>";

            await _emailService.SendEmailAsync(account.Email, "Tài khoản Học viên - LogiSimEdu", emailBody);

            return Ok("Tài khoản Student đã được tạo và gửi email thành công.");
        }



        [HttpPost("forgot_password")]
        [SwaggerOperation(Summary = "Reset password", Description = "Reset password using token from email")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            var user = await _accountRepository.GetByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("Email không tồn tại.");

            // Tạo token và lưu vào memory
            var token = Guid.NewGuid().ToString();
            _cache.Set(token, model.Email, TimeSpan.FromMinutes(30)); // Token hợp lệ trong 30 phút

            //var resetLink = $"https://yourfrontend.com/reset-password?token={token}";
            var resetLink = $"https://www.facebook.com/NguyenHieuHien.Profile?token={token}";

            await _emailService.SendEmailAsync(
                model.Email,
                "Yêu cầu đặt lại mật khẩu - LogiSimEdu",
                $"<p>Nhấn vào liên kết sau để đặt lại mật khẩu:</p><a href='{resetLink}'>{resetLink}</a>"
            );

            return Ok("Email đặt lại mật khẩu đã được gửi.");
        }


        [HttpPost("reset-password")]
        [SwaggerOperation(Summary = "Reset password", Description = "Reset password using token from email")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            // Kiểm tra token
            if (!_cache.TryGetValue(model.Token, out object? emailObj) || emailObj is not string email)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null)
                return BadRequest("Không tìm thấy người dùng.");

            // Cập nhật mật khẩu mới
            var (success, message) = await _accountService.ResetPasswordAsync(email, model.NewPassword);


            if (!success)
                return BadRequest(new { message });

            // Xoá token sau khi dùng
            _cache.Remove(model.Token);

            return Ok("Đặt lại mật khẩu thành công.");
        }

        //[Authorize]
        [HttpPost("change_password")]
        [SwaggerOperation(Summary = "Change password", Description = "Authenticated user changes their password")]

        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            Console.WriteLine($"[Token] Email from JWT: {email}");

            if (string.IsNullOrEmpty(email))
                return Unauthorized("Email not found in token");

            var (success, message) = await _accountService.ChangePasswordAsync(email, request.CurrentPassword, request.NewPassword);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        //[Authorize]
        [HttpPost("request_change_email")]
        [SwaggerOperation(Summary = "Request email change", Description = "Send OTP to new email for verification before updating email address")]

        public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequest request)
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(JwtRegisteredClaimNames.Email);
            if (string.IsNullOrEmpty(currentEmail))
                return Unauthorized("Email not found in token");

            if (currentEmail.Equals(request.NewEmail, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Email mới phải khác email hiện tại.");

            if (await _accountRepository.GetByEmailAsync(request.NewEmail) != null)
                return BadRequest("Email mới đã được sử dụng bởi tài khoản khác.");

            var user = await _accountRepository.GetByEmailAsync(currentEmail);
            if (user == null)
                return Unauthorized("Không tìm thấy người dùng.");

            var result = new PasswordHasher<Account>().VerifyHashedPassword(user, user.Password, request.Password);
            if (result != PasswordVerificationResult.Success)
                return BadRequest("Mật khẩu hiện tại không đúng.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"change_email_token_{otp}", new { OldEmail = currentEmail, request.NewEmail }, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(
                request.NewEmail,
                "Mã xác nhận đổi email - LogiSimEdu",
                $"<p>Mã xác thực để đổi email là: <strong>{otp}</strong>. Mã này sẽ hết hạn sau 10 phút.</p>");

            return Ok("Mã xác nhận đã được gửi đến email mới của bạn.");
        }


        [HttpPost("resend_verify")]
        [SwaggerOperation(Summary = "Resend verification OTP", Description = "Resend email verification code to user's email")]

        public async Task<IActionResult> ResendVerifyOtp([FromBody] string email)
        {
            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null || user.IsEmailVerify == true)
                return BadRequest("Tài khoản không hợp lệ hoặc đã xác thực.");

            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"verify_email_token_{otp}", email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(
                email,
                "Gửi lại mã xác thực - LogiSimEdu",
                $"<p>Mã xác thực mới của bạn là: <strong>{otp}</strong>. Mã này có hiệu lực trong 10 phút.</p>");

            return Ok("Mã xác thực đã được gửi lại.");
        }


        [HttpPut("update_account/{id}")]
        [SwaggerOperation(Summary = "Update account by ID", Description = "Update existing account information using account ID")]

        public async Task<ActionResult> Put(string Id, AccountDTOUpdate request)
        {
            var existingAccount = await _accountService.GetById(Id);
            if (existingAccount == null)
            {
                return NotFound(new { Message = $"Account with ID {Id} was not found." });
            }
            existingAccount.OrganizationId = request.OrganizationId;
            existingAccount.UserName = request.UserName;
            existingAccount.FullName = request.FullName;
            //existingAccount.Password = request.Password;
            existingAccount.Email = request.Email;
            existingAccount.Phone = request.Phone;

            if (request.RoleId is > 0 and <= 4) // Giả sử có 4 role
            {
                existingAccount.RoleId = request.RoleId;
            }
            else
            {
                return BadRequest("Vai trò không hợp lệ. RoleId hợp lệ là 1 đến 4.");
            }

            await _accountService.Update(existingAccount);

            return Ok(new
            {
                Message = "Account updated successfully.",
                Data = new
                {
                    Id = existingAccount.Id,
                    OrganizationId = existingAccount.OrganizationId,
                    RoleId = existingAccount.RoleId,
                    UserName = existingAccount.UserName,
                    FullName = existingAccount.FullName,
                    //Password = existingAccount.Password,
                    Email = existingAccount.Email,
                    Phone = existingAccount.Phone,
                    IsActive = existingAccount.IsActive,
                }
            });
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_account/{id}")]
        [SwaggerOperation(Summary = "Delete account", Description = "Delete an account by its ID")]

        public async Task<bool> Delete(string id)
        {
            return await _accountService.Delete(id);
        }

        private string GenerateJSONWebToken(Account account)
        {
            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("JWT Key is not configured.");
            if (string.IsNullOrEmpty(issuer))
                throw new InvalidOperationException("JWT Issuer is not configured.");
            if (string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Audience is not configured.");

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
        // [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpPut("ban_account/{id}")]
        [SwaggerOperation(Summary = "Ban account", Description = "Disable an account by setting IsActive = false")]
        public async Task<IActionResult> BanAccount(string id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
                return NotFound("Không tìm thấy tài khoản.");

            if (account.IsActive == false)
                return BadRequest("Tài khoản đã bị khóa trước đó.");

            account.IsActive = false;
            await _accountService.Update(account);

            return Ok("Tài khoản đã bị khóa.");
        }


        // [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpPut("unban_account/{id}")]
        [SwaggerOperation(Summary = "Unban account", Description = "Enable a previously banned account by setting IsActive = true")]
        public async Task<IActionResult> UnbanAccount(string id)
        {
            var account = await _accountService.GetById(id);
            if (account == null)
                return NotFound("Không tìm thấy tài khoản.");

            if (account.IsActive == true)
                return BadRequest("Tài khoản đã đang hoạt động.");

            account.IsActive = true;
            await _accountService.Update(account);

            return Ok("Tài khoản đã được mở khóa.");
        }


    }
}
