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

        public LessonController(ILessonService service)
        {
            _service = service;
        }

        [HttpGet("get_all_lesson")]
        [SwaggerOperation(Summary = "Get all lessons", Description = "Returns a list of all lessons.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get_lesson/{id}")]
        [SwaggerOperation(Summary = "Get lesson by ID", Description = "Returns a specific lesson based on the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("Lesson not found");
            return Ok(lesson);
        }

        [HttpGet("by-topic/{topicId}")]
        [SwaggerOperation(Summary = "Get lessons by Topic ID", Description = "Returns all lessons associated with a specific Topic ID.")]
        public async Task<IActionResult> GetByTopicId(Guid topicId)
        {
            var lessons = await _service.GetLessonsByTopicId(topicId);
            return Ok(lessons);
        }

        [HttpGet("{lessonId}/quizzes")]
        public async Task<IActionResult> GetQuizzesForLesson(Guid lessonId)
        {
            var quizzes = await _service.GetQuizzesByLessonId(lessonId);
            if (!quizzes.Any()) return NotFound(new { Message = "No quizzes found for this lesson." });
            return Ok(quizzes);
        }

        [HttpPost("create_lesson")]
        [SwaggerOperation(Summary = "Create a new lesson", Description = "Creates a new lesson and saves it to the database.")]
        public async Task<IActionResult> Post([FromBody] LessonDTOCreate request)
        {
            var model = new Lesson
            {
                TopicId = request.TopicId,
                LessonName = request.LessonName,
                Status = request.Status,
                Title = request.Title,
                Description = request.Description,
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update_lesson/{id}")]
        [SwaggerOperation(Summary = "Update an existing lesson", Description = "Updates the title or description of a lesson.")]
        public async Task<IActionResult> Put(string id, [FromBody] LessonDTOUpdate request)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("Lesson not found");

            lesson.TopicId = request.TopicId;
            lesson.LessonName = request.LessonName; 
            lesson.Status = request.Status;
            lesson.Title = request.Title;
            lesson.Description = request.Description;

            var (success, message) = await _service.Update(lesson);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

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
