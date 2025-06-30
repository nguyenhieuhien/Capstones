using LogiSimEduProject_BE_API.Controllers.DTO.Scene;
using LogiSimEduProject_BE_API.Controllers.DTO.SceneOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SceneOfWorkspaceController : ControllerBase
    {
        private readonly ISceneOfWorkSpaceService _service;

        public SceneOfWorkspaceController(ISceneOfWorkSpaceService service) => _service = service;

        [HttpGet("GetAll")]
        public async Task<IEnumerable<SceneOfWorkSpace>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<SceneOfWorkSpace> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(SceneOfWorkSpaceDTOCreate request)
        {
            var scWs = new SceneOfWorkSpace
            {
                SceneId = request.SceneId,
                WorkSpaceId = request.WorkSpaceId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(scWs);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, SceneOfWorkSpaceDTOUpdate request)
        {
            var existingScWs = await _service.GetById(id);
            if (existingScWs == null)
            {
                return NotFound(new { Message = $"SceneOfWorkSpace with ID {id} was not found." });
            }
            existingScWs.SceneId = request.SceneId;
            existingScWs.WorkSpaceId = request.WorkSpaceId;
            existingScWs.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingScWs);

            return Ok(new
            {
                Message = "SceneOfWorkSpace updated successfully.",
                Data = new
                {
                    SceneId = existingScWs.SceneId,
                    WorkSpaceId = existingScWs.WorkSpaceId,
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
