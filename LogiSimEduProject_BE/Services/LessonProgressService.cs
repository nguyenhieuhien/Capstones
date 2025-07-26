using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class LessonProgressService : ILessonProgressService
    {
        private readonly LessonProgressRepository _repository;
        private readonly LessonRepository _lessonRepo;
        private readonly TopicRepository _topicRepo;
        private readonly CourseProgressRepository _courseProgressRepo;
        private readonly LogiSimEduContext _dbContext;
        public LessonProgressService(LogiSimEduContext dbContext)
        {
            _repository = new LessonProgressRepository();
            _lessonRepo = new LessonRepository();
            _topicRepo = new TopicRepository();
            _courseProgressRepo = new CourseProgressRepository();
            _dbContext = dbContext;
        }
        public async Task<(bool Success, string Message, Guid? Id)> Create(LessonProgress request)
        {
            try
            {
                var lessonProgess = new LessonProgress
                {
                    Id = Guid.NewGuid(),
                    AccountId = request.AccountId,
                    LessonId = request.LessonId,
                    Status = request.Status,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.CreateAsync(lessonProgess);
                if (result > 0)
                    return (true, "Lesson created successfully", lessonProgess.Id);
                return (false, "Failed to create lesson", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> UpdateLessonProgressAsync(Guid accountId, Guid lessonId, int status)
        {
            try
            {
                var progress = await _repository.GetByAccountAndLesson(accountId, lessonId);
                if (progress == null)
                    return (false, "LessonProgress not found.");

                progress.Status = status;
                progress.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(progress);

                if (result > 0 && progress.Status == 2)
                {
                    await UpdateCourseProgress(progress.AccountId.Value, progress.LessonId.Value);
                }

                return result > 0
                    ? (true, "Lesson progress updated and course progress refreshed")
                    : (false, "Failed to update lesson progress");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task UpdateCourseProgress(Guid accountId, Guid lessonId)
        {
            var lesson = await _lessonRepo.GetByIdAsync(lessonId);
            if (lesson?.TopicId == null) return;

            var topic = await _topicRepo.GetByIdAsync(lesson.TopicId.Value);
            if (topic?.CourseId == null) return;

            var courseId = topic.CourseId.Value;

            var allLessons = await _lessonRepo.GetLessonsByCourseId(courseId);
            var totalLessons = allLessons.Count;
            var completedCount = await _repository.CountCompletedLessonsAsync(accountId, allLessons.Select(l => l.Id).ToList());

            var percent = totalLessons == 0 ? 0 : (completedCount * 100.0 / totalLessons);

            var courseProgress = await _courseProgressRepo.GetByAccountAndCourse(accountId, courseId);

            if (courseProgress != null)
            {
                courseProgress.ProgressPercent = percent;
                courseProgress.UpdatedAt = DateTime.UtcNow;
                await _courseProgressRepo.UpdateAsync(courseProgress);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var lessonProgress = await _repository.GetByIdAsync(id);
                if (lessonProgress == null)
                    return (false, "Lesson progress not found");

                var result = await _repository.RemoveAsync(lessonProgress);
                if (result)
                    return (true, "Lesson progress deleted successfully");
                return (false, "Failed to delete Lesson progress");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<LessonProgress>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<LessonProgress?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> Update(LessonProgress request)
        {
            try
            {
                var lessonProgress = new LessonProgress
                {
                    AccountId = request.AccountId,
                    LessonId = request.LessonId,
                    Status = request.Status,
                    IsActive = true,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = await _repository.UpdateAsync(lessonProgress);
                if (result > 0)
                    return (true, "Lesson progress updated successfully");
                return (false, "Failed to update lesson progress");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
