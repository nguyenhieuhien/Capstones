using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Services;
using Repositories.Models;
using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using LogiSimEduProject_BE_API.Controllers.Request.AccountRequest;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAccountService _accountService;

        public AccountController(IConfiguration config, IAccountService accountService)
        {
            _config = config;
            _accountService = accountService;
        }

        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all accounts")]
        public async Task<IEnumerable<Account>> Get() => await _accountService.GetAll();

        [HttpGet("get_account/{id}")]
        [SwaggerOperation(Summary = "Get account by ID")]
        public async Task<Account> Get(string id) => await _accountService.GetById(id);

        [HttpPost("login")]
        [SwaggerOperation(Summary = "Login Account")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var account = await _accountService.Authenticate(request.UserName, request.Password);
            if (account == null) return Unauthorized("Sai tài khoản hoặc mật khẩu");
            if (!(account.IsActive ?? false)) return Unauthorized("Tài khoản đã bị khóa.");
            if (!(account.IsEmailVerify ?? false)) return Unauthorized("Tài khoản chưa xác thực email.");

            var token = _accountService.GenerateToken(account, _config);
            return Ok(new
            {
                token,
                user = new { account.Id, account.UserName, account.Email }
            });
        }

        [HttpPost("verify_email")]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] string otp)
        {
            var (success, message) = await _accountService.VerifyEmailOtp(otp);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpPost("confirm_change_email")]
        public async Task<IActionResult> ConfirmChangeEmail([FromBody] string otp)
        {
            var (success, message) = await _accountService.ConfirmChangeEmail(otp);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpPost("forgot_password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            var (success, message) = await _accountService.SendResetPasswordEmail(model.Email);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            var (success, message) = await _accountService.ResetPasswordAsync(model.Token, model.NewPassword);
            return success ? Ok(message) : BadRequest(message);
        }

        [Authorize]
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var (success, message) = await _accountService.ChangePasswordAsync(email, request.CurrentPassword, request.NewPassword);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [Authorize]
        [HttpPost("request_change_email")]
        public async Task<IActionResult> RequestChangeEmail([FromBody] ChangeEmailRequest request)
        {
            var currentEmail = User.FindFirstValue(ClaimTypes.Email);
            var (success, message) = await _accountService.RequestChangeEmail(currentEmail, request.NewEmail, request.Password);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpPost("resend_verify")]
        public async Task<IActionResult> ResendVerifyOtp([FromBody] string email)
        {
            var (success, message) = await _accountService.ResendVerifyOtp(email);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpPut("update_account/{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] AccountDTOUpdate request)
        {
            var existing = await _accountService.GetById(id);
            if (existing == null) return NotFound("Không tìm thấy tài khoản.");

            existing.OrganizationId = request.OrganizationId;
            existing.UserName = request.UserName;
            existing.FullName = request.FullName;
            existing.Email = request.Email;
            existing.Phone = request.Phone;

            if (request.RoleId is > 0 and <= 4)
                existing.RoleId = request.RoleId;
            else
                return BadRequest("RoleId không hợp lệ.");

            await _accountService.Update(existing);
            return Ok("Cập nhật thành công.");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_account/{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            var success = await _accountService.Delete(id);
            return success ? Ok("Xóa tài khoản thành công.") : NotFound("Không tìm thấy tài khoản.");
        }

        [HttpPut("ban_account/{id}")]
        public async Task<IActionResult> BanAccount(string id)
        {
            var success = await _accountService.BanAccount(id);
            return success ? Ok("Tài khoản đã bị khóa.") : BadRequest("Không thể khóa tài khoản.");
        }

        [HttpPut("unban_account/{id}")]
        public async Task<IActionResult> UnbanAccount(string id)
        {
            var success = await _accountService.UnbanAccount(id);
            return success ? Ok("Tài khoản đã được mở khóa.") : BadRequest("Không thể mở khóa tài khoản.");
        }
    }
}
