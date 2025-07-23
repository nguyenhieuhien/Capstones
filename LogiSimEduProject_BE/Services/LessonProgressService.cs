using Repositories;
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

        public LessonProgressService()
        {
            _repository = new LessonProgressRepository();
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
