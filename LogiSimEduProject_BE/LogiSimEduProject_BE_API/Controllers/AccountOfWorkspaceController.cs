
using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/accountOfWorkSpace")]
    [ApiController]
    public class AccountOfWorkspaceController : ControllerBase
    {
        private readonly IAccountOfWorkSpaceService _service;
        public AccountOfWorkspaceController(IAccountOfWorkSpaceService service) => _service = service;

        [HttpGet("get_all_accountOfWorkSpace")]
        [SwaggerOperation(Summary = "Get all account-workspace relations", Description = "Retrieve all records of accounts assigned to workspaces")]
        public async Task<IEnumerable<AccountOfWorkSpace>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Get account-workspace relation by ID", Description = "Retrieve a specific account-workspace relation by ID")]
        public async Task<AccountOfWorkSpace> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_accountOfWorkSpace")]
        [SwaggerOperation(Summary = "Create account-workspace relation", Description = "Assign an account to a workspace")]
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
        [HttpPut("update_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Update account-workspace relation", Description = "Update an existing account-workspace record by ID")]
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
        [HttpDelete("delete_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Delete account-workspace relation", Description = "Delete an account-workspace relation by its ID")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
