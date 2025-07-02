using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using LogiSimEduProject_BE_API.Controllers.DTO.Quiz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;
        public QuizController(IQuizService service) => _service = service;
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Quiz>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Quiz> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(QuizDTOCreate request)
        {
            var quiz = new Quiz
            {
                TopicId = request.TopicId,
                QuizName = request.QuizName,
                Score = request.Score,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(quiz);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        [HttpPost("CreateFullQuiz")]
        public async Task<IActionResult> CreateFullQuiz([FromBody] QuizDTO dto)
        {
            var quizId = Guid.NewGuid();
            var quiz = new Quiz
            {
                Id = quizId,
                TopicId = dto.TopicId,
                QuizName = dto.QuizName,
                Score = dto.Score,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Questions = dto.Questions.Select(q =>
                {
                    var questionId = Guid.NewGuid();
                    return new Question
                    {
                        Id = questionId,
                        QuizId = quizId,
                        Description = q.Description,
                        CreatedAt = DateTime.UtcNow,
                        Answers = q.Answers.Select(a => new Answer
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = questionId,
                            Description = a.Description,
                            IsAnswerCorrect = a.IsAnswerCorrect,
                            CreatedAt = DateTime.UtcNow
                        }).ToList()
                    };
                }).ToList()
            };

            var result = await _service.CreateFullQuiz(quiz);
            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new { Message = "Quiz created" });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, QuizDTOUpdate request)
        {
            var existingQuiz = await _service.GetById(id);
            if (existingQuiz == null)
            {
                return NotFound(new { Message = $"Question with ID {id} was not found." });
            }

            existingQuiz.TopicId = request.TopicId;
            existingQuiz.QuizName = request.QuizName;
            existingQuiz.Score = request.Score;
            existingQuiz.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingQuiz);

            return Ok(new
            {
                Message = "Quiz updated successfully.",
                Data = new
                {
                    TopicId = existingQuiz.TopicId,
                    QuizName = existingQuiz.QuizName,
                    Score = existingQuiz.Score,
                    IsActive = existingQuiz.IsActive,
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
