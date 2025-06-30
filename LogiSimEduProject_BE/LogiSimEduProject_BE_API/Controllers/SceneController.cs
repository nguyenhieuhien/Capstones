using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Scene;
using LogiSimEduProject_BE_API.Controllers.DTO.Topic;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SceneController : ControllerBase
    {
        private readonly ISceneService _service;

        public SceneController(ISceneService service) => _service = service;

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Scene>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Scene> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(SceneDTOCreate request)
        {
            var scene  = new Scene
            {
                SceneName = request.SceneName,
                ImgUrl = request.ImgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(scene);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, SceneDTOUpdate request)
        {
            var existingScene = await _service.GetById(id);
            if (existingScene == null)
            {
                return NotFound(new { Message = $"Scene with ID {id} was not found." });
            }
            existingScene.SceneName = request.SceneName;
            existingScene.ImgUrl = request.ImgUrl;
            existingScene.Description = request.Description;
            existingScene.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingScene);

            return Ok(new
            {
                Message = "Scene updated successfully.",
                Data = new
                {
                    SceneName = existingScene.SceneName,
                    ImgUrl = existingScene.ImgUrl,
                    Description = existingScene.Description,
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
