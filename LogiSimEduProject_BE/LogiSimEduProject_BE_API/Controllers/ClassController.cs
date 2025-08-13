// File: Controllers/ClassController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.DTO.Class;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/class")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _service;

        public ClassController(IClassService service)
        {
            _service = service;
        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_all_class")]
        [SwaggerOperation(Summary = "Get all classes", Description = "Return a list of all classes.")]
        public async Task<IActionResult> GetAll()
        {
            var classes = await _service.GetAll();
            return Ok(classes);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpGet("get_class/{id}")]
        [SwaggerOperation(Summary = "Get class by ID", Description = "Return class details by its ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var _class = await _service.GetById(id);
            if (_class == null)
                return NotFound("Class not found");

            return Ok(_class);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_class")]
        [SwaggerOperation(Summary = "Create a new class", Description = "Create a new class with course ID, name, and number of students.")]
        public async Task<IActionResult> Create([FromBody] ClassDTOCreate request)
        {
            if (request == null)
                return BadRequest("Invalid request data");

            var newClass = new Class
            {
                CourseId = request.CourseId,
                ClassName = request.ClassName,
                NumberOfStudent = request.NumberOfStudent
            };

            var (success, message, id) = await _service.Create(newClass);
            if (!success)
                return BadRequest(message);

            return Ok(new { Id = id, Message = message });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_class/{id}")]
        [SwaggerOperation(Summary = "Update class", Description = "Update class details.")]
        public async Task<IActionResult> Update(string id, [FromBody] ClassDTOUpdate request)
        {
            var existingClass = await _service.GetById(id);
            if (existingClass == null)
                return NotFound("Class not found");

            existingClass.CourseId = request.CourseId;
            existingClass.ClassName = request.ClassName;
            existingClass.NumberOfStudent = request.NumberOfStudent;

            var (success, message) = await _service.Update(existingClass);
            if (!success)
                return BadRequest(message);

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_class/{id}")]
        [SwaggerOperation(Summary = "Delete class", Description = "Delete a class by ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _service.Delete(id);
            if (!success)
                return NotFound(message);

            return Ok(new { Message = message });
        }
    }
}
