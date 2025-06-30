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

        private readonly ITopicService _service;

        public TopicController(ITopicService service) => _service = service;

        // GET: api/<TopicController>
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Topic>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Topic> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(TopicDTOCreate request)
        {
            var topic = new Topic
            {
                SceneId = request.SceneId,
                CourseId = request.CourseId,
                TopicName = request.TopicName,
                ImgUrl = request.ImgUrl,
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, TopicDTOUpdate request)
        {
            var existingTopic = await _service.GetById(id);
            if (existingTopic == null)
            {
                return NotFound(new { Message = $"Topic with ID {id} was not found." });
            }
            existingTopic.SceneId = request.SceneId;
            existingTopic.CourseId = request.CourseId;
            existingTopic.TopicName = request.TopicName;
            existingTopic.ImgUrl = request.ImgUrl;
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
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
