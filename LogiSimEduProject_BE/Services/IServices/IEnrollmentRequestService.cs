using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IEnrollmentRequestService
    {
        Task<List<AccountOfCourse>> GetAll();
        Task<AccountOfCourse?> GetById(string id);
        Task<List<AccountOfCourse>> GetByCourseId(string courseId);
        Task<List<Account>> GetStudentsInClass(Guid classId);
        Task<List<Course>> GetEnrolledCoursesByAccountId(Guid accountId);
        Task<List<Account>> GetEnrolledStudentsWithoutClass(Guid courseId);
        Task<string> CheckEnrollmentStatusAsync(Guid accountId, Guid courseId);
        Task<List<Course>> GetPendingCoursesByAccountId(Guid accountId);
        Task<(bool Success, string Message)> AssignStudentToClass(Guid AccountOfCourseId, Guid classId);
        Task<(bool Success, string Message, Guid? Id)> Create(AccountOfCourse request);
        Task<(bool Success, string Message)> Update(AccountOfCourse request);
        Task<(bool Success, string Message)> Delete(string id);
    }

}
