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
        [HttpGet("courses/students")]
        public async Task<ActionResult<ChartSeriesDto>> GetStudentsPerCourse([FromQuery] int[]? statuses)
        {
            var rows = await _svc.GetStudentCountsPerCourseAsync(statuses);
            var chart = new ChartSeriesDto
            {
                Labels = rows.Select(r => r.CourseName).ToList(),
                Data = rows.Select(r => r.StudentCount).ToList()
            };
            return Ok(chart);
        }

        /// <summary>
        /// Số student theo từng class trong 1 course (chart-ready)
        /// GET /api/dashboard/courses/{courseId}/classes/students?statuses=1&statuses=2
        /// </summary>
        [HttpGet("courses/{courseId:guid}/classes/students")]
        public async Task<ActionResult<ChartSeriesDto>> GetStudentsPerClass(Guid courseId, [FromQuery] int[]? statuses)
        {
            var rows = await _svc.GetStudentCountsPerClassAsync(courseId, statuses);
            var chart = new ChartSeriesDto
            {
                Labels = rows.Select(r => r.ClassName).ToList(),
                Data = rows.Select(r => r.StudentCount).ToList()
            };
            return Ok(chart);
        }

        /// <summary>
        /// Tổng hợp dashboard (tổng quan + per-course + per-class + chart data)
        /// GET /api/dashboard?courseId={optional}&topN=10&statuses=1&statuses=2
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<DashboardCourseAndClassSummaryDTO>> GetDashboard([FromQuery] Guid? courseId, [FromQuery] int[]? statuses, [FromQuery] int? topN)
        {
            var summary = await _svc.GetDashboardAsync(courseId, statuses, topN);
            return Ok(summary);
        }
    }
}
