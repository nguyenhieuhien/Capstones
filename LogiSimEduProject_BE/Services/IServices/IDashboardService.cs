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
        Task<List<CourseStudentCountDto>> GetStudentCountsPerCourseAsync(int[]? statuses = null);
        Task<List<ClassStudentCountDto>> GetStudentCountsPerClassAsync(Guid courseId, int[]? statuses = null);
        Task<DashboardCourseAndClassSummaryDTO> GetDashboardAsync(Guid? courseId = null, int[]? statuses = null, int? topN = null);
    }
}
