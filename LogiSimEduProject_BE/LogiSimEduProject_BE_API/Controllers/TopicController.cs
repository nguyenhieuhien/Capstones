using Azure.Core;

using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Services.DTO.Topic;

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

        [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_topic")]
        [SwaggerOperation(Summary = "Get all topics", Description = "Returns a list of all topics.")]
        public async Task<IEnumerable<Topic>> Get()
        {
            return await _service.GetAll();
        }

        [Authorize(Roles = "Student,Instructor")]
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

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_topic")]
        [SwaggerOperation(Summary = "Create new topic", Description = "Creates a new topic and uploads image if provided.")]
        public async Task<IActionResult> Post([FromForm] TopicDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Topics",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    imgUrl = result.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)result.StatusCode, result.Error?.Message);
                }
            }
            var topic = new Topic
            {
                CourseId = request.CourseId,
                TopicName = request.TopicName,
                ImgUrl = imgUrl,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var resultCreate = await _service.Create(topic);
            if (resultCreate <= 0)
                return BadRequest("Fail Create");

            return Ok(new { Data = request });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_topic/{id}")]
        [SwaggerOperation(Summary = "Update topic", Description = "Updates an existing topic, including uploading a new image if provided.")]
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
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Topics",
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
                    CourseId = existingTopic.CourseId,
                    TopicName = existingTopic.TopicName,
                    ImgUrl = existingTopic.ImgUrl,
                    Description = existingTopic.Description,
                }
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_topic/{id}")]
        [SwaggerOperation(Summary = "Delete topic", Description = "Deletes a topic by ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
