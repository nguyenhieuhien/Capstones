
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.DTO.QuizSubmission;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/quizSubmission")]
    [ApiController]
    public class QuizSubmissionController : ControllerBase
    {
        private readonly IQuizSubmissionService _service;

        public QuizSubmissionController(IQuizSubmissionService service)
        {
            _service = service;
        }

        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetByQuizId(Guid quizId)
        {
            var submissions = await _service.GetAllSubmissionByQuizId(quizId);

            if (submissions == null || !submissions.Any())
                return NotFound("No submissions found for this quiz.");

            return Ok(submissions);
        }

        [HttpGet("lesson/{lessonId}/quiz-submissions")]
        public async Task<IActionResult> GetLessonQuizSubmissions(Guid lessonId)
        {
            var result = await _service.GetLessonQuizSubmissionsGroupedByClass(lessonId);

            if (result == null || !result.Any())
                return NotFound("No quiz submissions found for this lesson");

            return Ok(result);
        }

        //[Authorize(Roles = "Student")]
        [HttpPost("submit_quiz")]
        [SwaggerOperation(Summary = "Submit quiz answers", Description = "Student submits their answers to a quiz and receives score.")]
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
