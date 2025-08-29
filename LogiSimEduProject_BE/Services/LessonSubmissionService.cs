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

        public LessonSubmissionService()
        {
            _repository = new LessonSubmissionRepository();
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
    }
}
