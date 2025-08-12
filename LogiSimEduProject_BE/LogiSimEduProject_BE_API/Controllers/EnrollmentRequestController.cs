// File: Controllers/EnrollmentRequestController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.DTO.EnrollmentRequestment;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

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

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_all_enrollmentRequest")]
        [SwaggerOperation(Summary = "Get all enrollment requests", Description = "Retrieve all enrollment requests from all students.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_enrollmentRequest_by_corse/{courseId}")]
        [SwaggerOperation(Summary = "Get requests by course", Description = "Get all enrollment requests submitted for a specific course.")]
        public async Task<IActionResult> GetByCourse(string courseId)
        {
            var result = await _service.GetByCourseId(courseId);
            return Ok(result);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Get enrollment request by ID", Description = "Retrieve a specific enrollment request by its ID.")]
        public async Task<IActionResult> Get(string id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound("Request not found");
            return Ok(item);
        }

        [HttpGet("student/{accountId}/enrolled-courses")]
        [SwaggerOperation(Summary = "Get enrolled courses for a student", Description = "Returns all courses a student has successfully enrolled.")]
        public async Task<IActionResult> GetEnrolledCourses(Guid accountId)
        {
            var courses = await _service.GetEnrolledCoursesByAccountId(accountId);
            return Ok(new
            {
                Message = $"Found {courses.Count} enrolled course(s)",
                Data = courses
            });
        }

        //[Authorize(Roles = "Student")]
        [HttpPost("create_enrollmentRequest")]
        [SwaggerOperation(Summary = "Create enrollment request", Description = "Submit a request to enroll in a course (only for students).")]
        public async Task<IActionResult> Post([FromBody] EnrollmentRequestDTOCreate request)
        {
            var account = await _accountService.GetById(request.StudentId.ToString());
            if (account == null)
                return BadRequest("Tài khoản không tồn tại.");

            if (account.RoleId != 4)
                return Unauthorized("Chỉ tài khoản Student mới được gửi yêu cầu.");

            var model = new AccountOfCourse
            {
                AccountId = request.StudentId,
                CourseId = request.CourseId,
            };

            var (success, message, id) = await _service.Create(model);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("assign-student-to-class/{id}")]
        public async Task<IActionResult> AssignStudentToClass(Guid Id,[FromBody] AssignStudentToClassDTO dto)
        {

            var (success, message) = await _service.AssignStudentToClass(Id, dto.ClassId);
            return success ? Ok(new { Message = message }) : BadRequest(new { Message = message });
        }

        [HttpGet("class/{classId}/students")]
        public async Task<IActionResult> GetStudentsInClass(Guid classId)
        {
            var students = await _service.GetStudentsInClass(classId);
            return Ok(students);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("accepted_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Accepted request status", Description = "Approve an enrollment request (only for instructors).")]
        public async Task<IActionResult> AcceptedEnroll(string id)
        {
            var request = await _service.GetById(id);
            if (request == null) return NotFound("Request not found");

            request.Status = 1;
            var (success, message) = await _service.Update(request);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [HttpPut("rejected_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Rejected request status", Description = "Approve an enrollment request (only for instructors).")]
        public async Task<IActionResult> RejectedEnroll(string id)
        {
            var request = await _service.GetById(id);
            if (request == null) return NotFound("Request not found");

            request.Status = 2;
            var (success, message) = await _service.Update(request);
            if (!success) return BadRequest(message);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Student")]
        [HttpDelete("delete_enrollmentRequest/{id}")]
        [SwaggerOperation(Summary = "Delete enrollment request", Description = "Delete an enrollment request by ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success) return NotFound(message);
            return Ok(new { Message = message });
        }
    }
}
