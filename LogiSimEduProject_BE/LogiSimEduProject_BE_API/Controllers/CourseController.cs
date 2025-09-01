using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Repositories.Models;
using Services;
using Services.DTO.Course;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/course")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CourseController(ICourseService courseService, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _courseService = courseService;
            _cloudinary = cloudinary;
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all courses", Description = "Retrieve all courses.")]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseService.GetAll();
            return Ok(courses);
        }

        [HttpGet("my_course/{instructorId}")]
        [SwaggerOperation(Summary = "Get all courses by instructor", Description = "Retrieve all courses.")]
        public async Task<IActionResult> GetCoursesByInstructor(Guid instructorId)
        {
            var courses = await _courseService.GetCoursesByInstructorId(instructorId);
            return Ok(courses);
        }

        [HttpGet("courses/{categoryId}")]
        [SwaggerOperation(Summary = "Get all courses by category", Description = "Retrieve all courses.")]
        public async Task<IActionResult> GetCoursesByCategory(Guid categoryId)
        {
            var courses = await _courseService.GetCoursesByCategoryId(categoryId);
            return Ok(courses);
        }

        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_by_id/{id}")]
        [SwaggerOperation(Summary = "Get course by ID", Description = "Retrieve a course by ID.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var course = await _courseService.GetById(id);
            return course != null ? Ok(course) : NotFound("Course not found.");
        }

        [HttpGet("{courseId}/instructor-name")]
        public async Task<IActionResult> GetInstructorName(Guid courseId)
        {
            try
            {
                var name = await _courseService.GetInstructorFullNameAsync(courseId);
                return Ok(name);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("get_all_course_by_workspace/{workspaceId}")]
        [SwaggerOperation(Summary = "Get all courses by workspace ID", Description = "Retrieve all courses that belong to a specific workspace.")]
        public async Task<IActionResult> GetAllCourseByWorkspaceId(Guid workspaceId)
        {
            var courses = await _courseService.GetAllByWorkspaceId(workspaceId);
            return Ok(courses);
        }

        [HttpGet("get_all_course_by_category/{categoryId}")]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId)
        {
            var courses = await _courseService.GetAllByCategoryId(categoryId);
            return Ok(courses);
        }


        //[Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_by_org/{orgId}")]
        [SwaggerOperation(Summary = "Get all courses by organization ID", Description = "Retrieve all courses that belong to a specific organization.")]
        public async Task<IActionResult> GetAllByOrgId(Guid orgId)
        {
            var courses = await _courseService.GetAllByOrgId(orgId);
            return Ok(courses);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "Create a course", Description = "Create a new course with optional image.")]
        public async Task<IActionResult> Create([FromForm] CourseDTOCreate dto)
        {
            string? imgUrl = null;

            if (dto.ImgUrl != null)
            {
                using var stream = dto.ImgUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(dto.ImgUrl.FileName, stream),
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
                CategoryId = dto.CategoryId,
                WorkSpaceId = dto.WorkSpaceId,
                InstructorId = dto.InstructorId,
                CourseName = dto.CourseName,
                Description = dto.Description,
                RatingAverage = dto.RatingAverage,
                ImgUrl = imgUrl
            };

            var (success, message, id) = await _courseService.Create(course);
            return success ? Ok(new { Message = message, CourseId = id }) : BadRequest(message);
        }

        //[Authorize(Roles = "Instructor")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromForm] CourseDTOUpdate request)
        {
            Course? existing = await _courseService.GetById(id);
            if (existing == null) return NotFound("Không tìm thấy khóa học.");

            if (request.CourseName != null) existing.CourseName = request.CourseName;
            if (request.Description != null) existing.Description = request.Description;
            if (request.RatingAverage.HasValue) existing.RatingAverage = request.RatingAverage.Value;
            if (request.CategoryId.HasValue) existing.CategoryId = request.CategoryId.Value;
            if (request.WorkSpaceId.HasValue) existing.WorkSpaceId = request.WorkSpaceId.Value;
            if (request.InstructorId.HasValue) existing.InstructorId = request.InstructorId.Value;

            // Chỉ upload file ảnh
            if (request.ImgUrl != null)
            {
                if (!request.ImgUrl.ContentType.StartsWith("image/"))
                    return BadRequest(new { success = false, message = "File ảnh không hợp lệ." });

                using var stream = request.ImgUrl.OpenReadStream();
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
                    return StatusCode((int)uploadResult.StatusCode, new { success = false, message = uploadResult.Error?.Message });

                existing.ImgUrl = uploadResult.SecureUrl.ToString();
            }

            var (success, message) = await _courseService.Update(existing);
            return success
                ? Ok(new { success = true, message })
                : BadRequest(new { success = false, message });
        }




        //[Authorize(Roles = "Instructor")]
        [HttpDelete("delete/{id}")]
        [SwaggerOperation(Summary = "Delete course", Description = "Delete a course by ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _courseService.Delete(id);
            return success ? Ok(new { Message = message }) : NotFound(message);
        }

       
      
    }
}
