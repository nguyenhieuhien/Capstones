using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Services.DTO.Dashboard;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class DashboardService : IDashboardService
    {
        private readonly LogiSimEduContext _ctx;
        public DashboardService(LogiSimEduContext ctx) => _ctx = ctx;

        public async Task<List<CourseStudentCountDto>> GetStudentCountsPerCourseAsync(int[]? statuses = null)
        {
            var q = _ctx.AccountOfCourses
                .AsNoTracking()
                .Where(aoc =>
                    (aoc.IsActive ?? false) &&
                    aoc.CourseId != null &&
                    aoc.AccountId != null &&
                    aoc.Course != null &&
                    (aoc.Course.IsActive ?? false));

            if (statuses != null && statuses.Length > 0)
                q = q.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

            var data = await q
                .GroupBy(aoc => new { aoc.CourseId, aoc.Course.CourseName })
                .Select(g => new CourseStudentCountDto
                {
                    CourseId = g.Key.CourseId!.Value,
                    CourseName = g.Key.CourseName,
                    StudentCount = g.Select(x => x.AccountId!.Value).Distinct().Count()
                })
                .OrderByDescending(x => x.StudentCount)
                .ToListAsync();

            return data;
        }

        public async Task<List<ClassStudentCountDto>> GetStudentCountsPerClassAsync(Guid courseId, int[]? statuses = null)
        {
            var q = _ctx.AccountOfCourses
                .AsNoTracking()
                .Where(aoc =>
                    (aoc.IsActive ?? false) &&
                    aoc.CourseId == courseId &&
                    aoc.ClassId != null &&
                    aoc.AccountId != null &&
                    aoc.Class != null &&
                    (aoc.Class.IsActive ?? false));

            if (statuses != null && statuses.Length > 0)
                q = q.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

            var data = await q
                .GroupBy(aoc => new { aoc.ClassId, aoc.Class.ClassName, aoc.CourseId, CourseName = aoc.Course.CourseName })
                .Select(g => new ClassStudentCountDto
                {
                    ClassId = g.Key.ClassId!.Value,
                    ClassName = g.Key.ClassName,
                    CourseId = g.Key.CourseId!.Value,
                    CourseName = g.Key.CourseName,
                    StudentCount = g.Select(x => x.AccountId!.Value).Distinct().Count()
                })
                .OrderByDescending(x => x.StudentCount)
                .ToListAsync();

            return data;
        }

        public async Task<DashboardCourseAndClassSummaryDTO> GetDashboardAsync(Guid? courseId = null, int[]? statuses = null, int? topN = null)
        {
            // Per-course
            var perCourse = await GetStudentCountsPerCourseAsync(statuses);
            if (courseId.HasValue)
                perCourse = perCourse.Where(x => x.CourseId == courseId.Value).ToList();

            // Per-class (nếu chỉ định courseId thì trả theo course đó; nếu không chỉ định, trả top theo tất cả)
            List<ClassStudentCountDto> perClass;
            if (courseId.HasValue)
            {
                perClass = await GetStudentCountsPerClassAsync(courseId.Value, statuses);
            }
            else
            {
                // lấy tất cả class
                var q = _ctx.AccountOfCourses
                    .AsNoTracking()
                    .Where(aoc =>
                        (aoc.IsActive ?? false) &&
                        aoc.ClassId != null &&
                        aoc.AccountId != null &&
                        aoc.Class != null && (aoc.Class.IsActive ?? false) &&
                        aoc.Course != null && (aoc.Course.IsActive ?? false));

                if (statuses != null && statuses.Length > 0)
                    q = q.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

                perClass = await q
                    .GroupBy(aoc => new { aoc.ClassId, aoc.Class.ClassName, aoc.CourseId, CourseName = aoc.Course.CourseName })
                    .Select(g => new ClassStudentCountDto
                    {
                        ClassId = g.Key.ClassId!.Value,
                        ClassName = g.Key.ClassName,
                        CourseId = g.Key.CourseId!.Value,
                        CourseName = g.Key.CourseName,
                        StudentCount = g.Select(x => x.AccountId!.Value).Distinct().Count()
                    })
                    .OrderByDescending(x => x.StudentCount)
                    .ToListAsync();
            }

            if (topN.HasValue && topN.Value > 0)
            {
                perCourse = perCourse.Take(topN.Value).ToList();
                perClass = perClass.Take(topN.Value).ToList();
            }

            // Tổng DISTINCT student toàn hệ thống (theo filter)
            var qDistinctStudents = _ctx.AccountOfCourses
                .AsNoTracking()
                .Where(aoc => (aoc.IsActive ?? false) && aoc.AccountId != null);

            if (statuses != null && statuses.Length > 0)
                qDistinctStudents = qDistinctStudents.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

            var totalStudentsDistinct = await qDistinctStudents
                .Select(aoc => aoc.AccountId!.Value)
                .Distinct()
                .CountAsync();

            // Build chart series
            var courseChart = new ChartSeriesDto
            {
                Labels = perCourse.Select(x => x.CourseName).ToList(),
                Data = perCourse.Select(x => x.StudentCount).ToList()
            };
            var classChart = new ChartSeriesDto
            {
                Labels = perClass.Select(x => $"{x.ClassName} ({x.CourseName})").ToList(),
                Data = perClass.Select(x => x.StudentCount).ToList()
            };

            return new DashboardCourseAndClassSummaryDTO
            {
                TotalCourses = await _ctx.Courses.CountAsync(c => (c.IsActive ?? false)),
                TotalClasses = await _ctx.Classes.CountAsync(c => (c.IsActive ?? false)),
                TotalStudentsDistinct = totalStudentsDistinct,
                PerCourse = perCourse,
                PerClass = perClass,
                CourseChart = courseChart,
                ClassChart = classChart
            };
        }
    }
}
