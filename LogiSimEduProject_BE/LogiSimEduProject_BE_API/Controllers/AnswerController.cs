using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using LogiSimEduProject_BE_API.Controllers.DTO.Role;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _service;
        public AnswerController(IAnswerService service) => _service = service;
        [HttpGet("GetAllAnswer")]
        public async Task<IEnumerable<Answer>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetAnswer/{id}")]
        public async Task<Answer> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateAnswer")]
        public async Task<IActionResult> Post(AnswerDTOCreate request)
        {
            var answer = new Answer
            {
                QuestionId = request.QuestionId,
                Description = request.Description,
                IsAnswerCorrect = request.IsAnswerCorrect,
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
        [HttpPut("UpdateAnswer/{id}")]
        public async Task<IActionResult> Put(string id, AnswerDTOUpdate request)
        {
            var existingAnswer = await _service.GetById(id);
            if (existingAnswer == null)
            {
                return NotFound(new { Message = $"Answer with ID {id} was not found." });
            }

            existingAnswer.QuestionId = request.QuestionId;
            existingAnswer.Description = request.Description;
            existingAnswer.IsAnswerCorrect = request.IsAnswerCorrect;
            existingAnswer.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingAnswer);

            return Ok(new
            {
                Message = "Answer updated successfully.",
                Data = new
                {
                    QuestionId = existingAnswer.QuestionId,
                    Description = existingAnswer.Description,
                    IsAnswerCorrect = existingAnswer.IsAnswerCorrect,
                    IsActive = existingAnswer.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeleteAnswer/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
