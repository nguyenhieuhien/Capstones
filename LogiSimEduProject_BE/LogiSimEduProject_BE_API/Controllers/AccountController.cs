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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


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

         [HttpPost("VerifyEmail")]
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


        [HttpPost("ConfirmChangeEmail")]
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


        [HttpPost("Login")]
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

        [HttpPost("SignUp")]
        public async Task<IActionResult> Register(AccountDTOCreate request)
        {
            var passwordHasher = new PasswordHasher<Account>();

            var account = new Account
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                IsEmailVerify = false,
                CreatedAt = DateTime.UtcNow,
                Password = passwordHasher.HashPassword(new Account(), request.Password)
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Đăng ký thất bại");

            // Tạo mã OTP và lưu email theo mã
            var otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"verify_email_token_{otp}", account.Email, TimeSpan.FromMinutes(10));

            await _emailService.SendEmailAsync(
                account.Email,
                "Xác thực email - LogiSimEdu",
                $"<p>Mã xác thực email của bạn là: <strong>{otp}</strong>. Mã này sẽ hết hạn sau 10 phút.</p>"
            );

            return Ok("Tài khoản đã được tạo. Vui lòng kiểm tra email để lấy mã xác thực.");
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


        [HttpPost("RequestChangeEmail")]
        [Authorize]
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

        [HttpPost("ResendVerify")]
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
    }
}
//[HttpGet("ConfirmChangeEmail")]
//public async Task<IActionResult> ConfirmChangeEmail([FromQuery] string token)
//{
//    if (!_cache.TryGetValue($"change_email_{token}", out dynamic? data) || data == null)
//        return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

//    string oldEmail = data.OldEmail;
//    string newEmail = data.NewEmail;

//    var user = await _accountRepository.GetByEmailAsync(oldEmail);
//    if (user == null)
//        return BadRequest("Không tìm thấy người dùng.");

//    user.Email = newEmail;
//    user.IsEmailVerify = true; // hoặc false nếu muốn bắt xác thực lại
//    await _accountService.Update(user);

//    _cache.Remove($"change_email_{token}");

//    return Ok("Email của bạn đã được cập nhật thành công.");
//}

//[HttpGet("Search")]
//public async Task<IEnumerable<Account>> Get(string username, string fullname, string email, string phone)
//{
//    return await _accountService.Search(username, fullname, email, phone);
//}

//[Authorize]
//[HttpPost("RequestChangeEmail")]
//public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequest request)
//{
//    var email = User.FindFirstValue(ClaimTypes.Email)
//        ?? User.FindFirstValue(JwtRegisteredClaimNames.Email)
//        ?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

//    if (string.IsNullOrEmpty(email))
//        return Unauthorized("Email not found in token");

//    var user = await _accountRepository.GetByEmailAsync(email);
//    if (user == null)
//        return Unauthorized("User not found");

//    var passwordHasher = new PasswordHasher<Account>();
//    var verify = passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);

//    if (verify != PasswordVerificationResult.Success)
//        return BadRequest("Current password is incorrect.");

//    // ✅ Tạo token xác nhận đổi email
//    var token = Guid.NewGuid().ToString();
//    _cache.Set($"change_email_{token}", new { OldEmail = email, NewEmail = request.NewEmail }, TimeSpan.FromHours(1));

//    var confirmLink = $"https://yourfrontend.com/confirm-change-email?token={token}";

//    await _emailService.SendEmailAsync(
//        request.NewEmail,
//        "Xác nhận đổi email - LogiSimEdu",
//        $"<p>Nhấn vào liên kết sau để xác nhận đổi email:</p><a href='{confirmLink}'>{confirmLink}</a>"
//    );

//    return Ok("Một liên kết xác nhận đã được gửi đến email mới của bạn. Vui lòng kiểm tra hộp thư.");
//}

//[HttpPost("RegisterAccount")]
//public async Task<IActionResult> Register(AccountDTOCreate request)
//{
//    var passwordHasher = new PasswordHasher<Account>();

//    var account = new Account
//    {
//        UserName = request.UserName,
//        FullName = request.FullName,
//        Email = request.Email,
//        Phone = request.Phone,
//        IsActive = true,
//        IsEmailVerify = false, // ban đầu là chưa kích hoạt
//        CreatedAt = DateTime.UtcNow,
//        Password = passwordHasher.HashPassword(new Account(), request.Password)
//    };

//    var result = await _accountService.Register(account);
//    if (result <= 0)
//        return BadRequest("Đăng ký thất bại");

//    // Tạo token xác thực và lưu vào cache
//    var token = Guid.NewGuid().ToString();
//    _cache.Set($"verify_{token}", account.Email, TimeSpan.FromHours(1));

//    var verifyLink = $"https://www.facebook.com/NguyenHieuHien.Profile?token={token}";

//    await _emailService.SendEmailAsync(
//        account.Email,
//        "Xác thực email - LogiSimEdu",
//        $"<p>Nhấn vào liên kết sau để xác thực email:</p><a href='{verifyLink}'>{verifyLink}</a>"
//    );

//    return Ok("Tài khoản đã được tạo. Vui lòng kiểm tra email để xác thực.");
//}
//[HttpGet("VerifyEmail")]
//public async Task<IActionResult> VerifyEmail([FromQuery] string token)
//{
//    if (!_cache.TryGetValue($"verify_{token}", out string? email) || email == null)
//        return BadRequest("Token không hợp lệ hoặc đã hết hạn.");

//    var user = await _accountRepository.GetByEmailAsync(email);
//    if (user == null)
//        return BadRequest("Không tìm thấy tài khoản.");

//    user.IsEmailVerify = true;
//    await _accountService.Update(user);

//    _cache.Remove($"verify_{token}");

//    return Ok("Tài khoản đã được xác thực thành công.");
//}