using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using LogiSimEduProject_BE_API.Controllers.DTO.Scene;
using LogiSimEduProject_BE_API.Controllers.DTO.Topic;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/scene")]
    [ApiController]
    public class SceneController : ControllerBase
    {
        private readonly ISceneService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public SceneController(ISceneService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [HttpGet("get_all_scene")]
        [SwaggerOperation(Summary = "Get all scenes", Description = "Returns a list of all available scenes.")]
        public async Task<IEnumerable<Scene>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_scene/{id}")]
        [SwaggerOperation(Summary = "Get a scene by ID", Description = "Returns a scene object by its unique identifier.")]
        public async Task<Scene> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_scene")]
        [SwaggerOperation(Summary = "Create a new scene", Description = "Uploads a ZIP file and creates a new scene entry.")]
        public async Task<IActionResult> Post([FromForm] SceneDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Scenes",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true,
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != HttpStatusCode.OK)
                    return StatusCode((int)result.StatusCode, result.Error?.Message);

                imgUrl = result.SecureUrl.ToString();
            }

            var scene  = new Scene
            {
                SceneName = request.SceneName,
                ImgUrl = imgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var saveResult = await _service.Create(scene);
            if (saveResult <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_scene/{id}")]
        [SwaggerOperation(Summary = "Update scene info", Description = "Updates the name, description or ZIP file of a scene.")]
        public async Task<IActionResult> Put(string id, [FromForm] SceneDTOUpdate request)
        {
            var existingScene = await _service.GetById(id);
            if (existingScene == null)
            {
                return NotFound(new { Message = $"Scene with ID {id} was not found." });
            }

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Scenes",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != HttpStatusCode.OK)
                    return StatusCode((int)result.StatusCode, result.Error?.Message);

                existingScene.ImgUrl = result.SecureUrl.ToString();
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

        [HttpGet("download_scene/{id}")]
        [SwaggerOperation(Summary = "Download ZIP file of a scene", Description = "Downloads the uploaded ZIP file of the given scene.")]
        public async Task<IActionResult> Download(string id)
        {
            var scene = await _service.GetById(id);
            if (scene == null || string.IsNullOrEmpty(scene.ImgUrl))
                return NotFound("Scene or file not found.");

            return Redirect(scene.ImgUrl);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("delete_scene/{id}")]
        [SwaggerOperation(Summary = "Delete a scene", Description = "Removes a scene from the system by ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
