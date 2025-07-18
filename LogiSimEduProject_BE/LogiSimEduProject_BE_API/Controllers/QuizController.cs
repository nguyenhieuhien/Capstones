using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using LogiSimEduProject_BE_API.Controllers.DTO.Quiz;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;
        public QuizController(IQuizService service) => _service = service;

        [Authorize(Roles = "Instructor")]
        [HttpGet("get_all_quiz")]
        [SwaggerOperation(Summary = "Get all quizzes", Description = "Returns a list of all quizzes.")]
        public async Task<IEnumerable<Quiz>> Get()
        {
            return await _service.GetAll();
        }

        [Authorize(Roles = "Instructor,Student")]
        [HttpGet("get_quiz/{id}")]
        [SwaggerOperation(Summary = "Get quiz by ID", Description = "Returns quiz details by quiz ID.")]
        public async Task<Quiz> Get(string id)
        {
            return await _service.GetById(id);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_quiz")]
        [SwaggerOperation(Summary = "Create a new quiz", Description = "Creates a basic quiz with topic and score.")]
        public async Task<IActionResult> Post(QuizDTOCreate request)
        {
            var quiz = new Quiz
            {
                LessonId = request.LessonId,
                QuizName = request.QuizName,
                TotalScore = request.TotalScore,
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

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_full_quiz")]
        [SwaggerOperation(Summary = "Create quiz with full questions & answers", Description = "Creates a quiz including its questions and answers in one go.")]
        public async Task<IActionResult> CreateFullQuiz([FromBody] QuizDTO dto)
        {
            var quizId = Guid.NewGuid();
            var quiz = new Quiz
            {
                Id = quizId,
                LessonId = dto.LessonId,
                QuizName = dto.QuizName,
                TotalScore = dto.TotalScore,
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
                            IsCorrect = a.IsAnswerCorrect,
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

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_quiz/{id}")]
        [SwaggerOperation(Summary = "Update quiz", Description = "Update topic, name or score of the quiz.")]
        public async Task<IActionResult> Put(string id, QuizDTOUpdate request)
        {
            var existingQuiz = await _service.GetById(id);
            if (existingQuiz == null)
            {
                return NotFound(new { Message = $"Question with ID {id} was not found." });
            }

            existingQuiz.LessonId = request.LessonId;
            existingQuiz.QuizName = request.QuizName;
            existingQuiz.TotalScore = request.TotalScore;
            existingQuiz.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingQuiz);

            return Ok(new
            {
                Message = "Quiz updated successfully.",
                Data = new
                {
                    LessonId = existingQuiz.LessonId,
                    QuizName = existingQuiz.QuizName,
                    TotalScore = existingQuiz.TotalScore,
                    IsActive = existingQuiz.IsActive,
                }
            });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_quiz/{id}")]
        [SwaggerOperation(Summary = "Delete quiz", Description = "Delete a quiz by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
