using Azure.Core;

using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Services.DTO.Topic;
using Microsoft.Identity.Client;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/topic")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly ITopicService _service;

        public TopicController(ITopicService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_topic")]
        [SwaggerOperation(Summary = "Get all topics", Description = "Returns a list of all topics.")]
        public async Task<IEnumerable<Topic>> Get()
        {
            return await _service.GetAll();
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_topic/{id}")]
        [SwaggerOperation(Summary = "Get topic by ID", Description = "Returns a specific topic by its ID.")]
        public async Task<Topic> Get(string id)
        {
            return await _service.GetById(id);
        }

        [HttpGet("by-course/{courseId}")]
        [SwaggerOperation(Summary = "Get topics by Course ID", Description = "Returns all topics associated with a specific Course ID.")]
        public async Task<IActionResult> GetByCourseId(Guid courseId)
        {
            var topics = await _service.GetTopicsByCourseId(courseId);
            return Ok(topics);
        }

        [HttpGet("course/{courseId:guid}/with-student-finish")]
        public async Task<ActionResult<List<TopicWithFinishDTO>>> GetTopicsWithStudentFinish(
        Guid courseId,
        [FromQuery] int completedStatus = 2)
        {
            var result = await _service.GetTopicsByCourseIdAsync(courseId, completedStatus);
            return Ok(result);
        }


        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_topic")]
        [SwaggerOperation(
            Summary = "Create new topic",
            Description = "Creates a new topic (no image upload).")]
        public async Task<IActionResult> Post([FromBody] TopicDTOCreate request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var topic = new Topic
            {
                CourseId = request.CourseId,
                TopicName = request.TopicName,
                Description = request.Description,
                OrderIndex = request.OrderIndex,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var resultCreate = await _service.Create(topic);
            if (resultCreate <= 0)
                return BadRequest("Fail Create");

            // Có thể trả về entity đã tạo hoặc id; tuỳ convention bạn đang dùng
            return Ok(new
            {
                Message = "Topic created successfully.",
                Data = new
                {
                    topic.Id,
                    topic.CourseId,
                    topic.TopicName,
                    topic.Description,
                    topic.OrderIndex,
                    topic.IsActive,
                    topic.CreatedAt
                }
            });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("update_topic/{id}")]
        [SwaggerOperation(
            Summary = "Update topic",
            Description = "Updates an existing topic (no image upload).")]
        public async Task<IActionResult> Put(string id, [FromBody] TopicDTOUpdate request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingTopic = await _service.GetById(id);
            if (existingTopic == null)
                return NotFound(new { Message = $"Topic with ID {id} was not found." });

            // Cập nhật các trường cho phép
            existingTopic.CourseId = request.CourseId;
            existingTopic.TopicName = request.TopicName;
            existingTopic.Description = request.Description;
            existingTopic.OrderIndex = request.OrderIndex;
            //existingTopic.IsActive = request.IsActive; // nếu DTOUpdate có cờ này
            existingTopic.UpdatedAt = DateTime.UtcNow;

            var updated = await _service.Update(existingTopic);
            if (updated <= 0)
                return BadRequest("Fail Update");

            return Ok(new
            {
                Message = "Topic updated successfully.",
                Data = new
                {
                    existingTopic.Id,
                    existingTopic.CourseId,
                    existingTopic.TopicName,
                    existingTopic.Description,
                    existingTopic.OrderIndex,
                    existingTopic.IsActive,
                    existingTopic.UpdatedAt
                }
            });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpDelete("delete_topic/{id}")]
        [SwaggerOperation(Summary = "Delete topic", Description = "Deletes a topic by ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
