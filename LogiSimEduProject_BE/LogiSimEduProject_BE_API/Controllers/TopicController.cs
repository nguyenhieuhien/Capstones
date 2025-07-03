using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Topic;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ITopicService _service;

        public TopicController(ITopicService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // GET: api/<TopicController>
        [HttpGet("GetAllTopic")]
        public async Task<IEnumerable<Topic>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetTopic/{id}")]
        public async Task<Topic> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateTopic")]
        public async Task<IActionResult> Post([FromForm] TopicDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads/topics");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                imgUrl = $"/uploads/topics/{fileName}";
            }
            var topic = new Topic
            {
                SceneId = request.SceneId,
                CourseId = request.CourseId,
                TopicName = request.TopicName,
                ImgUrl = imgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(topic);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdateTopic/{id}")]
        public async Task<IActionResult> Put(string id, TopicDTOUpdate request)
        {
            var existingTopic = await _service.GetById(id);
            if (existingTopic == null)
            {
                return NotFound(new { Message = $"Topic with ID {id} was not found." });
            }
            string imgUrl = existingTopic.ImgUrl;

            if (request.ImgUrl != null)
            {
                // Nếu có file ảnh mới thì lưu ảnh mới
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads/topics");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                // Nếu muốn xóa ảnh cũ khỏi ổ đĩa, có thể thêm đoạn sau:
                if (!string.IsNullOrEmpty(existingTopic.ImgUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingTopic.ImgUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                imgUrl = $"/uploads/topics/{fileName}";
            }

            existingTopic.SceneId = request.SceneId;
            existingTopic.CourseId = request.CourseId;
            existingTopic.TopicName = request.TopicName;
            existingTopic.ImgUrl = imgUrl;
            existingTopic.Description = request.Description;
            existingTopic.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingTopic);

            return Ok(new
            {
                Message = "Topic updated successfully.",
                Data = new
                {
                    SceneId = existingTopic.SceneId,
                    CourseId = existingTopic.CourseId,
                    TopicName = existingTopic.TopicName,
                    ImgUrl = existingTopic.ImgUrl,
                    Description = existingTopic.Description,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeleteTopic/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
