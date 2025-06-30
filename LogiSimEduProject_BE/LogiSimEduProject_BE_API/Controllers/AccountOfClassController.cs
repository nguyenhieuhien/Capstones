using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfClass;
using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountOfClassController : ControllerBase
    {
        private readonly IAccountOfClassService _service;
        public AccountOfClassController(IAccountOfClassService service) => _service = service;
        [HttpGet("GetAll")]
        public async Task<IEnumerable<AccountOfClass>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<AccountOfClass> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(AccountOfClassDTOCreate request)
        {
            var answer = new AccountOfClass
            {
                AccountId = request.AccountId,
                ClassId = request.ClassId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(answer);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, AccountOfClassDTOUpdate request)
        {
            var existingAcCl = await _service.GetById(id);
            if (existingAcCl == null)
            {
                return NotFound(new { Message = $"AccountOfClass with ID {id} was not found." });
            }

            existingAcCl.AccountId = request.AccountId;
            existingAcCl.ClassId = request.ClassId;
            existingAcCl.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingAcCl);

            return Ok(new
            {
                Message = "AccountOfClass updated successfully.",
                Data = new
                {
                    AccountId = existingAcCl.AccountId,
                    ClassId = existingAcCl.ClassId,
                    IsActive = existingAcCl.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
