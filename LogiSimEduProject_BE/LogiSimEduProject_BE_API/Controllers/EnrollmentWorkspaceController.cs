using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.DTO.AccountOfWorkSpace;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/accountOfWorkSpace")]
    [ApiController]
    public class EnrollmentWorkspaceController : ControllerBase
    {
        private readonly IEnrollmentWorkSpaceService _service;

        public EnrollmentWorkspaceController(IEnrollmentWorkSpaceService service)
        {
            _service = service;
        }


        [HttpGet("get_all_accountOfWorkSpace")]
        [SwaggerOperation(Summary = "Get all account-workspace relations", Description = "Retrieve all records of accounts assigned to workspaces")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Get account-workspace relation by ID", Description = "Retrieve a specific account-workspace relation by ID")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetById(id);
            return result != null ? Ok(result) : NotFound($"Không t?m th?y b?n ghi v?i ID = {id}");
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("create_accountOfWorkSpace")]
        [SwaggerOperation(Summary = "Create account-workspace relation", Description = "Assign an account to a workspace")]
        public async Task<IActionResult> Create([FromBody] AccountOfWorkSpaceDTOCreate request)
        {
            var model = new EnrollmentWorkSpace
            {
                AccountId = request.AccountId,
                WorkSpaceId = request.WorkSpaceId
            };

            var (success, message) = await _service.Create(model);
            return success ? Ok(new { message, data = request }) : BadRequest(message);
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("update_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Update account-workspace relation", Description = "Update an existing account-workspace record by ID")]
        public async Task<IActionResult> Update(string id, [FromBody] AccountOfWorkSpaceDTOUpdate request)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound($"Không t?m th?y b?n ghi v?i ID = {id}");

            existing.AccountId = request.AccountId;
            existing.WorkSpaceId = request.WorkSpaceId;

            var (success, message) = await _service.Update(existing);
            return success ? Ok(new { message }) : BadRequest(message);
        }

        //[Authorize(Roles = "Admin")]
        [HttpDelete("delete_accountOfWorkSpace/{id}")]
        [SwaggerOperation(Summary = "Delete account-workspace relation", Description = "Delete an account-workspace relation by its ID")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
