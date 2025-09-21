using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Repositories.Models;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IEnrollmentRequestService
    {
        Task<List<EnrollmentRequest>> GetAll();
        Task<EnrollmentRequest?> GetById(string id);
        Task<List<EnrollmentRequest>> GetByCourseId(string courseId);
        Task<List<Account>> GetStudentsInClass(Guid classId);
        Task<List<Course>> GetEnrolledCoursesByAccountId(Guid accountId);
        Task<List<EnrollmentRequest>> GetEnrolledStudentsWithoutClass(Guid courseId);
        Task<List<EnrollmentRequest>> GetPendingStudents(Guid courseId);
        Task<int> CheckEnrollmentStatusAsync(Guid accountId, Guid courseId);
        Task<List<Course>> GetPendingCoursesByAccountId(Guid accountId);
        Task<(bool Success, string Message)> AssignStudentToClass(Guid AccountOfCourseId, Guid classId);
        Task<(bool Success, string Message, Guid? Id)> Create(EnrollmentRequest request);
        Task<(bool Success, string Message)> Update(EnrollmentRequest request);
        Task<(bool Success, string Message)> UpdateAccept(EnrollmentRequest request);
        Task<(bool Success, string Message)> Delete(string id);
    }

}
