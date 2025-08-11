using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.DTO.Class;
using Services.DTO.Course;
using Services.DTO.CourseProgress;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/courseProgress")]
    [ApiController]
    public class CourseProgressController : ControllerBase
    {
        private readonly ICourseProgressService _service;

        public CourseProgressController(ICourseProgressService service)
        {
            _service = service;
        }

        [HttpGet("get_all_courseProgress")]
        public async Task<IActionResult> GetAll()
        {
            var courseProgresses = await _service.GetAll();
            return Ok(courseProgresses);
        }

        [HttpGet("get_courseProgress/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var courseProgresses = await _service.GetById(id);
            if (courseProgresses == null)
                return NotFound("Course Progresses not found");

            return Ok(courseProgresses);
        }

        [HttpPost("create_courseProgress")]
        public async Task<IActionResult> Create([FromBody] CourseProgressDTOCreate request)
        {
            var model = new CourseProgress
            {
                AccountId = request.AccountId,
                CourseId = request.CourseId,
                ProgressPercent = request.ProgressPercent,
                Status = request.Status,
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update_courseProgress/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] CourseProgressDTOUpdate request)
        {
            var courseProgress = await _service.GetById(id);
            if (courseProgress == null) return NotFound("Course progress not found");

            courseProgress.AccountId = request.AccountId;
            courseProgress.CourseId = request.CourseId;
            courseProgress.ProgressPercent = request.ProgressPercent;
            courseProgress.Status = request.Status;

            var (success, message) = await _service.Update(courseProgress);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [HttpDelete("delete_courseProgress/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success)
                return NotFound(message);

            return Ok(new { Message = message });
        }
    }
}
