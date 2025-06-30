using Google.Apis.Util;
using LogiSimEduProject_BE_API.Controllers.DTO.Class;
using LogiSimEduProject_BE_API.Controllers.DTO.Course;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _service;

        public ClassController(IClassService service) => _service = service;

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Class>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Class> Get(string id)
        {
            return await _service.GetById(id);
        }


        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(ClassDTOCreate request)
        {
            var course = new Class
            {
                CourseId = request.CourseId,
                ClassName = request.ClassName,
                NumberOfStudent = request.NumberOfStudent,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(course);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, ClassDTOUpdate request)
        {
            var existingClass = await _service.GetById(id);
            if (existingClass == null)
            {
                return NotFound(new { Message = $"Class with ID {id} was not found." });
            }

            existingClass.CourseId = request.CourseId;
            existingClass.ClassName = request.ClassName;
            existingClass.NumberOfStudent = request.NumberOfStudent;
            existingClass.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingClass);

            return Ok(new
            {
                Message = "Class updated successfully.",
                Data = new
                {
                    CourseId = existingClass.CourseId,
                    ClassName = existingClass.ClassName,
                    NumberOfStudent = existingClass.NumberOfStudent,
                    IsActive = existingClass.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
