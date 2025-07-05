using LogiSimEduProject_BE_API.Controllers.DTO.QuizSubmission;
using Microsoft.AspNetCore.Mvc;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizSubmissionController : ControllerBase
    {
        private readonly IQuizSubmissionService _service;

        public QuizSubmissionController(IQuizSubmissionService service)
        {
            _service = service;
        }

        [HttpPost("SubmitQuiz")]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionRequest request)
        {
            if (request.QuizId == Guid.Empty || request.AccountId == Guid.Empty || request.Answers.Count == 0)
                return BadRequest("Invalid submission.");

            try
            {
                var totalQuestions = request.Answers.Count;

                var totalCorrect = await _service.SubmitQuiz(
                    request.QuizId,
                    request.AccountId,
                    request.Answers.Select(a => (a.QuestionId, a.AnswerId)).ToList()
                );

                var score = Math.Round((double)totalCorrect / totalQuestions * 10, 2); // thang điểm 10

                return Ok(new
                {
                    Message = "Submit success",
                    TotalQuestions = totalQuestions,
                    TotalCorrect = totalCorrect,
                    Score = score
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Submit failed. Reason: {ex.Message}");
            }
        }
    }
}
