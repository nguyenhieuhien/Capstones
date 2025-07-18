using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/question")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;
        public QuestionController(IQuestionService service) => _service = service;

        [Authorize(Roles = "Instructor")]
        [HttpGet("get_all_question")]
        [SwaggerOperation(Summary = "Get all questions", Description = "Returns a list of all questions.")]
        public async Task<IEnumerable<Question>> Get()
        {
            return await _service.GetAll();
        }

        [Authorize(Roles = "Instructor")]
        [HttpGet("get_question/{id}")]
        [SwaggerOperation(Summary = "Get a question by ID", Description = "Returns a specific question using the provided ID.")]
        public async Task<Question> Get(string id)
        {
            return await _service.GetById(id);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_question")]
        [SwaggerOperation(Summary = "Create a new question", Description = "Creates and saves a new question to the database.")]
        public async Task<IActionResult> Post(QuestionDTOCreate request)
        {
            var question = new Question
            {
                QuizId = request.QuizId,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(question);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_question/{id}")]
        [SwaggerOperation(Summary = "Update a question", Description = "Updates the quiz ID, description, or correctness of an existing question.")]
        public async Task<IActionResult> Put(string id,QuestionDTOUpdate request)
        {
            var existingQuestion = await _service.GetById(id);
            if (existingQuestion == null)
            {
                return NotFound(new { Message = $"Question with ID {id} was not found." });
            }

            existingQuestion.QuizId = request.QuizId;
            existingQuestion.Description = request.Description;
            existingQuestion.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingQuestion);

            return Ok(new
            {
                Message = "Question updated successfully.",
                Data = new
                {
                    QuizId = existingQuestion.QuizId,
                    Description = existingQuestion.Description,
                    IsActive = existingQuestion.IsActive,
                }
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_question/{id}")]
        [SwaggerOperation(Summary = "Delete a question", Description = "Deletes a question by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
