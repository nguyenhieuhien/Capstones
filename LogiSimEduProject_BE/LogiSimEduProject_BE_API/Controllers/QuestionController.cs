
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.Controllers.DTO.Question;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;

        public QuestionController(IQuestionService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("get_all_question")]
        [SwaggerOperation(Summary = "Get all questions", Description = "Returns a list of all questions.")]
        public async Task<IActionResult> GetAll()
        {
            var questions = await _service.GetAll();
            return Ok(questions);
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("get_question/{id}")]
        [SwaggerOperation(Summary = "Get a question by ID", Description = "Returns a specific question using the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var question = await _service.GetById(id);
            if (question == null)
                return NotFound(new { Message = $"Question with ID {id} not found." });

            return Ok(question);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_question")]
        [SwaggerOperation(Summary = "Create a new question", Description = "Creates and saves a new question to the database.")]
        public async Task<IActionResult> Post([FromBody] QuestionDTOCreate request)
        {
            var question = new Question
            {
                QuizId = request.QuizId,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var (success, message, id) = await _service.Create(question);

            if (!success)
                return BadRequest(new { Message = message });

            return Ok(new
            {
                Message = message,
                Data = new { Id = id, request.QuizId, request.Description }
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_question/{id}")]
        [SwaggerOperation(Summary = "Update a question", Description = "Updates the quiz ID or description of an existing question.")]
        public async Task<IActionResult> Put(string id, [FromBody] QuestionDTOUpdate request)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound(new { Message = $"Question with ID {id} not found." });

            existing.QuizId = request.QuizId;
            existing.Description = request.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            var (success, message) = await _service.Update(existing);

            if (!success)
                return BadRequest(new { Message = message });

            return Ok(new
            {
                Message = message,
                Data = new { existing.Id, existing.QuizId, existing.Description }
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_question/{id}")]
        [SwaggerOperation(Summary = "Delete a question", Description = "Deletes a question by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success)
                return NotFound(new { Message = message });

            return Ok(new { Message = message });
        }
    }
}
