using LogiSimEduProject_BE_API.Controllers.DTO.EnrollmentRequestment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/enrollmentRequest")]
    [ApiController]
    public class EnrollmentRequestController : ControllerBase
    {
        private readonly IEnrollmentRequestService _service;
        private readonly IAccountService _accountService;

        public EnrollmentRequestController(
            IEnrollmentRequestService service,
            IAccountService accountService)
        {
            _service = service;
            _accountService = accountService;
        }

        [HttpGet("get_all_enrollmentRequest")]
        [SwaggerOperation(Summary = "Get all enrollment requests", Description = "Retrieve all enrollment requests from all students.")]
        public async Task<IEnumerable<EnrollmentRequest>> GetAll()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_enrollmentRequest_by_corse/{courseId}")] // Lấy tất cả yêu cầu theo course
        [SwaggerOperation(Summary = "Get requests by course", Description = "Get all enrollment requests submitted for a specific course.")]
        public async Task<IEnumerable<EnrollmentRequest>> GetByCourse(string courseId)
        {
            return await _service.GetByCourseId(courseId);
        }

        [HttpGet("get_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Get enrollment request by ID", Description = "Retrieve a specific enrollment request by its ID.")]
        public async Task<ActionResult<EnrollmentRequest>> Get(string id)
        {
            var item = await _service.GetById(id);
            if (item == null)
                return NotFound();
            return item;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("create_enrollmentRequest")] // Student gửi yêu cầu
        [SwaggerOperation(Summary = "Create enrollment request", Description = "Submit a request to enroll in a course (only for students).")]
        public async Task<IActionResult> Post([FromBody] EnrollmentRequestDTOCreate request)
        {
            // Kiểm tra tài khoản tồn tại
            var account = await _accountService.GetById(request.StudentId.ToString());
            if (account == null)
                return BadRequest("Tài khoản không tồn tại.");

            if (account.SystemMode != true || account.OrganizationRole?.ToLower() != "student")
                return Unauthorized("Chỉ tài khoản Student mới được gửi yêu cầu.");


            var model = new EnrollmentRequest
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            var result = await _service.Create(model);
            if (result <= 0)
                return BadRequest("Request failed");

            return Ok(new { Message = "Enrollment request sent successfully." });
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("update_enrollmentRequest/{id}")] // Instructor duyệt hoặc từ chối yêu cầu
        [SwaggerOperation(Summary = "Update request status", Description = "Approve or deny an enrollment request (only for instructors).")]
        public async Task<IActionResult> Put(string id, [FromBody] string status)
        {
            var request = await _service.GetById(id);
            if (request == null)
                return NotFound();

            if (status != "Accepted" && status != "Denied")
                return BadRequest("Invalid status");

            request.Status = status;
            request.RespondedAt = DateTime.UtcNow;

            await _service.Update(request);

            return Ok(new { Message = $"Request has been {status.ToLower()}." });
        }

        [HttpDelete("delete_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Delete enrollment request", Description = "Delete an enrollment request by ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
