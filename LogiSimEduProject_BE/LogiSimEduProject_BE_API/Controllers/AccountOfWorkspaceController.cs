using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountOfWorkspaceController : ControllerBase
    {
        private readonly IAccountOfWorkSpaceService _service;
        public AccountOfWorkspaceController(IAccountOfWorkSpaceService service) => _service = service;
        [HttpGet("GetAll")]
        public async Task<IEnumerable<AccountOfWorkSpace>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<AccountOfWorkSpace> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(AccountOfWorkSpaceDTOCreate request)
        {
            var answer = new AccountOfWorkSpace
            {
                AccountId = request.AccountId,
                WorkSpaceId = request.WorkSpaceId,
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
        public async Task<IActionResult> Put(string id, AccountOfWorkSpaceDTOUpdate request)
        {
            var existingAccWs = await _service.GetById(id);
            if (existingAccWs == null)
            {
                return NotFound(new { Message = $"AccountOfWorkSpace with ID {id} was not found." });
            }

            existingAccWs.AccountId = request.AccountId;
            existingAccWs.WorkSpaceId = request.WorkSpaceId;
            existingAccWs.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingAccWs);

            return Ok(new
            {
                Message = "AccountOfWorkSpace updated successfully.",
                Data = new
                {
                    QuestionId = existingAccWs.AccountId,
                    Description = existingAccWs.WorkSpaceId,
                    IsActive = existingAccWs.IsActive,
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
