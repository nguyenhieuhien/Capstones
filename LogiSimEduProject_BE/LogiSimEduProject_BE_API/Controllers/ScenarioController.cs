// File: Controllers/ScenarioController.cs
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Services.DTO.Scenario;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/scenario")]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        public ScenarioController(IScenarioService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_scenario")]
        [SwaggerOperation(Summary = "Get all scenarios", Description = "Returns all active scenarios.")]
        public async Task<ActionResult<IEnumerable<Scenario>>> GetAll()
        {
            var scenarios = await _service.GetAll();
            return Ok(scenarios);
        }

        [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_scenario/{id}")]
        [SwaggerOperation(Summary = "Get scenario by ID", Description = "Retrieve a scenario using its unique identifier.")]
        public async Task<ActionResult<Scenario>> GetById(string id)
        {
            var scenario = await _service.GetById(id);
            if (scenario == null)
                return NotFound(new { Message = $"Scenario with ID {id} not found." });

            return Ok(scenario);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_scenario")]
        [SwaggerOperation(Summary = "Create new scenario", Description = "Instructor can create a new simulation scenario.")]
        public async Task<IActionResult> Create([FromForm] ScenarioDTOCreate dto)
        {
            string fileUrl = null;

            if (dto.FileUrl != null)
            {
                await using var stream = dto.FileUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(dto.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Scenarios",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }
            var scenario = new Scenario
            {
                SceneId = dto.SceneId,
                ScenarioName = dto.ScenarioName,
                Description = dto.Description,
                FileUrl = fileUrl
            };

            var result = await _service.Create(scenario);
            if (result <= 0)
                return BadRequest("Failed to create scenario.");

            return Ok(new { Message = "Scenario created successfully." });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("update_scenario/{id}")]
        [SwaggerOperation(Summary = "Update scenario", Description = "Update existing scenario by ID.")]
        public async Task<IActionResult> Update(string id, [FromBody] ScenarioDTOUpdate dto)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound(new { Message = $"Scenario with ID {id} not found." });

            string fileUrl = existing.FileUrl;

            if (dto.FileUrl != null)
            {
                await using var stream = dto.FileUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(dto.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Topics",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            existing.SceneId = dto.SceneId;
            existing.ScenarioName = dto.ScenarioName;
            existing.Description = dto.Description;
            existing.FileUrl = fileUrl;

            var result = await _service.Update(existing);
            if (result <= 0)
                return BadRequest("Failed to update scenario.");

            return Ok(new { Message = "Scenario updated successfully." });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_scenario/{id}")]
        [SwaggerOperation(Summary = "Delete scenario", Description = "Deletes a scenario by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
 