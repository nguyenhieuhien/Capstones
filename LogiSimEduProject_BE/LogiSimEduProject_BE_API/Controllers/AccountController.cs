using Google.Apis.Auth;
using LogiSimEduProject_BE_API.Controllers.DTO;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        public AccountController(IConfiguration config, IAccountService accountService)
        {
            _config = config;
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountDTO>>> GetAll()
        {
            try
            {
                var accounts = await _accountService.GetAll();
                var result = accounts.Select(account => new AccountDTO
                {
                    Id = account.Id,
                    UserName = account.UserName,
                    Password = account.Password,
                    FullName = account.FullName,
                    Email = account.Email,
                    Phone = account.Phone,
                    RoleId = account.RoleId,
                    IsActive = account.IsActive,
                }).ToList();

                return Ok(result); // <- You were missing this line
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var account = _accountService.Authenticate(request.Email, request.Password);

            if (account == null || account.Result == null)
                return Unauthorized();

            var token = GenerateJSONWebToken(account.Result);

            return Ok(token);
        }

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);

                if (payload == null)
                    return Unauthorized("Invalid Google token");

                // Tìm hoặc tạo tài khoản
                var account = await _accountService.GetOrCreateGoogleAccountAsync(payload.Email, payload.Name);

                if (account == null)
                    return Unauthorized("Unable to create or retrieve user");

                // Sinh JWT như login thường
                var token = GenerateJSONWebToken(account);

                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GenerateJSONWebToken(Account accountInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"]
                    , _config["Jwt:Audience"]
                    , new Claim[]
                    {
                    new(ClaimTypes.Name, accountInfo.UserName),
                    //new(ClaimTypes.Email, userInfo.Email),
                    new(ClaimTypes.Role, accountInfo.RoleId.ToString()),
                    },
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public sealed record LoginRequest(string Email, string Password);
        public sealed record GoogleLoginRequest(string IdToken);
    }
}
