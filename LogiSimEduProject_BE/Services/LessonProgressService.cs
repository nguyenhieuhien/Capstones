using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using Services.DTO.Certificate;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DinkToPdf;

namespace Services
{
    public class LessonProgressService : ILessonProgressService
    {
        private readonly LessonProgressRepository _repository;
        private readonly LessonRepository _lessonRepo;
        private readonly TopicRepository _topicRepo;
        private readonly CourseProgressRepository _courseProgressRepo;
        private readonly CertificateRepository _certificateRepo;
        private readonly CertificateTemplateRepository _templateRepo;
        private readonly QuizSubmissionRepository _submissionRepo;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly IPdfService _pdfService;
        private readonly LogiSimEduContext _dbContext;
        public LessonProgressService(
               LessonProgressRepository repository,
               LessonRepository lessonRepo,
               TopicRepository topicRepo,
               CourseProgressRepository courseProgressRepo,
               CertificateRepository certificateRepo,
               CertificateTemplateRepository templateRepo,
               QuizSubmissionRepository quizSubmissionRepo,
               CloudinaryDotNet.Cloudinary cloudinary,
               IPdfService pdfService,
               LogiSimEduContext dbContext)
        {
            _repository = repository;
            _lessonRepo = lessonRepo;
            _topicRepo = topicRepo;
            _courseProgressRepo = courseProgressRepo;
            _certificateRepo = certificateRepo;
            _templateRepo = templateRepo;
            _submissionRepo = quizSubmissionRepo;
            _cloudinary = cloudinary;
            _pdfService = pdfService;
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

        public async Task<(bool Success, string Message, CertificateDTO? Certificate, Lesson? NextLesson)> UpdateLessonProgressAsync(Guid accountId, Guid lessonId)
        {
            try
            {
                var lesson = await _lessonRepo.GetByIdAsync(lessonId);
                if (lesson == null)
                    return (false, "Lesson not found.", null, null);

                var quiz = lesson.Quizzes.FirstOrDefault(q => q.IsActive == true);
                if (quiz != null)
                {
                    var submission = await _submissionRepo.GetLatestByAccountAndQuiz(accountId, quiz.Id);
                    if (submission == null)
                        return (false, "You must complete the quiz before finishing this lesson.", null, null);

                    if (submission.TotalScore < 5) // passing score = 5/10
                        return (false, "You must pass the quiz to complete this lesson.", null, null);
                }

                var progress = await _repository.GetByAccountAndLesson(accountId, lessonId);
                if (progress == null)
                    return (false, "LessonProgress not found.", null, null);

                if (progress.Status == 2)
                    return (true, "Lesson already completed.", null, null);

                progress.Status = 2; // Completed
                progress.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(progress);

                if (result <= 0)
                    return (false, "Failed to update lesson progress.", null, null);

                CertificateDTO? certificate = null;

                if (result > 0 && progress.Status == 2)
                {
                    certificate = await UpdateCourseProgressAndCertificate(progress.AccountId.Value, progress.LessonId.Value);
                }

                var nextLesson = await GetNextLessonAsync(lessonId);

                return (true, "Lesson progress updated.", certificate, nextLesson);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null, null);
            }
        }

