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
        private readonly EnrollmentRequestRepository _enrollRepo;

        public LessonService()
        {
            _repository = new LessonRepository();
            _topicRepo = new TopicRepository();
            _enrollRepo = new EnrollmentRequestRepository();
            _lessonProgressRepo = new LessonProgressRepository();
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
                    Status = request.Status,  
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
            if (request == null || request.Id == Guid.Empty || string.IsNullOrWhiteSpace(request.LessonName))
                return (false, "Invalid update data");

            request.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(request);
            return result > 0 ? (true, "Updated successfully") : (false, "Update failed");
        }
    }
}
