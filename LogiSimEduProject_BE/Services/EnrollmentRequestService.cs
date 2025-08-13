// File: Services/IEnrollmentRequestService.cs
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    
    public class EnrollmentRequestService : IEnrollmentRequestService
    {
        private readonly EnrollmentRequestRepository _repository;
        private readonly LogiSimEduContext _dbContext;
        private readonly LessonRepository _lessonRepo;

        public EnrollmentRequestService(EnrollmentRequestRepository repository, LogiSimEduContext dbContext, LessonRepository lessonRepo)
        {
            _repository = repository;
            _dbContext = dbContext;
            _lessonRepo = lessonRepo;
        }

        public async Task<List<AccountOfCourse>> GetAll()
        {
            return await _repository.GetAll() ?? new List<AccountOfCourse>();
        }

        public async Task<AccountOfCourse?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetById(id);
        }

        public async Task<List<AccountOfCourse>> GetByCourseId(string courseId)
        {
            return await _repository.GetByCourseId(courseId);
        }

        public async Task<List<Account>> GetStudentsInClass(Guid classId)
        {
            return await _repository.GetStudentsByClassId(classId);
        }

        public async Task<List<Account>> GetEnrolledStudentsWithoutClass(Guid courseId)
        {
            return await _repository.GetEnrolledStudentsWithoutClass(courseId);
        }

        public async Task<List<Course>> GetEnrolledCoursesByAccountId(Guid accountId)
        {
            return await _repository.GetEnrolledCoursesByAccountId(accountId);
        }

        public async Task<List<Course>> GetPendingCoursesByAccountId(Guid accountId)
        {
            return await _repository.GetPendingCoursesByAccountId(accountId);
        }

        public async Task<int> CheckEnrollmentStatusAsync(Guid accountId, Guid courseId)
        {
            var status = await _repository.GetEnrollmentStatusAsync(accountId, courseId);

            // Nếu không tìm thấy thì trả về -1
            if (status == null)
                return -1;

            // Đảm bảo chỉ trả về 0, 1, 2
            return status.Value;
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(AccountOfCourse request)
        {
            if (request == null || request.AccountId == Guid.Empty || request.CourseId == Guid.Empty)
                return (false, "Invalid request data", null);

            // Kiểm tra xem đã tồn tại bản ghi giống chưa
            var existing = await _repository.GetActiveByAccountAndCourseAsync(request.AccountId.Value, request.CourseId.Value);

            if (existing != null)
                return (false, "Học viên đã gửi yêu cầu hoặc đang theo học khóa học này", null);

            request.Id = Guid.NewGuid();
            request.Status = 0;
            request.IsActive = true;
            request.CreatedAt = DateTime.UtcNow;

            var result = await _repository.CreateAsync(request);
            return result > 0
                ? (true, "Enrollment request created", request.Id)
                : (false, "Failed to create enrollment request", null);
        }

        public async Task<(bool Success, string Message)> AssignStudentToClass(Guid accountOfCourseId, Guid classId)
        {
            var record = await _repository.GetByAccountAndCourse(accountOfCourseId);

            if (record == null)
                return (false, "Không tìm thấy học viên đủ điều kiện");

            record.ClassId = classId;
            record.UpdatedAt = DateTime.UtcNow;

            // 🔍 Check CourseProgress đã tồn tại chưa
            var existsCourseProgress = await _dbContext.CourseProgresses
                .AnyAsync(cp => cp.AccountId == record.AccountId && cp.CourseId == record.CourseId);


            if (!existsCourseProgress)
            {
                var courseProgress = new CourseProgress
                {
                    Id = Guid.NewGuid(),
                    AccountId = record.AccountId,
                    CourseId = record.CourseId,
                    ProgressPercent = 0,
                    Status = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _dbContext.CourseProgresses.AddAsync(courseProgress);
            }

            // 🔍 Check LessonProgress cho từng lesson
            var lessons = await _lessonRepo.GetLessonsByCourseId(record.CourseId.Value);

            foreach (var lesson in lessons)
            {
                bool existsLessonProgress = await _dbContext.LessonProgresses
                    .AnyAsync(lp => lp.AccountId == record.AccountId && lp.LessonId == lesson.Id);

                if (!existsLessonProgress)
                {
                    var lessonProgress = new LessonProgress
                    {
                        Id = Guid.NewGuid(),
                        AccountId = record.AccountId,
                        LessonId = lesson.Id,
                        Status = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _dbContext.LessonProgresses.AddAsync(lessonProgress);
                }
            }

            //// 🔹 Tăng số lượng học viên trong Class
            //var classEntity = await _dbContext.Classes.FindAsync(classId);
            //if (classEntity != null)
            //{
            //    classEntity.NumberOfStudent = (classEntity.NumberOfStudent ?? 0) + 1;
            //    classEntity.UpdatedAt = DateTime.UtcNow;
            //    _dbContext.Classes.Update(classEntity);
            //}


            _dbContext.AccountOfCourses.Update(record);
            await _dbContext.SaveChangesAsync();

            return (true, "Đã gán học viên vào lớp và khởi tạo tiến độ thành công.");
        }

        public async Task<(bool Success, string Message)> Update(AccountOfCourse request)
        {
            if (request == null || request.Id == Guid.Empty)
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(request);
            return result > 0 ? (true, "Request updated successfully") : (false, "Failed to update request");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetById(id);
            if (item == null)
                return (false, "Request not found");

            item.IsActive = false;
            item.DeleteAt = DateTime.UtcNow;

            await _repository.UpdateAsync(item);
            return (true, "Deleted successfully");
        }
    }
}
