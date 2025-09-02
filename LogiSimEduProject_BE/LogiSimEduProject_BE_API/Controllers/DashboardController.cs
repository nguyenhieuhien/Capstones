using Microsoft.AspNetCore.Mvc;
using Services.DTO.Dashboard;
using Services.IServices;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {

        private readonly IDashboardService _svc;
        public DashboardController(IDashboardService svc) => _svc = svc;

        /// <summary>
        /// Số student theo từng course (chart-ready)
        /// GET /api/dashboard/courses/students?statuses=1&statuses=2
        /// </summary>
        [HttpGet("instructors/{instructorId:guid}/courses/students")]
        public async Task<ActionResult<ChartSeriesDto>> GetStudentsPerCourseByInstructor(
        Guid instructorId, [FromQuery] int[]? statuses)
        {
            var rows = await _svc.GetStudentCountsPerCourseByInstructorAsync(instructorId, statuses);
            return Ok(new ChartSeriesDto
            {
                Labels = rows.Select(r => r.CourseName).ToList(),
                Data = rows.Select(r => r.StudentCount).ToList()
            });
        }

        /// <summary>
        /// Số student theo từng class trong 1 course (chart-ready)
        /// GET /api/dashboard/courses/{courseId}/classes/students?statuses=1&statuses=2
        /// </summary>
        [HttpGet("instructors/{instructorId:guid}/classes/students")]
        public async Task<ActionResult<ChartSeriesDto>> GetStudentsPerClassByInstructor(
        Guid instructorId, [FromQuery] Guid? courseId, [FromQuery] int[]? statuses,
        [FromQuery] bool classScopeByClassInstructor = true, bool includeEmptyClasses = true)
        {
            var rows = await _svc.GetStudentCountsPerClassByInstructorAsync(
                instructorId, courseId, statuses, classScopeByClassInstructor, includeEmptyClasses);

            return Ok(new ChartSeriesDto
            {
                Labels = rows.Select(r => r.ClassName).ToList(),
                Data = rows.Select(r => r.StudentCount).ToList()
            });
        }

        /// <summary>
        /// Tổng hợp dashboard (tổng quan + per-course + per-class + chart data)
        /// GET /api/dashboard?courseId={optional}&topN=10&statuses=1&statuses=2
        /// </summary>
        [HttpGet("instructors/{instructorId:guid}")]
        public async Task<ActionResult<DashboardCourseAndClassSummaryDTO>> GetDashboardByInstructor(
        Guid instructorId, [FromQuery] Guid? courseId, [FromQuery] int[]? statuses,
        [FromQuery] int? topN, [FromQuery] bool classScopeByClassInstructor = false)
        {
            var summary = await _svc.GetDashboardByInstructorAsync(
                instructorId, courseId, statuses, topN, classScopeByClassInstructor);

            return Ok(summary);
        }
    }
}
