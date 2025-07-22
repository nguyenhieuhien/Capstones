// File: Services/QuestionService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    public class QuestionService : IQuestionService
    {
        private readonly QuestionRepository _repository;

        public QuestionService()
        {
            _repository = new QuestionRepository();
        }

        public async Task<List<Question>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Question>();
        }

        public async Task<Question?> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Question question)
        {
            try
            {
                question.Id = Guid.NewGuid();
                question.IsActive = true;
                question.CreatedAt = DateTime.UtcNow;

                var result = await _repository.CreateAsync(question);
                if (result > 0)
                    return (true, "Question created successfully", question.Id);
                return (false, "Failed to create question", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string Message)> Update(Question question)
        {
            try
            {
                question.UpdatedAt = DateTime.UtcNow;
                var result = await _repository.UpdateAsync(question);
                if (result > 0)
                    return (true, "Question updated successfully");
                return (false, "Failed to update question");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null)
                    return (false, "Question not found");

                var result = await _repository.RemoveAsync(item);
                if (result)
                    return (true, "Question deleted successfully");
                return (false, "Failed to delete question");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
