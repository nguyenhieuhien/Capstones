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
        private readonly IWebHostEnvironment _env;

        public SceneController(ISceneService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

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
        public async Task<IActionResult> Post([FromForm] SceneDTOCreate request)
        {
            string imgUrl = null;
            if (request.ImgUrl != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads/zip");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                imgUrl = $"/uploads/zip/{fileName}";
            }

            var scene  = new Scene
            {
                SceneName = request.SceneName,
                ImgUrl = imgUrl,
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
        public async Task<IActionResult> Put(string id, [FromForm] SceneDTOUpdate request)
        {
            var existingScene = await _service.GetById(id);
            if (existingScene == null)
            {
                return NotFound(new { Message = $"Scene with ID {id} was not found." });
            }

            if (request.ImgUrl != null)
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads/zip");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                existingScene.ImgUrl = $"/uploads/zip/{fileName}";
            }

            existingScene.SceneName = request.SceneName;
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

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(string id)
        {
            var scene = await _service.GetById(id);
            if (scene == null || string.IsNullOrEmpty(scene.ImgUrl))
                return NotFound("Scene or file not found.");

            // Đường dẫn vật lý đến file ZIP
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", scene.ImgUrl.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            return File(fileBytes, "application/zip", fileName);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
