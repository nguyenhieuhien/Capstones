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
        private readonly IWebHostEnvironment _env;
        private readonly ICourseService _service;

        public CourseController(ICourseService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

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
        public async Task<IActionResult> Post([FromForm] CourseDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads/topics");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                imgUrl = $"/uploads/topics/{fileName}";
            }

            var course = new Course
            {
                CategoryId = request.CategoryId,
                WorkSpaceId = request.WorkSpaceId,
                CourseName = request.CourseName,
                Description = request.Description,
                RatingAverage = request.RatingAverage,
                ImgUrl = imgUrl,
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
            string imgUrl = existingCourse.ImgUrl;

            if (request.ImgUrl != null)
            {
                // Nếu có file ảnh mới thì lưu ảnh mới
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads/courses");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{request.ImgUrl.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImgUrl.CopyToAsync(stream);
                }

                // Nếu muốn xóa ảnh cũ khỏi ổ đĩa, có thể thêm đoạn sau:
                if (!string.IsNullOrEmpty(existingCourse.ImgUrl))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, existingCourse.ImgUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                imgUrl = $"/uploads/courses/{fileName}";
            }

            existingCourse.CategoryId = request.CategoryId;
            existingCourse.WorkSpaceId = request.WorkSpaceId;
            existingCourse.CourseName = request.CourseName;
            existingCourse.Description = request.Description;
            existingCourse.RatingAverage = request.RatingAverage;
            existingCourse.ImgUrl = imgUrl;
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
