using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using LogiSimEduProject_BE_API.Controllers.DTO.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;

        public CourseController(ICourseService service) => _service = service;

        [HttpGet("GetAll")]
        public async Task<IEnumerable<Course>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Course> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1, 2")]
        [HttpGet("Search")]
        public async Task<IEnumerable<Course>> Get(string name, string description)
        {
            return await _service.Search(name, description);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Post(CourseDTOCreate request)
        {
            var course = new Course
            {
                CategoryId = request.CategoryId,
                WorkSpaceId = request.WorkSpaceId,
                CourseName = request.CourseName,
                Description = request.Description,
                RatingAverage = request.RatingAverage,
                ImgUrl = request.ImgUrl,
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
        public async Task<IActionResult> Put(string id, CourseDTOUpdate request)
        {
            var existingCourse = await _service.GetById(id);
            if (existingCourse == null)
            {
                return NotFound(new { Message = $"Course with ID {id} was not found." });
            }

            existingCourse.CategoryId = request.CategoryId;
            existingCourse.WorkSpaceId = request.WorkSpaceId;
            existingCourse.CourseName = request.CourseName;
            existingCourse.Description = request.Description;
            existingCourse.RatingAverage = request.RatingAverage;
            existingCourse.ImgUrl = request.ImgUrl;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingCourse);

            return Ok(new
            {
                Message = "Course updated successfully.",
                Data = new
                {
                    CategoryId = existingCourse.CategoryId,
                    WorkSpaceId = existingCourse.WorkSpaceId,
                    CourseName = existingCourse.CourseName,
                    Description = existingCourse.Description,
                    RatingAverage = existingCourse.RatingAverage,
                    ImgUrl = existingCourse.ImgUrl,
                    IsActive = existingCourse.IsActive,
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
