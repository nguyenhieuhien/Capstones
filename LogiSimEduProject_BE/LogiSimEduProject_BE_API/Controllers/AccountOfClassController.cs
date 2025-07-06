using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfClass;
using LogiSimEduProject_BE_API.Controllers.DTO.AccountOfWorkSpace;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/account_class")]
    [ApiController]
    public class AccountOfClassController : ControllerBase
    {
        private readonly IAccountOfClassService _service;
        public AccountOfClassController(IAccountOfClassService service) => _service = service;
        [HttpGet("GetAllAccountOfClass")]
        [SwaggerOperation(Summary = "Get all account-class relations", Description = "Retrieve all account-class relationship records")]
        public async Task<IEnumerable<AccountOfClass>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_account0fClass/{id}")]
        [SwaggerOperation(Summary = "Get account-class relation by ID", Description = "Retrieve a specific account-class relationship by its ID")]
        public async Task<AccountOfClass> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_account0fClass")]
        [SwaggerOperation(Summary = "Create account-class relation", Description = "Assign an account to a class by creating a new relation")]
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

        [HttpPost("assign_student_to_class/{classId},{studentId}")]
        [SwaggerOperation(Summary = "Assign student to class", Description = "Assign a student to a class by their IDs")]
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
        [HttpPut("update_accountOfClass/{id}")]
        [SwaggerOperation(Summary = "Update account-class relation", Description = "Update an existing account-class relationship by ID")]
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

        [HttpDelete("remove_student/{classId},{studentId}")]
        [SwaggerOperation(Summary = "Remove student from class", Description = "Remove a student from a class by their IDs")]
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
