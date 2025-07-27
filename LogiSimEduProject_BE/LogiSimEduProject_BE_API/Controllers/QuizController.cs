// File: Controllers/QuizController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Account;
using Services.DTO.Quiz;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/quiz")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _service;
        public QuizController(IQuizService service) => _service = service;

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_all_quiz")]
        [SwaggerOperation(Summary = "Get all quizzes", Description = "Returns a list of all quizzes.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        //[Authorize(Roles = "Instructor,Student")]
        [HttpGet("get_quiz/{id}")]
        [SwaggerOperation(Summary = "Get quiz by ID", Description = "Returns quiz details by quiz ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _service.GetById(id);
            if (result == null) return NotFound(new { Message = "Quiz not found." });
            return Ok(result);
        }

        [HttpGet("{quizId}/questions")]
        [SwaggerOperation(Summary = "Get all questions and answers in a quiz")]
        public async Task<IActionResult> GetQuestionsWithAnswers(Guid quizId)
        {
            var result = await _service.GetQuestionsWithAnswersByQuizId(quizId);
            return Ok(result);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_quiz")]
        [SwaggerOperation(Summary = "Create a new quiz", Description = "Creates a basic quiz with topic and score.")]
        public async Task<IActionResult> Create([FromBody] QuizDTOCreate request)
        {
            var quiz = new Quiz
            {
                LessonId = request.LessonId,
                QuizName = request.QuizName,
                TotalScore = request.TotalScore
            };

            var (success, message, id) = await _service.Create(quiz);
            if (!success) return BadRequest(new { Message = message });

            return Ok(new { Message = message, QuizId = id });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPost("create_full_quiz")]
        [SwaggerOperation(Summary = "Create quiz with full questions & answers", Description = "Creates a quiz including its questions and answers in one go.")]
        public async Task<IActionResult> CreateFullQuiz([FromBody] QuizDTO dto)
        {
            var (success, message) = await _service.CreateFullQuiz(new Quiz
            {
                LessonId = dto.LessonId,
                QuizName = dto.QuizName,
                TotalScore = dto.TotalScore,
                Questions = dto.Questions.Select(q => new Question
                {
                    Description = q.Description,
                    Answers = q.Answers.Select(a => new Answer
                    {
                        Description = a.Description,
                        IsCorrect = a.IsAnswerCorrect
                    }).ToList()
                }).ToList()
            });

            if (!success) return BadRequest(new { Message = message });
            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_quiz/{id}")]
        [SwaggerOperation(Summary = "Update quiz", Description = "Update topic, name or score of the quiz.")]
        public async Task<IActionResult> Update(string id, [FromBody] QuizDTOUpdate request)
        {
            var quiz = await _service.GetById(id);
            if (quiz == null) return NotFound(new { Message = "Quiz not found." });

            quiz.LessonId = request.LessonId;
            quiz.QuizName = request.QuizName;
            quiz.TotalScore = request.TotalScore;

            var (success, message) = await _service.Update(quiz);
            if (!success) return BadRequest(new { Message = message });

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_quiz/{id}")]
        [SwaggerOperation(Summary = "Delete quiz", Description = "Delete a quiz by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(new { Message = message });

            return Ok(new { Message = message });
        }
    }
}
