using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Topic;
using LogiSimEduProject_BE_API.Controllers.DTO.Workspace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _service;
        public WorkspaceController(IWorkspaceService service) => _service = service;

        // GET: api/<WorkspaceController>
        [HttpGet("GetAllWorkSpace")]
        public async Task<IEnumerable<WorkSpace>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetWorkSpace/{id}")]
        public async Task<WorkSpace> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateWorkSpace/")]
        public async Task<IActionResult> Post(WorkspaceDTOCreate request)
        {
            var workspace = new WorkSpace
            {
                OrderId = request.OrderId,
                OrganizationId = request.OrganizationId,
                WorkSpaceName = request.WorkSpaceName,
                NumberOfAccount = request.NumberOfAccount,
                ImgUrl = request.ImgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(workspace);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdateWorkSpace/{id}")]
        public async Task<IActionResult> Put(string id, WorkspaceDTOUpdate request)
        {
            var existingWorkSpace = await _service.GetById(id);
            if (existingWorkSpace == null)
            {
                return NotFound(new { Message = $"WorkSpace with ID {id} was not found." });
            }
            existingWorkSpace.OrderId = request.OrderId;
            existingWorkSpace.OrganizationId = request.OrganizationId;
            existingWorkSpace.WorkSpaceName = request.WorkSpaceName;
            existingWorkSpace.NumberOfAccount = request.NumberOfAccount;
            existingWorkSpace.ImgUrl = request.ImgUrl;
            existingWorkSpace.Description = request.Description;
            existingWorkSpace.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingWorkSpace);

            return Ok(new
            {
                Message = "WorkSpace updated successfully.",
                Data = new
                {
                    OrderId = existingWorkSpace.OrderId,
                    CourseId = existingWorkSpace.OrganizationId,
                    WorkSpaceName = existingWorkSpace.WorkSpaceName,
                    ImgUrl = existingWorkSpace.ImgUrl,
                    Description = existingWorkSpace.Description,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeleteWorkSpace/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
