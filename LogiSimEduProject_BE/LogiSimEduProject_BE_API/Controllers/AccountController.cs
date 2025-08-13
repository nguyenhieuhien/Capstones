using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using LogiSimEduProject_BE_API.Controllers.Request.AccountRequest;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using Services.IServices;
using Services.DTO.Account;
using Microsoft.AspNetCore.Identity;

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
        //[Authorize(Roles = "Admin")]
        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all accounts")]
        public async Task<IEnumerable<Account>> Get() => await _accountService.GetAll();

        //[Authorize(Roles = "Admin,Organization_Admin,Instructor,Student")]
        [HttpGet("get_account/{id}")]
        [SwaggerOperation(Summary = "Get account by ID")]
        public async Task<Account> Get(string id) => await _accountService.GetById(id);

        [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpGet("get_all_by_org/{orgId}")]
        [SwaggerOperation(Summary = "Get all accounts by organization ID", Description = "Retrieve all accounts that belong to a specific organization.")]
        public async Task<IActionResult> GetAllByOrgId(Guid orgId)
        {
            var accounts = await _accountService.GetAllByOrgId(orgId);
            return Ok(accounts);
        }


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
                user = new { account.Id, account.UserName, account.FullName, account.Email,account.RoleId, account.OrganizationId }
            });
        }


        [HttpPost("register-admin-account")]
        [SwaggerOperation(Summary = "Register new admin account", Description = "Create a new admin account and send OTP for email verification")]
        public async Task<IActionResult> RegisterAdminAccount([FromBody] AccountDTOCreateAd request)
        {
            var (success, message) = await _accountService.RegisterAdminAccountAsync(request);
            return success ? Ok(message) : BadRequest(message);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("register-organization-admin-account")]
        public async Task<IActionResult> RegisterOrganizationAdmin([FromBody] AccountDTOCreateOg request)
        {
            var (success, message) = await _accountService.RegisterOrganizationAdminAccountAsync(request);
            return success ? Ok(message) : BadRequest(message);
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpPost("register-instructor-account")]
        public async Task<IActionResult> RegisterInstructor([FromBody] AccountDTOCreate request)
        {
            var (success, message) = await _accountService.RegisterInstructorAccountAsync(request);
            return success ? Ok(message) : BadRequest(message);
        }


        [Authorize(Roles = "Organization_Admin")]
        [HttpPost("register-student-account")]
        public async Task<IActionResult> RegisterStudent([FromBody] AccountDTOCreate request)
        {
            var (success, message) = await _accountService.RegisterStudentAccountAsync(request);
            return success ? Ok(message) : BadRequest(message);
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

        [HttpPost("reset_password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            var (success, message) = await _accountService.ResetPasswordAsync(model.Token, model.NewPassword);
            return success ? Ok(message) : BadRequest(message);
        }

        //[Authorize]
        [HttpPost("change_password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var (success, message) = await _accountService.ChangePasswordAsync(email, request.CurrentPassword, request.NewPassword);
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        //[Authorize]
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

            if (request.FullName != null)
                existing.FullName = request.FullName;

            if (request.Phone != null)
                existing.Phone = request.Phone;

            if (request.Address != null)
                existing.Address = request.Address;

            if (request.AvtUrl != null)
                existing.AvtUrl = request.AvtUrl;

            if (request.Gender != null)
                existing.Gender = request.Gender;

            existing.UpdatedAt = DateTime.UtcNow;

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

        [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpPut("ban_account/{id}")]
        public async Task<IActionResult> BanAccount(string id)
        {
            var success = await _accountService.BanAccount(id);
            return success ? Ok("Tài khoản đã bị khóa.") : BadRequest("Không thể khóa tài khoản.");
        }

        [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpPut("unban_account/{id}")]
        public async Task<IActionResult> UnbanAccount(string id)
        {
            var success = await _accountService.UnbanAccount(id);
            return success ? Ok("Tài khoản đã được mở khóa.") : BadRequest("Không thể mở khóa tài khoản.");
        }
    }
}
