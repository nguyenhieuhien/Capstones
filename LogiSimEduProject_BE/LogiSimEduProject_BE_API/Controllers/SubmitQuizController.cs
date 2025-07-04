using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using LogiSimEduProject_BE_API.Controllers.DTO.SubmitQuiz;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubmitQuizController : ControllerBase
    {
        private readonly IQuizSubmissionService _service;

        public SubmitQuizController(IQuizSubmissionService service) => _service = service;

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionRequest request)
        {
            var submission = new QuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = request.QuizId,
                AccountId = request.AccountId,
                SubmittedAt = DateTime.UtcNow,
                ScoreObtained = 0
            };

            var submissionAnswers = request.Answers.Select(a => new QuizSubmissionAnswer
            {
                Id = Guid.NewGuid(),
                QuizSubmissionId = submission.Id,
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId
            }).ToList();

            double score = await _service.SubmitQuizAsync(submission, submissionAnswers);
            return Ok(new { Score = score });
        }
    }
}
