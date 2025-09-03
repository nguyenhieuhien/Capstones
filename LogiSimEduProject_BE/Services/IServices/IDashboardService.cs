using Services.DTO.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IDashboardService
    {
        Task<List<CourseStudentCountDto>> GetStudentCountsPerCourseByInstructorAsync(Guid instructorId, int[]? statuses = null);
        Task<List<ClassStudentCountDto>> GetStudentCountsPerClassByInstructorAsync(Guid instructorId, Guid? courseId = null, int[]? statuses = null,
        bool classScopeByClassInstructor = true, bool includeEmptyClasses = true);
        Task<DashboardCourseAndClassSummaryDTO> GetDashboardByInstructorAsync(Guid instructorId, Guid? courseId = null, int[]? statuses = null, int? topN = null,
        bool classScopeByClassInstructor = false);
    }
}
