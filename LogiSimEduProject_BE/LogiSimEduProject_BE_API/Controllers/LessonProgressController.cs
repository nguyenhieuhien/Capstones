using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Lesson;
using Services.DTO.LessonProgress;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/lessonProgress")]
    [ApiController]
    public class LessonProgressController : ControllerBase
    {
        private readonly ILessonProgressService _service;

        public LessonProgressController(ILessonProgressService service)
        {
            _service = service;
        }

        [HttpGet("get_all_lessonProgress")]
        //[SwaggerOperation(Summary = "Get all lessons", Description = "Returns a list of all lessons.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get_lessonProgress/{id}")]
        //[SwaggerOperation(Summary = "Get lesson by ID", Description = "Returns a specific lesson based on the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("LessonProgress not found");
            return Ok(lesson);
        }

        [HttpPost("create_lessonProgress")]
        //[SwaggerOperation(Summary = "Create a new lesson", Description = "Creates a new lesson and saves it to the database.")]
        public async Task<IActionResult> Post([FromBody] LessonProgressDTOCreate request)
        {
            var model = new LessonProgress
            {
                AccountId = request.AccountId,
                LessonId = request.LessonId,
                Status = request.Status,
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update-lesson-progress")]
        [SwaggerOperation(Summary = "Update lesson progress", Description = "Update student's lesson progress and recalculate course progress.")]
        public async Task<IActionResult> UpdateLessonProgress([FromHeader] Guid accountId, [FromHeader] Guid lessonId, [FromBody] UpdateLessonProgressDto request)
        {
            var (success, message, certificate) = await _service.UpdateLessonProgressAsync(accountId, lessonId, request.Status);
            return success ? Ok(new { Message = message, Certificate = certificate }) : BadRequest(new { Message = message });
        }

        [HttpPut("update_lesson/{id}")]
        //[SwaggerOperation(Summary = "Update an existing lesson", Description = "Updates the title or description of a lesson.")]
        public async Task<IActionResult> Put(string id, [FromBody] LessonProgressDTOUpdate request)
        {
            var lesson = await _service.GetById(id);
            if (lesson == null) return NotFound("Lesson not found");

            lesson.AccountId = request.AccountId;
            lesson.LessonId = request.LessonId;
            lesson.Status = request.Status;

            var (success, message) = await _service.Update(lesson);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [HttpDelete("delete_notification/{id}")]
        //[SwaggerOperation(Summary = "Delete a lesson", Description = "Deletes a lesson by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(message);
            return Ok(new { Message = message });
        }
    }
}
