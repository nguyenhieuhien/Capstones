using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfClass;
using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountOfClassController : ControllerBase
    {
        private readonly IAccountOfClassService _service;
        public AccountOfClassController(IAccountOfClassService service) => _service = service;
        [HttpGet("GetAllAccountOfClass")]
        public async Task<IEnumerable<AccountOfClass>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetAccountOfClass/{id}")]
        public async Task<AccountOfClass> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateAccountOfClass")]
        public async Task<IActionResult> Post(AccountOfClassDTOCreate request)
        {
            var answer = new AccountOfClass
            {
                AccountId = request.AccountId,
                ClassId = request.ClassId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(answer);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        [HttpPost("AssignStudentToClass/{classId},{studentId}")]
        public async Task<IActionResult> AssignStudentToClass(Guid classId, Guid studentId)
        {
            try
            {
                var result = await _service.AddStudentToClass(classId, studentId);
                return Ok(new { Message = "Thêm học sinh vào lớp thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdateAccountOfClass/{id}")]
        public async Task<IActionResult> Put(string id, AccountOfClassDTOUpdate request)
        {
            var existingAcCl = await _service.GetById(id);
            if (existingAcCl == null)
            {
                return NotFound(new { Message = $"AccountOfClass with ID {id} was not found." });
            }

            existingAcCl.AccountId = request.AccountId;
            existingAcCl.ClassId = request.ClassId;
            existingAcCl.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingAcCl);

            return Ok(new
            {
                Message = "AccountOfClass updated successfully.",
                Data = new
                {
                    AccountId = existingAcCl.AccountId,
                    ClassId = existingAcCl.ClassId,
                    IsActive = existingAcCl.IsActive,
                }
            });
        }

        [HttpDelete("RemoveStudent/{classId},{studentId}")]
        public async Task<IActionResult> RemoveStudent(Guid classId, Guid studentId)
        {
            try
            {
                var result = await _service.RemoveStudentFromClass(classId, studentId);
                return Ok(new { Message = "Đã xoá học sinh khỏi lớp." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
