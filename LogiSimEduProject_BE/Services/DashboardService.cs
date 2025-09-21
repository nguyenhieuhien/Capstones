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

        public async Task<List<CourseStudentCountDto>> GetStudentCountsPerCourseByInstructorAsync(
        Guid instructorId, int[]? statuses = null)
        {
            var q = _ctx.EnrollmentRequests
            .AsNoTracking()
            .Where(aoc =>
                (aoc.IsActive ?? false) &&
                aoc.AccountId != null &&
                aoc.CourseId != null &&
                aoc.Course != null && (aoc.Course.IsActive ?? false) &&
                aoc.Course.InstructorId == instructorId);

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

        public async Task<List<ClassStudentCountDto>> GetStudentCountsPerClassByInstructorAsync(Guid instructorId, Guid? courseId = null, int[]? statuses = null, bool classScopeByClassInstructor = true, bool includeEmptyClasses = true)
        {
            if (includeEmptyClasses)
            {
                // Xuất phát từ Classes để không bỏ sót lớp chưa có enroll
                var classes = _ctx.Classes
                    .AsNoTracking()
                    .Where(cls => (cls.IsActive ?? false));

                classes = classScopeByClassInstructor
                    ? classes.Where(cls => cls.InstructorId == instructorId)                    // class do chính instructor dạy
                    : classes.Where(cls => cls.Course != null && cls.Course.InstructorId == instructorId); // lớp thuộc course của instructor

                if (courseId.HasValue)
                    classes = classes.Where(cls => cls.CourseId == courseId.Value);

                var data = await classes
                    .Select(cls => new ClassStudentCountDto
                    {
                        ClassId = cls.Id,
                        ClassName = cls.ClassName,
                        // Nếu chắc chắn không null thì dùng cls.CourseId!.Value
                        CourseId = cls.CourseId ?? Guid.Empty,
                        CourseName = cls.Course != null ? cls.Course.CourseName : string.Empty,
                        StudentCount = _ctx.EnrollmentRequests
                            .Where(aoc =>
                                (aoc.IsActive ?? false) &&
                                aoc.ClassId == cls.Id &&
                                aoc.AccountId != null &&
                                (statuses == null || statuses.Length == 0 || (aoc.Status != null && statuses.Contains(aoc.Status.Value))))
                            .Select(aoc => aoc.AccountId!.Value)
                            .Distinct()
                            .Count()
                    })
                    .OrderByDescending(x => x.StudentCount)
                    .ToListAsync();

                return data;
            }
            else
            {
                // Giữ nguyên logic cũ (bắt đầu từ AccountOfCourses) — chỉ trả lớp có ít nhất 1 enroll
                var q = _ctx.EnrollmentRequests
                    .AsNoTracking()
                    .Where(aoc =>
                        (aoc.IsActive ?? false) &&
                        aoc.AccountId != null &&
                        aoc.ClassId != null &&
                        aoc.Class != null && (aoc.Class.IsActive ?? false) &&
                        aoc.Course != null && (aoc.Course.IsActive ?? false));

                if (classScopeByClassInstructor)
                {
                    q = q.Where(aoc => aoc.Class!.InstructorId == instructorId);
                }
                else
                {
                    // Nếu muốn loại các class chưa gán instructor ở nhánh course-owner:
                    q = q.Where(aoc => aoc.Course!.InstructorId == instructorId &&
                                       aoc.Class!.InstructorId != null);
                }

                if (courseId.HasValue)
                    q = q.Where(aoc => aoc.CourseId == courseId.Value);

                if (statuses != null && statuses.Length > 0)
                    q = q.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

                var data = await q
                    .GroupBy(aoc => new
                    {
                        aoc.ClassId,
                        aoc.Class!.ClassName,
                        aoc.CourseId,
                        CourseName = aoc.Course!.CourseName
                    })
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
        }

        public async Task<DashboardCourseAndClassSummaryDTO> GetDashboardByInstructorAsync(Guid instructorId, Guid? courseId = null, int[]? statuses = null, int? topN = null,
        bool classScopeByClassInstructor = false)
        {
            // Per-course
            var perCourse = await GetStudentCountsPerCourseByInstructorAsync(instructorId, statuses);
            if (courseId.HasValue)
                perCourse = perCourse.Where(x => x.CourseId == courseId.Value).ToList();

            // Per-class
            var perClass = await GetStudentCountsPerClassByInstructorAsync(
                instructorId, courseId, statuses, classScopeByClassInstructor);

            if (topN.HasValue && topN.Value > 0)
            {
                perCourse = perCourse.Take(topN.Value).ToList();
                perClass = perClass.Take(topN.Value).ToList();
            }

            // Tổng distinct students trong phạm vi instructor
            var qDistinct = _ctx.EnrollmentRequests
                .AsNoTracking()
                .Where(aoc =>
                    (aoc.IsActive ?? false) &&
                    aoc.AccountId != null &&
                    aoc.Course != null && (aoc.Course.IsActive ?? false) &&
                    aoc.Course.InstructorId == instructorId);

            if (statuses != null && statuses.Length > 0)
                qDistinct = qDistinct.Where(aoc => aoc.Status != null && statuses.Contains(aoc.Status.Value));

            var totalStudentsDistinct = await qDistinct
                .Select(aoc => aoc.AccountId!.Value)
                .Distinct()
                .CountAsync();

            // Tổng courses/classes của instructor
            var totalCourses = await _ctx.Courses
                .CountAsync(c => (c.IsActive ?? false) && c.InstructorId == instructorId);

            var totalClassesQuery = _ctx.Classes
                .Where(cls => (cls.IsActive ?? false));

            totalClassesQuery = classScopeByClassInstructor
                ? totalClassesQuery.Where(cls => cls.InstructorId == instructorId)
                : totalClassesQuery.Where(cls => cls.Course != null && cls.Course.InstructorId == instructorId);

            var totalClasses = await totalClassesQuery.CountAsync();

            // Chart series
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
                TotalCourses = totalCourses,
                TotalClasses = totalClasses,
                TotalStudentsDistinct = totalStudentsDistinct,
                PerCourse = perCourse,
                PerClass = perClass,
                CourseChart = courseChart,
                ClassChart = classChart
            };
        }
    }
}
