using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LogiSimEduProject_BE_API.Controllers.DTO.Account;

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

        // GET: api/<AccountController>
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Account>> Get()
        {
            return await _accountService.GetAll();
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

        [HttpGet("{id}")]
        public async Task<Account> Get(string id)
        {
            return await _accountService.GetById(id);
        }

        //[Authorize(Roles = "1, 2")]
        [HttpGet("Search")]
        public async Task<IEnumerable<Account>> Get(string username, string fullname, string email, string phone)
        {
            return await _accountService.Search(username, fullname, email, phone);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(AccountDTOCreate request)
        {
            var account = new Account
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password, 
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _accountService.Register(account);

            if (result <= 0)
                return BadRequest("Fail Register");

            var createdAccount = await _accountService.Authenticate(account.Email, account.Password);

            if (createdAccount == null)
                return Unauthorized();

            var token = GenerateJSONWebToken(createdAccount);

            return Ok(new
            {
                token,
                user = new
                {
                    createdAccount.Id,
                    createdAccount.UserName,
                    createdAccount.Email
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string Id, AccountDTOUpdate request)
        {
            var existingAccount = await _accountService.GetById(Id);
            if (existingAccount == null)
            {
                return NotFound(new { Message = $"Account with ID {Id} was not found." });
            }
            existingAccount.UserName = request.UserName;
            existingAccount.FullName = request.FullName;
            existingAccount.Password = request.Password;
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
                    Password = existingAccount.Password,
                    Email = existingAccount.Email,
                    Phone = existingAccount.Phone,
                    IsActive = existingAccount.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _accountService.Delete(id);
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

        //[HttpPost("GoogleLogin")]
        //public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        //{
        //    try
        //    {
        //        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);

        //        if (payload == null)
        //            return Unauthorized("Invalid Google token");

        //        // Tìm hoặc tạo tài khoản
        //        var account = await _accountService.GetOrCreateGoogleAccountAsync(payload.Email, payload.Name);

        //        if (account == null)
        //            return Unauthorized("Unable to create or retrieve user");

        //        // Sinh JWT như login thường
        //        var token = GenerateJSONWebToken(account);

        //        return Ok(token);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { error = ex.Message });
        //    }
        //}

        public sealed record LoginRequest(string Email, string Password);

        public sealed record RegisterRequest(string UserName, string FullName, string Email, string Password, string Phone);

    }
}
