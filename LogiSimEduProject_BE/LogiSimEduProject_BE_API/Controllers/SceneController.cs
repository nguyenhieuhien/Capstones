// File: Controllers/SceneController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Scene;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/scene")]
    public class SceneController : ControllerBase
    {
        private readonly ISceneService _sceneService;

        public SceneController(ISceneService sceneService)
        {
            _sceneService = sceneService;
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_scene")]
        [SwaggerOperation(Summary = "Get all scenes", Description = "Retrieve a list of all scenes.")]
        public async Task<ActionResult<List<Scene>>> GetAll()
        {
            var scenes = await _sceneService.GetAll();
            return Ok(scenes);
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_scene/{id}")]
        [SwaggerOperation(Summary = "Get scene by ID", Description = "Retrieve a scene by its ID.")]
        public async Task<ActionResult<Scene>> GetById(string id)
        {
            var scene = await _sceneService.GetById(id);
            if (scene == null) return NotFound();
            return Ok(scene);
        }

        [HttpGet("get_all_by_org/{orgId}")]
        [SwaggerOperation(Summary = "Get all scenes by organization ID", Description = "Retrieve all scenes that belong to a specific organization.")]
        public async Task<IActionResult> GetAllByOrgId(Guid orgId)
        {
            var scenes = await _sceneService.GetAllByOrgId(orgId);
            return Ok(scenes);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_scene")]
        [SwaggerOperation(Summary = "Create new scene", Description = "Create a new scene with basic information.")]
        public async Task<ActionResult<int>> Create([FromBody] SceneDTOCreate dto)
        {
            if (dto == null) return BadRequest();

            var scene = new Scene
            {
                SceneName = dto.SceneName,
                Description = dto.Description
            };

            var result = await _sceneService.Create(scene);
            if (result == 0) return BadRequest("Failed to create scene.");

            return Ok(result);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_scene/{id}")]
        [SwaggerOperation(Summary = "Update scene", Description = "Update an existing scene's information.")]
        public async Task<ActionResult<int>> Update(string id, [FromBody] SceneDTOUpdate dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(id)) return BadRequest();

            var existing = await _sceneService.GetById(id);
            if (existing == null) return NotFound();

            existing.SceneName = dto.SceneName;
            existing.Description = dto.Description;

            var result = await _sceneService.Update(existing);
            if (result == 0) return BadRequest("Failed to update scene.");

            return Ok(result);
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_scene/{id}")]
        [SwaggerOperation(Summary = "Delete scene", Description = "Remove a scene by its ID.")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            var (success, message) = await _sceneService.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
