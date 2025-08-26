using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Lesson;
using Services.DTO.Notification;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/lesson")]
    [ApiController]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public LessonController(ILessonService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_lesson")]
        [SwaggerOperation(Summary = "Get all lessons", Description = "Returns a list of all lessons.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_lesson/{id}")]
        [SwaggerOperation(Summary = "Get lesson by ID", Description = "Returns a specific lesson based on the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("Lesson not found");
            return Ok(lesson);
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("by-topic/{topicId}")]
        [SwaggerOperation(Summary = "Get lessons by Topic ID", Description = "Returns all lessons associated with a specific Topic ID.")]
        public async Task<IActionResult> GetByTopicId(Guid topicId)
        {
            var lessons = await _service.GetLessonsByTopicId(topicId);
            return Ok(lessons);
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("{lessonId}/quizzes")]
        public async Task<IActionResult> GetQuizzesForLesson(Guid lessonId)
        {
            var quizzes = await _service.GetQuizzesByLessonId(lessonId);
            if (!quizzes.Any()) return NotFound(new { Message = "No quizzes found for this lesson." });
            return Ok(quizzes);
        }


        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_lesson")]
        [SwaggerOperation(Summary = "Create a new lesson", Description = "Creates a new lesson, uploads file to Cloudinary if provided, and saves it to the database.")]
        public async Task<IActionResult> Post([FromForm] LessonDTOCreate request)
        {
            string? fileUrl = null;

            // Nếu có file thì upload
            if (request.FileUrl != null)
            {
                await using var stream = request.FileUrl.OpenReadStream();
                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(request.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Lessons/Videos",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl?.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            var model = new Lesson
            {
                TopicId = request.TopicId,
                ScenarioId = request.ScenarioId,
                LessonName = request.LessonName,
                Status = request.Status,
                Title = request.Title,
                Description = request.Description,
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new
            {
                Message = message,
                Id = id,
                Data = new
                {
                    model.TopicId,
                    model.LessonName,
                    model.Status,
                    model.Title,
                    model.Description,
                    model.FileUrl
                }
            });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("update_lesson/{id}")]
        [SwaggerOperation(Summary = "Update an existing lesson", Description = "Updates fields of a lesson and uploads a new file to Cloudinary if provided.")]
        public async Task<IActionResult> Put(string id, [FromForm] LessonDTOUpdate request)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("Lesson not found");

            string? fileUrl = lesson.FileUrl;

            // Nếu client upload file mới -> upload & thay thế
            if (request.FileUrl != null)
            {
                await using var stream = request.FileUrl.OpenReadStream();

                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(request.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Lessons",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl?.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            // Cập nhật các trường
            lesson.TopicId = request.TopicId;
            lesson.ScenarioId = request.ScenarioId;
            lesson.LessonName = request.LessonName;
            lesson.Status = request.Status;
            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.FileUrl = fileUrl; // giữ nguyên nếu không upload mới
            lesson.UpdatedAt = DateTime.UtcNow;

            var (success, message) = await _service.Update(lesson);
            if (!success) return BadRequest(message);

            return Ok(new
            {
                Message = message,
                Data = new
                {
                    lesson.Id,
                    lesson.TopicId,
                    lesson.ScenarioId,
                    lesson.LessonName,
                    lesson.Status,
                    lesson.Title,
                    lesson.Description,
                    lesson.FileUrl
                }
            });
        }


        //[Authorize(Roles = "Instructor")]
        [HttpDelete("delete_lesson/{id}")]
        [SwaggerOperation(Summary = "Delete a lesson", Description = "Deletes a lesson by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(message);
            return Ok(new { Message = message });
        }
    }
}