        private async Task<CertificateDTO?> UpdateCourseProgressAndCertificate(Guid accountId, Guid lessonId)
        {
            var lesson = await _lessonRepo.GetByIdAsync(lessonId);
            if (lesson?.TopicId == null) return null;

            var topic = await _topicRepo.GetByIdAsync(lesson.TopicId.Value);
            if (topic?.CourseId == null) return null;

            var courseId = topic.CourseId.Value;

            var allLessons = await _lessonRepo.GetLessonsByCourseId(courseId);
            var totalLessons = allLessons.Count;
            var completedCount = await _repository.CountCompletedLessonsAsync(accountId, allLessons.Select(l => l.Id).ToList());

            var percent = totalLessons == 0 ? 0 : (completedCount * 100.0 / totalLessons);

            var courseProgress = await _courseProgressRepo.GetByAccAndCourse(accountId, courseId);

            if (courseProgress != null && percent < 100)
            {
                courseProgress.ProgressPercent = percent;
                courseProgress.Status = 2;
                courseProgress.UpdatedAt = DateTime.UtcNow;
                await _courseProgressRepo.UpdateAsync(courseProgress);
            }
            else if (percent == 100)
            {
                // Update trạng thái course progress
                if (courseProgress != null)
                {
                    courseProgress.ProgressPercent = percent;
                    courseProgress.Status = 3;
                    courseProgress.UpdatedAt = DateTime.UtcNow;
                    await _courseProgressRepo.UpdateAsync(courseProgress);
                }

                // Check certificate đã tồn tại chưa
                var existingCert = await _certificateRepo.GetByAccountAndCourse(accountId, courseId);
                if (existingCert.Any())
                {
                    var cert = existingCert.First();
                    return new CertificateDTO
                    {
                        Id = cert.Id,
                        FileUrl = cert.FileUrl
                    };
                }

                // Lấy thông tin account và course
                var account = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Id == accountId);
                var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

                if (account == null || course == null) return null;

                byte[] pdfBytes = _pdfService.GenerateCertificate(
                                 account.FullName,
                                 course.CourseName,
                                "https://png.pngtree.com/background/20250424/original/pngtree-certificate-of-achievement-picture-image_16452391.jpg"
                );

                // Upload PDF lên Cloudinary
                string fileUrl;
                await using (var stream = new MemoryStream(pdfBytes))
                {
                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription($"certificate_{accountId}_{courseId}.pdf", stream),
                        Folder = "LogiSimEdu_Certificates",
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true,
                        PublicId = $"certificate_{accountId}_{courseId}",
                        Type = "upload"       // đảm bảo public
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                        throw new Exception($"Cloudinary upload failed: {uploadResult.Error?.Message}");

                    fileUrl = uploadResult.SecureUrl.ToString();
                }

                // Lưu certificate vào DB
                var certificate = new Certificate
                {
                    Id = Guid.NewGuid(),
                    AccountId = accountId,
                    CourseId = courseId,
                    CertiTempId = null, // vì không dùng template DB
                    CertificateName = $"{account.FullName} - {course.CourseName}",
                    Score = 95,
                    Rank = null,
                    FileUrl = fileUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _certificateRepo.CreateAsync(certificate);

                return new CertificateDTO
                {
                    Id = certificate.Id,
                    FileUrl = certificate.FileUrl
                };
            }
            return null;
        }

        private async Task<Lesson?> GetNextLessonAsync(Guid currentLessonId)
        {
            var currentLesson = await _lessonRepo.GetByIdAsync(currentLessonId);
            if (currentLesson == null || currentLesson.TopicId == null) return null;

            var currentTopic = await _topicRepo.GetByIdAsync(currentLesson.TopicId.Value);
            if (currentTopic == null || currentTopic.CourseId == null) return null;

            // 1. next lesson in same topic
            var nextLesson = await _lessonRepo.GetByTopicAndOrderIndexAsync(currentTopic.Id, currentLesson.OrderIndex + 1);
            if (nextLesson != null) return nextLesson;

            // 2. next topic
            var nextTopic = await _topicRepo.GetByCourseAndOrderIndexAsync(currentTopic.CourseId.Value, currentTopic.OrderIndex + 1);
            if (nextTopic == null) return null;

            // 3. first lesson of next topic
            return await _lessonRepo.GetByTopicAndOrderIndexAsync(nextTopic.Id, 1);
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var lessonProgress = await _repository.GetByIdAsync(id);
                if (lessonProgress == null)
                    return (false, "Lesson progress not found");

                lessonProgress.IsActive = false;
                lessonProgress.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(lessonProgress);
                return (true, "Deleted successfully");
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
