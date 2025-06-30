using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid login data");

            var account = await _accountService.Authenticate(request.Email, request.Password);

            if (account == null)
                return Unauthorized("Invalid email or password");

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
            var passwordHasher = new PasswordHasher<Account>();

            var account = new Account
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Password = passwordHasher.HashPassword(null, request.Password)
            };

            var result = await _accountService.Register(account);
            if (result <= 0)
                return BadRequest("Fail Register");

            // Không cần Authenticate lại!
            return Ok(new
            {
                user = new
                {
                    account.Id,
                    account.UserName,
                    account.Email
                }
            });
        }



        //[Authorize(Roles = "1")]
        [HttpPut("Update/{id}")]
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



        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _accountService.Delete(id);
        }

        private string GenerateJSONWebToken(Account account)
        {
            var key = _config["Jwt:Key"];
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.Email, account.Email),              // ✅ sử dụng chuẩn ClaimTypes.Email
        new Claim("id", account.Id.ToString()),
        new Claim("username", account.UserName),
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





        public sealed record LoginRequest(string Email, string Password);

        public sealed record RegisterRequest(string UserName, string FullName, string Email, string Password, string Phone);

    }
}
