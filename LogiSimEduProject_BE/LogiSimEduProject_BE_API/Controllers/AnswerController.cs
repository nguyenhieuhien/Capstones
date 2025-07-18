using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/answer")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _service;
        public AnswerController(IAnswerService service) => _service = service;
        [HttpGet("get_all_answer")]
        [SwaggerOperation(Summary = "Get all answers", Description = "Retrieve all answers from the system")]
        public async Task<IEnumerable<Answer>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_answer/{id}")]
        [SwaggerOperation(Summary = "Get an answer by ID", Description = "Retrieve a specific answer by its ID")]
        public async Task<Answer> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_answer")]
        [SwaggerOperation(Summary = "Create a new answer", Description = "Add a new answer for a specific question")]
        public async Task<IActionResult> Post(AnswerDTOCreate request)
        {
            var answer = new Answer
            {
                QuestionId = request.QuestionId,
                Description = request.Description,
                IsCorrect = request.IsCorrect,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(answer);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_answer/{id}")]
        [SwaggerOperation(Summary = "Update an existing answer", Description = "Update description or correctness of an existing answer")]
        public async Task<IActionResult> Put(string id, AnswerDTOUpdate request)
        {
            var existingAnswer = await _service.GetById(id);
            if (existingAnswer == null)
            {
                return NotFound(new { Message = $"Answer with ID {id} was not found." });
            }

            existingAnswer.QuestionId = request.QuestionId;
            existingAnswer.Description = request.Description;
            existingAnswer.IsCorrect = request.IsCorrect;
            existingAnswer.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingAnswer);

            return Ok(new
            {
                Message = "Answer updated successfully.",
                Data = new
                {
                    QuestionId = existingAnswer.QuestionId,
                    Description = existingAnswer.Description,
                    IsAnswerCorrect = existingAnswer.IsCorrect,
                    IsActive = existingAnswer.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("delete_answer/{id}")]
        [SwaggerOperation(Summary = "Delete an answer", Description = "Delete an answer by its ID")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
