using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Services.Controllers.DTO.Workspace;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/workspace")]
    [ApiController]
    public class WorkspaceController : ControllerBase
    {
        private readonly IWorkspaceService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        public WorkspaceController(IWorkspaceService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpGet("get_all_workSpace")]
        [SwaggerOperation(Summary = "Get all workspaces", Description = "Returns a list of all workspaces.")]
        public async Task<IEnumerable<WorkSpace>> Get()
        {
            return await _service.GetAll();
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpGet("get_workSpace/{id}")]
        [SwaggerOperation(Summary = "Get a workspace by ID", Description = "Returns details of a specific workspace.")]
        public async Task<WorkSpace> Get(string id)
        {
            return await _service.GetById(id);
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpPost("create_workSpace")]
        [SwaggerOperation(Summary = "Create new workspace", Description = "Creates a new workspace with given information.")]
        public async Task<IActionResult> Post([FromForm] WorkspaceDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Workspaces",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    imgUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            var workspace = new WorkSpace
            {
                OrganizationId = request.OrganizationId,
                WorkSpaceName = request.WorkSpaceName,
                NumberOfAccount = request.NumberOfAccount,
                ImgUrl = imgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _service.Create(workspace);
            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new { Data = workspace });
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpPut("update_workSpace/{id}")]
        [SwaggerOperation(Summary = "Update workspace", Description = "Updates an existing workspace by ID.")]
        public async Task<IActionResult> Put(string id, WorkspaceDTOUpdate request)
        {
            var existingWorkSpace = await _service.GetById(id);
            if (existingWorkSpace == null)
                return NotFound(new { Message = $"WorkSpace with ID {id} was not found." });

            string imgUrl = existingWorkSpace.ImgUrl;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Workspaces",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    imgUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }
            existingWorkSpace.OrganizationId = request.OrganizationId;
            existingWorkSpace.WorkSpaceName = request.WorkSpaceName;
            existingWorkSpace.NumberOfAccount = request.NumberOfAccount;
            existingWorkSpace.ImgUrl = imgUrl;
            existingWorkSpace.Description = request.Description;
            existingWorkSpace.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingWorkSpace);

            return Ok(new
            {
                Message = "WorkSpace updated successfully.",
                Data = new
                {
                    CourseId = existingWorkSpace.OrganizationId,
                    WorkSpaceName = existingWorkSpace.WorkSpaceName,
                    ImgUrl = existingWorkSpace.ImgUrl,
                    Description = existingWorkSpace.Description,
                }
            });
        }

        [Authorize(Roles = "Organization_Admin")]
        [HttpDelete("delete_workSpace/{id}")]
        [SwaggerOperation(Summary = "Delete workspace", Description = "Deletes a workspace by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
