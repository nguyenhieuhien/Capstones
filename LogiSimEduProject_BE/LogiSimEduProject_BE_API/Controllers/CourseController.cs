using Azure.Core;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using LogiSimEduProject_BE_API.Controllers.DTO.Answer;
using LogiSimEduProject_BE_API.Controllers.DTO.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/course")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _service;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        public CourseController(ICourseService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _service = service;
            _cloudinary = cloudinary;
        }

        [HttpGet("get_all_course")]
        [SwaggerOperation(Summary = "Get all courses", Description = "Retrieve a list of all available courses.")]
        public async Task<IEnumerable<Course>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_course/{id}")]
        [SwaggerOperation(Summary = "Get course by ID", Description = "Get detailed information about a course by its ID.")]
        public async Task<Course> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1, 2")]
        //[HttpGet("SearchCourse/{name},{description}")]
        //public async Task<IEnumerable<Course>> Get(string name, string description)
        //{
        //    return await _service.Search(name, description);
        //}

        
        [HttpPost("create_course")]
        [SwaggerOperation(Summary = "Create new course", Description = "Create a new course with optional image upload.")]
        public async Task<IActionResult> Post([FromForm] CourseDTOCreate request)
        {
            string imgUrl = null;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Courses",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode != HttpStatusCode.OK)
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);

                imgUrl = uploadResult.SecureUrl.ToString();
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
        [HttpPut("update_course/{id}")]
        [SwaggerOperation(Summary = "Update course", Description = "Update an existing course and replace image if provided.")]
        public async Task<IActionResult> Put(string id, [FromForm] CourseDTOUpdate request)
        {
            var existingCourse = await _service.GetById(id);
            if (existingCourse == null)
                return NotFound(new { Message = $"Course with ID {id} was not found." });

            string imgUrl = existingCourse.ImgUrl;

            if (request.ImgUrl != null)
            {
                await using var stream = request.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImgUrl.FileName, stream),
                    Folder = "LogiSimEdu_Courses",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode != HttpStatusCode.OK)
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);

                imgUrl = uploadResult.SecureUrl.ToString();
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
        [HttpDelete("delete_course/{id}")]
        [SwaggerOperation(Summary = "Delete course", Description = "Delete a course by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
