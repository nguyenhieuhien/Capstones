using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.DTO.LessonSubmission;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonSubmissionController : ControllerBase
    {
        private readonly ILessonSubmissionService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        public LessonSubmissionController(ILessonSubmissionService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [HttpGet("get_all_lesson_submission")]
        [SwaggerOperation(Summary = "Get all lesson submissions", Description = "Returns a list of all lesson submissions.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get_lesson_submission/{id}")]
        [SwaggerOperation(Summary = "Get lesson submission by ID", Description = "Returns a specific lesson submission based on the provided ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var submission = await _service.GetById(id);
            if (submission == null) return NotFound("Lesson not found");
            return Ok(submission);
        }

        [HttpPost("submit-lesson")]
        [SwaggerOperation(Summary = "Submit a lesson", Description = "Student submits a lesson with an optional file.")]
        public async Task<IActionResult> SubmitLesson([FromForm] LessonSubmissionDTOCreate dto)
        {
            string fileUrl = null;

            if (dto.FileUrl != null)
            {
                await using var stream = dto.FileUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(dto.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Submissions",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            var submission = new LessonSubmission
            {
                LessonId = dto.LessonId,
                AccountId = dto.AccountId,
                FileUrl = fileUrl,
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow,
            };

            var (success, message, id) = await _service.SubmitLesson(submission);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update-submission/{id}")]
        [SwaggerOperation(Summary = "Update submitted lesson", Description = "Update an existing lesson submission.")]
        public async Task<IActionResult> UpdateSubmission(string id, [FromForm] LessonSubmissionDTOUpdate dto)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound(new { Message = $"Submission with ID {id} not found." });

            string fileUrl = existing.FileUrl;

            if (dto.FileUrl != null)
            {
                await using var stream = dto.FileUrl.OpenReadStream();
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(dto.FileUrl.FileName, stream),
                    Folder = "LogiSimEdu_Submissions",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    fileUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            existing.FileUrl = fileUrl;
            existing.Note = dto.Note;
            existing.UpdatedAt = DateTime.UtcNow;

            var (success, message) = await _service.Update(existing);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [HttpPut("grade-submission/{id}")]
        [SwaggerOperation(Summary = "Grade a lesson submission", Description = "Instructor grades a lesson submission.")]
        public async Task<IActionResult> GradeSubmission(string id, [FromBody] LessonSubmissionDTOGrade dto)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound(new { Message = $"Submission with ID {id} not found." });

            existing.TotalScore = dto.TotalScore;
            existing.UpdatedAt = DateTime.UtcNow;

            var (success, message) = await _service.Update(existing);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }
        [HttpDelete("delete_notification/{id}")]
        [SwaggerOperation(Summary = "Delete a lesson submission", Description = "Deletes a lesson submission by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(message);
            return Ok(new { Message = message });
        }
    }
}
