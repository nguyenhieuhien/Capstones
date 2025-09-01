using CloudinaryDotNet;
using Google.Apis.Storage.v1.Data;
using Microsoft.AspNetCore.Identity;
using Repositories;
using Repositories.Models;
using Services.DTO.Lesson;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class LessonService : ILessonService
    {
        private readonly LessonRepository _repository;
        private readonly TopicRepository _topicRepo;
        private readonly LessonProgressRepository _lessonProgressRepo;
        private readonly QuizSubmissionRepository _quizSubRepo;
        private readonly EnrollmentRequestRepository _enrollRepo;

        public LessonService(
            LessonRepository repository,
            QuizSubmissionRepository quizSubRepo,
            TopicRepository topicRepo,
            LessonProgressRepository lessonProgressRepo,
            EnrollmentRequestRepository enrollRepo
            )
        {
            _repository = repository;
            _topicRepo = topicRepo;
            _enrollRepo = enrollRepo;
            _quizSubRepo = quizSubRepo;
            _lessonProgressRepo = lessonProgressRepo;
        }
        public async Task<(bool Success, string Message, Guid? Id)> Create(Lesson request)
        {
            try
            {
                var lesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    TopicId = request.TopicId,
                    ScenarioId=request.ScenarioId,
                    LessonName = request.LessonName,
                    OrderIndex = request.OrderIndex,
                    Description = request.Description,
                    Title = request.Title,
                    Status = 1,  
                    FileUrl = request.FileUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.CreateAsync(lesson);

                var topic = await _topicRepo.GetByIdAsync(lesson.TopicId.Value);
                if (topic?.CourseId != null)
                {
                    var courseId = topic.CourseId.Value;

                    // Lấy danh sách học viên trong course
                    var students = await _enrollRepo.GetStudentsByCourseId(courseId);

                    foreach (var student in students)
                    {
                        bool exists = await _lessonProgressRepo.ExistsAsync(student.AccountId.Value, lesson.Id);
                        if (!exists)
                        {
                            var lessonProgress = new LessonProgress
                            {
                                Id = Guid.NewGuid(),
                                AccountId = student.AccountId,
                                LessonId = lesson.Id,
                                Status = 1, // not started
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            await _lessonProgressRepo.Created(lessonProgress);
                        }
                    }
                }

                if (result > 0)
                    return (true, "Lesson created successfully", lesson.Id);
                return (false, "Failed to create lesson", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var lesson = await _repository.GetByIdAsync(id);
                if (lesson == null)
                    return (false, "Lesson not found");

                lesson.IsActive = false;
                lesson.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(lesson);
                return (true, "Deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<Lesson>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Lesson?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Lesson>> GetLessonsByTopicId(Guid topicId)
        {
            return await _repository.GetLessonsByTopicIdAsync(topicId);
        }

        public async Task<List<LessonWithQuizzesDTO>> GetLessonsWithLatestScoresAsync(Guid topicId, Guid? accountId = null)
        {
            var lessons = await _repository.GetLessonsByTopicId(topicId);

            Dictionary<Guid, double?> latestScores = new();
            if (accountId.HasValue)
            {
                latestScores = await _quizSubRepo.GetLatestScoresByQuizForTopicAsync(topicId, accountId.Value);
            }

            var result = lessons.Select(l => new LessonWithQuizzesDTO
            {
                Id = l.Id,
                TopicId = l.TopicId,
                LessonName = l.LessonName,     // đổi tên field cho khớp entity
                Title = l.Title,
                Description = l.Description,

                Scenario = (l.Scenario != null && (l.Scenario.IsActive ?? true))
                ? new ScenarioDto
                {
                    Id = l.Scenario.Id,
                    ScenarioName = l.Scenario.ScenarioName,
                    FileUrl = l.Scenario.FileUrl,
                    Description = l.Scenario.Description
                }
                : null,

                LessonProgresses = (l.LessonProgresses ?? [])
                .Where(lp => (lp.IsActive ?? false)
                             && (!accountId.HasValue || lp.AccountId == accountId))
                .Select(lp => new LessonProgressDto
                {
                    Id = lp.Id,
                    AccountId = lp.AccountId,
                    Status = lp.Status,
                    UpdatedAt = lp.UpdatedAt
                })
                .ToList(),

                LessonSubmissions = (l.LessonSubmissions ?? [])
                .Where(ls => ls.IsActive
                             && (!accountId.HasValue || ls.AccountId == accountId))
                .OrderByDescending(ls => ls.SubmitTime)
                .Select(ls => new LessonSubmissionDto
                {
                    Id = ls.Id,
                    AccountId = ls.AccountId,
                    SubmitTime = ls.SubmitTime,
                    FileUrl = ls.FileUrl,
                    Note = ls.Note,
                    TotalScore = ls.TotalScore
                })
                .ToList(),

                Quizzes = l.Quizzes
                    .Where(q => q.IsActive == true)
                    .Select(q => new QuizWithLatestScoreDto
                    {
                        Id = q.Id,
                        QuizName = q.QuizName,
                        TotalScore = q.TotalScore,
                        LatestScore = accountId.HasValue
                            ? latestScores.GetValueOrDefault(q.Id)
                            : null
                    })
                    .ToList()
            }).ToList();



            return result;
        }

        public async Task<List<QuizDTOByLesson>> GetQuizzesByLessonId(Guid lessonId)
        {
            var quizzes = await _repository.GetQuizzesByLessonId(lessonId);

            return quizzes.Select(q => new QuizDTOByLesson
            {
                QuizId = q.Id,
                QuizName = q.QuizName,
            }).ToList();
        }

        public async Task<(bool Success, string Message)> Update(Lesson request)
        {
            // Partial update: không bắt buộc LessonName phải được gửi lại
            if (request == null || request.Id == Guid.Empty)
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow; // nếu muốn bỏ hẳn UpdatedAt, xóa dòng này
            var affected = await _repository.UpdateAsync(request);

            if (affected == 0)
                return (true, "Không có thay đổi nào để cập nhật."); // không coi là lỗi

            return (true, "Updated successfully");
        }

    }
}
