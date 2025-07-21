// File: Services/IAnswerService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
    

    public class AnswerService : IAnswerService
    {
        private readonly AnswerRepository _repository;

        public AnswerService(AnswerRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Answer>> GetAll() => await _repository.GetAll();

        public async Task<Answer> GetById(string id) => await _repository.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> Create(Answer answer)
        {
            answer.Id = Guid.NewGuid();
            answer.CreatedAt = DateTime.UtcNow;
            answer.IsActive = true;

            var result = await _repository.CreateAsync(answer);
            return result > 0 ? (true, "Created successfully") : (false, "Failed to create answer");
        }

        public async Task<(bool Success, string Message)> Update(Answer answer)
        {
            answer.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(answer);
            return result > 0 ? (true, "Updated successfully") : (false, "Failed to update answer");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Answer not found");

            var success = await _repository.RemoveAsync(item);
            return success ? (true, "Deleted successfully") : (false, "Failed to delete answer");
        }
    }
}
