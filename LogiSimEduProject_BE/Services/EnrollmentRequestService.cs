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

        public async Task<List<EnrollmentRequest>> GetAll()
        {
            return await _repository.GetAll() ?? new List<EnrollmentRequest>();
        }

        public async Task<EnrollmentRequest?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetById(id);
        }

        public async Task<List<EnrollmentRequest>> GetByCourseId(string courseId)
        {
            return await _repository.GetByCourseId(courseId);
        }

        public async Task<List<Account>> GetStudentsInClass(Guid classId)
        {
            return await _repository.GetStudentsByClassId(classId);
        }

        public async Task<List<EnrollmentRequest>> GetEnrolledStudentsWithoutClass(Guid courseId)
        {
            return await _repository.GetEnrolledStudentsWithoutClass(courseId);
        }

        public async Task<List<EnrollmentRequest>> GetPendingStudents(Guid courseId)
        {
            return await _repository.GetPendingStudents(courseId);
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

        public async Task<(bool Success, string Message, Guid? Id)> Create(EnrollmentRequest request)
        {
            if (request == null || request.AccountId == Guid.Empty || request.CourseId == Guid.Empty)
                return (false, "Invalid request data", null);

            // Kiểm tra xem đã tồn tại bản ghi giống chưa
            var existing = await _repository.GetActiveByAccountAndCourseAsync(request.AccountId.Value, request.CourseId.Value);

            if (existing != null)
                return (false, "Students who have requested or are taking this course", null);

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

            var classEntity = await _dbContext.Classes
                .Include(c => c.EnrollmentRequests) // load danh sách học viên trong lớp
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                return (false, "Không tìm thấy lớp học");

            // 🔍 Đếm số học viên hiện tại trong lớp
            int currentStudents = classEntity.EnrollmentRequests.Count(a => a.IsActive == true);

            // 🔍 Kiểm tra số lượng tối đa
            if (classEntity.NumberOfStudent.HasValue && currentStudents >= classEntity.NumberOfStudent.Value)
            {
                return (false, "Lớp đã đủ số lượng học viên, không thể thêm mới.");
            }

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


            _dbContext.EnrollmentRequests.Update(record);
            await _dbContext.SaveChangesAsync();

            return (true, "Đã gán học viên vào lớp và khởi tạo tiến độ thành công.");
        }

        public async Task<(bool Success, string Message)> Update(EnrollmentRequest request)
        {
            if (request == null || request.Id == Guid.Empty)
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(request);
            return result > 0 ? (true, "Request updated successfully") : (false, "Failed to update request");
        }

        public async Task<(bool Success, string Message)> UpdateAccept(EnrollmentRequest request)
        {
            if (request == null || request.Id == Guid.Empty)
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(request);

            if (request.AccountId.HasValue && request.CourseId.HasValue)
            {
                // Lấy WorkSpaceId của course
                var wsId = await _dbContext.Courses
                    .Where(c => c.Id == request.CourseId.Value)
                    .Select(c => c.WorkSpaceId)
                    .FirstOrDefaultAsync();

                if (wsId != null)
                {
                    var existing = await _dbContext.EnrollmentWorkSpaces
                        .FirstOrDefaultAsync(x =>
                            x.AccountId == request.AccountId.Value &&
                            x.WorkSpaceId == wsId.Value);

                    if (existing == null)
                    {
                        _dbContext.EnrollmentWorkSpaces.Add(new EnrollmentWorkSpace
                        {
                            Id = Guid.NewGuid(),
                            AccountId = request.AccountId,
                            WorkSpaceId = wsId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                        });
                        await _dbContext.SaveChangesAsync();
                    }
                    else if (existing.IsActive != true || existing.DeleteAt != null)
                    {
                        existing.IsActive = true;
                        existing.DeleteAt = null;
                        existing.UpdatedAt = DateTime.UtcNow;
                        _dbContext.EnrollmentWorkSpaces.Update(existing);
                        await _dbContext.SaveChangesAsync();
                    }
                    // Nếu đã active đúng rồi thì không cần làm gì
                }
            }
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
