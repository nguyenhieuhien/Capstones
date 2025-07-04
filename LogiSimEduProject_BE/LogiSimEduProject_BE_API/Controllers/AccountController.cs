using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Models;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
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

        // GET: api/<AccountController>
        [HttpGet("GetAllAccount")]
        public async Task<IEnumerable<Account>> Get()
        {
            return await _accountService.GetAll();
        }

        [HttpGet("GetAccount/{id}")]
        public async Task<Account> Get(string id)
        {
            return await _accountService.GetById(id);
       }

        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (!_cache.TryGetValue($"verify_{token}", out string? email) || email == null)
                return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

            var user = await _accountRepository.GetByEmailAsync(email);
            if (user == null)
                return BadRequest("Không tìm thấy tài khoản.");

            user.IsActive = true;
            await _accountService.Update(user);

            _cache.Remove($"verify_{token}");

            return Ok("Tài khoản đã được xác thực thành công.");
        }


        //[HttpGet("Search")]
        //public async Task<IEnumerable<Account>> Get(string username, string fullname, string email, string phone)
        //{
        //    return await _accountService.Search(username, fullname, email, phone);
        //}

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Dữ liệu đăng nhập không hợp lệ");

            var account = await _accountService.Authenticate(request.Email, request.Password);

            if (account == null)
                return Unauthorized("Email hoặc mật khẩu không hợp lệ");

            // ✅ Thêm dòng này: Kiểm tra xác thực email
            if (!(account.IsActive ?? false))
                return Unauthorized("Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác thực.");

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
        [HttpPost("RegisterAccount")]
        public async Task<IActionResult> Register(AccountDTOCreate request)
        {
            var passwordHasher = new PasswordHasher<Account>();

            var account = new Account
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = false, // ban đầu là chưa kích hoạt
                CreatedAt = DateTime.UtcNow,
                Password = passwordHasher.HashPassword(new Account(), request.Password)
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Đăng ký thất bại");

            // Tạo token xác thực và lưu vào cache
            var token = Guid.NewGuid().ToString();
            _cache.Set($"verify_{token}", account.Email, TimeSpan.FromHours(1));

            var verifyLink = $"https://www.facebook.com/NguyenHieuHien.Profile?token={token}";

            await _emailService.SendEmailAsync(
                account.Email,
                "Xác thực email - LogiSimEdu",
                $"<p>Nhấn vào liên kết sau để xác thực email:</p><a href='{verifyLink}'>{verifyLink}</a>"
            );

            return Ok("Tài khoản đã được tạo. Vui lòng kiểm tra email để xác thực.");
        }
        

        [HttpPost("ForgotPassword")]
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


        [HttpPost("ResetPassword")]
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

        [Authorize]
        [HttpPost("ChangePassword")]
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


        [HttpPut("UpdateAccount/{id}")]
        public async Task<ActionResult> Put(string Id, AccountDTOUpdate request)
        {
            var existingAccount = await _accountService.GetById(Id);
            if (existingAccount == null)
            {
                return NotFound(new { Message = $"Account with ID {Id} was not found." });
            }
            existingAccount.UserName = request.UserName;
            existingAccount.FullName = request.FullName;
            //existingAccount.Password = request.Password;
            existingAccount.Email = request.Email;
            existingAccount.Phone = request.Phone;

            await _accountService.Update(existingAccount);

            return Ok(new
            {
                Message = "Account updated successfully.",
                Data = new
                {
                    Id = existingAccount.Id,
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


        //[Authorize(Roles = "1")]
        [HttpDelete("DeleteAccount/{id}")]
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
            var roleName = account.Role?.RoleName ?? "Student";
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

        public class ForgotPasswordRequest
        {
            public required string Email { get; set; }
        }

        public class ResetPasswordRequest
        {
            public required string Token { get; set; }
            public required string NewPassword { get; set; }
        }

        public sealed record LoginRequest(string Email, string Password);

        public sealed record RegisterRequest(string UserName, string FullName, string Email, string Password, string Phone);
    }
}
