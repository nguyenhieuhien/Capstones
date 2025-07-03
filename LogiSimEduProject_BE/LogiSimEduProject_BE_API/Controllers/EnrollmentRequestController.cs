using LogiSimEduProject_BE_API.Controllers.DTO.EnrollmentRequestment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("GetAllEnrollmentRequest")]
        public async Task<IEnumerable<EnrollmentRequest>> GetAll()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetEnrollmentRequestBy/{courseId}")] // Lấy tất cả yêu cầu theo course
        public async Task<IEnumerable<EnrollmentRequest>> GetByCourse(string courseId)
        {
            return await _service.GetByCourseId(courseId);
        }

        [HttpGet("GetEnrollmentRequest/{id}")]
        public async Task<ActionResult<EnrollmentRequest>> Get(string id)
        {
            var item = await _service.GetById(id);
            if (item == null)
                return NotFound();
            return item;
        }

        [Authorize(Roles = "Student")]
        [HttpPost("CreateEnrollmentRequest")] // Student gửi yêu cầu
        public async Task<IActionResult> Post([FromBody] EnrollmentRequestDTOCreate request)
        {
            // Kiểm tra tài khoản tồn tại
            var account = await _accountService.GetById(request.StudentId.ToString());
            if (account == null)
                return BadRequest("Tài khoản không tồn tại.");

            var roleName = account.Role?.RoleName?.ToLower();
            if (roleName != "student")
                return Unauthorized("Chỉ có tài khoản Student mới được gửi yêu cầu đăng ký.");

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
        [HttpPut("UpdateEnrollmentRequest/{id}")] // Instructor duyệt hoặc từ chối yêu cầu
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

        [HttpDelete("DeleteEnrollmentRequest/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
