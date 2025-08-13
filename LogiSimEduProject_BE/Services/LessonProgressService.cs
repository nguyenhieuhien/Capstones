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

        public async Task<(bool Success, string Message, CertificateDTO? Certificate)> UpdateLessonProgressAsync(Guid accountId, Guid lessonId)
        {
            try
            {
                var progress = await _repository.GetByAccountAndLesson(accountId, lessonId);
                if (progress == null)
                    return (false, "LessonProgress not found.", null);

                progress.Status = 2;
                progress.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(progress);

                CertificateDTO? certificate = null;

                if (result > 0 && progress.Status == 2)
                {
                    certificate = await UpdateCourseProgressAndCertificate(progress.AccountId.Value, progress.LessonId.Value);
                }

                return result > 0
                    ? (true, "Lesson progress updated and course progress refreshed", certificate)
                    : (false, "Failed to update lesson progress", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
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
                        AccessMode = "public"
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
