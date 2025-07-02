using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _service;
        public QuestionController(IQuestionService service) => _service = service;
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Question>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Question> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(QuestionDTOCreate request)
        {
            var question = new Question
            {
                QuizId = request.QuizId,
                Description = request.Description,
                IsCorrect = request.IsCorrect,
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

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id,QuestionDTOUpdate request)
        {
            var existingQuestion = await _service.GetById(id);
            if (existingQuestion == null)
            {
                return NotFound(new { Message = $"Question with ID {id} was not found." });
            }

            existingQuestion.QuizId = request.QuizId;
            existingQuestion.Description = request.Description;
            existingQuestion.IsCorrect = request.IsCorrect;
            existingQuestion.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingQuestion);

            return Ok(new
            {
                Message = "Question updated successfully.",
                Data = new
                {
                    QuizId = existingQuestion.QuizId,
                    Description = existingQuestion.Description,
                    IsCorrect = existingQuestion.IsCorrect,
                    IsActive = existingQuestion.IsActive,
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
