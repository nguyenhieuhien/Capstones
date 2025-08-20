using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Answer;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/answer")]
    [ApiController]
    public class AnswerController : ControllerBase
    {
        private readonly IAnswerService _service;

        public AnswerController(IAnswerService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_answer")]
        [SwaggerOperation(Summary = "Get all answers", Description = "Retrieve all answers from the system")]
        public async Task<IActionResult> GetAll()
        {
            var answers = await _service.GetAll();
            return Ok(answers);
        }

       // [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_answer/{id}")]
        [SwaggerOperation(Summary = "Get an answer by ID", Description = "Retrieve a specific answer by its ID")]
        public async Task<IActionResult> GetById(string id)
        {
            var answer = await _service.GetById(id);
            return answer != null ? Ok(answer) : NotFound($"Answer with ID {id} not found.");
        }


        [HttpGet("get_all_answer_by_question/{questionId:guid}")]
        public async Task<ActionResult<List<Answer>>> GetAllAnswersByQuestionId(Guid questionId)
        {
            var answers = await _service.GetAllAnswersByQuestionId(questionId);
            return Ok(answers);
        }


        [Authorize(Roles = "Instructor")]
        [HttpPost("create_answer")]
        [SwaggerOperation(Summary = "Create a new answer", Description = "Add a new answer for a specific question")]
        public async Task<IActionResult> Create([FromBody] AnswerDTOCreate request)
        {
            var answer = new Answer
            {
                QuestionId = request.QuestionId,
                Description = request.Description,
                IsCorrect = request.IsCorrect
            };

            var (success, message) = await _service.Create(answer);
            return success ? Ok(new { message, data = answer }) : BadRequest(message);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_answer/{id}")]
        [SwaggerOperation(Summary = "Update an existing answer", Description = "Update description or correctness of an existing answer")]
        public async Task<IActionResult> Update(string id, [FromBody] AnswerDTOUpdate request)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound($"Answer with ID {id} not found.");

            existing.QuestionId = request.QuestionId;
            existing.Description = request.Description;
            existing.IsCorrect = request.IsCorrect;

            var (success, message) = await _service.Update(existing);
            return success ? Ok(new { message, data = existing }) : BadRequest(message);
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_answer/{id}")]
        [SwaggerOperation(Summary = "Delete an answer", Description = "Delete an answer by its ID")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
