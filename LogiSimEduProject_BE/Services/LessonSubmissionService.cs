using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Repositories;
using Repositories.Models;
using Services.DTO.LessonSubmission;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class LessonSubmissionService : ILessonSubmissionService
    {
        private readonly LessonSubmissionRepository _repository;
        private readonly AccountRepository _accRepo;
        private readonly LessonRepository _lessonRepo;
        private readonly TopicRepository _topicRepo;
        private readonly CourseRepository _courseRepo;
        private readonly EmailService _emailRepo;

        public LessonSubmissionService(
       LessonSubmissionRepository repository,
       AccountRepository accRepo,
       LessonRepository lessonRepo,
       TopicRepository topicRepo,
       CourseRepository courseRepo,
       EmailService emailRepo)
        {
            _repository = repository;
            _accRepo = accRepo;
            _lessonRepo = lessonRepo;
            _topicRepo = topicRepo;
            _courseRepo = courseRepo;
            _emailRepo = emailRepo; 
        }

        public async Task<LessonSubmission?> GetLessonSubmission(Guid lessonId, Guid accountId)
        {
            return await _repository.GetLessonSubmissionAsync(lessonId, accountId);
        }

        public async Task<(bool Success, string Message, Guid? Id)> SubmitLesson(LessonSubmission lessonSubmission)
        {
            try {
                var existing = await _repository.GetLessonSubmissionAsync(lessonSubmission.LessonId,lessonSubmission.AccountId);

                if (existing != null)
                {
                    return (false, "You have already submitted this lesson.", existing.Id);
                }

                var submission = new LessonSubmission
                {
                    Id = Guid.NewGuid(),
                    AccountId = lessonSubmission.AccountId,
                    LessonId = lessonSubmission.LessonId,
                    Note = lessonSubmission.Note,
                    SubmitTime = DateTime.UtcNow,
                    FileUrl = lessonSubmission.FileUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.CreateAsync(submission);
                return result > 0 ? (true, "Submission successful", submission.Id) : (false, "Database error", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<Dictionary<string, List<StudentSubmissionDTO>>> GetGroupedByClassAsync(Guid lessonId)
        {
            var submissions = await _repository.GetByLessonIdAsync(lessonId);

            return submissions
            .GroupBy(s =>
                s.Account.AccountOfCourses
                    .FirstOrDefault(ac => ac.ClassId != null)?.Class?.ClassName ?? "Chưa có lớp"
            )
            .ToDictionary(
                g => g.Key,
                g => g.Select(s => new StudentSubmissionDTO
                {
                    SubmissionId = s.Id,
                    StudentId = s.AccountId,
                    StudentName = s.Account.FullName,
                    FileUrl = s.FileUrl,
                    SubmitTime = s.SubmitTime,
                    TotalScore = s.TotalScore
                }).ToList()
            );
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var lessonSubmission = await _repository.GetByIdAsync(id);
                if (lessonSubmission == null)
                    return (false, "LessonSubmission not found");

                lessonSubmission.IsActive = false;
                lessonSubmission.DeleteAt = DateTime.UtcNow;

                await _repository.UpdateAsync(lessonSubmission);
                return (true, "Deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<LessonSubmission>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<LessonSubmission?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message)> Update(LessonSubmission lessonSubmission)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(lessonSubmission.Id);
                if (existing == null)
                    return (false, $"Lesson Submission with ID {lessonSubmission.Id} not found");

                existing.TotalScore = lessonSubmission.TotalScore;
                existing.FileUrl = lessonSubmission.FileUrl;
                existing.Note = lessonSubmission.Note;
                existing.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(existing);
                if (result > 0)
                    return (true, "Lesson Submission updated successfully");

                return (false, "Failed to update lesson Submission");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> GradeSubmit(LessonSubmission lessonSubmission)
        {
            try
            {
                // 0) Tìm submission
                var existing = await _repository.GetByIdAsync(lessonSubmission.Id);
                if (existing == null)
                    return (false, $"Lesson Submission with ID {lessonSubmission.Id} not found");

                // 1) Cập nhật điểm
                existing.TotalScore = lessonSubmission.TotalScore;
                existing.UpdatedAt = DateTime.UtcNow;

                var result = await _repository.UpdateAsync(existing);
                if (result <= 0)
                    return (false, "Failed to update lesson submission");

                // 2) Lấy thông tin student, lesson, course
                var student = await _accRepo.GetByIdAsync(existing.AccountId);

                // Lấy lesson kèm topic (đã có Include Topic)
                var lesson = await _lessonRepo.GetByIdWithTopicCourseAsync(existing.LessonId);
                Course course = null;
                if (lesson?.Topic?.CourseId != null)
                {
                    course = await _courseRepo.GetByIdAsync(lesson.Topic.CourseId.Value);
                }

                // 3) Link course detail FE
                var courseLink = (course != null)
    ?           $"https://capstone-flexsim-fe.vercel.app/course-detail/{course.Id}"
    :           "#";

                // 4) Email
                if (!string.IsNullOrWhiteSpace(student?.Email))
                {
                    var subject = "LogiSimEdu - Your submission has been graded";
                    var body = $@"
                            <p>Hi {(student.FullName ?? "Student")},</p>
                            <p>Your submission has been graded. Please check the details below:</p>
                                <ul>
                                    <li><strong>Course:</strong> {System.Net.WebUtility.HtmlEncode(course?.CourseName ?? "N/A")}</li>
                                    <li><strong>Lesson:</strong> {System.Net.WebUtility.HtmlEncode(lesson?.LessonName ?? "N/A")}</li>
                                    <li><strong>Total Score:</strong> {existing.TotalScore}</li>
                                    <li><strong>Updated At:</strong> {existing.UpdatedAt:yyyy-MM-dd HH:mm} (UTC)</li>
                                </ul>
                            <p><a href=""{courseLink}"">👉 Open Course</a></p>
                            <p>Best regards,<br/>LogiSimEdu</p>";

                    await _emailRepo.SendEmailAsync(student.Email, subject, body);
                }

                return (true, "Lesson Submission updated and email sent successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
