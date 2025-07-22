// File: Services/ICategoryService.cs
using Repositories;
using Repositories.Models;
using Services.IServices;

namespace Services
{
  
    public class CategoryService : ICategoryService
    {
        private readonly CategoryRepository _repository;

        public CategoryService(CategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Category>> GetAll()
        {
            return await _repository.GetAll() ?? new List<Category>();
        }

        public async Task<Category?> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            return await _repository.GetByIdAsync(id);
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Invalid category data", null);

            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
            category.IsActive = true;

            var result = await _repository.CreateAsync(category);
            return result > 0
                ? (true, "Category created successfully", category.Id)
                : (false, "Failed to create category", null);
        }

        public async Task<(bool Success, string Message)> Update(Category category)
        {
            if (category == null || category.Id == Guid.Empty || string.IsNullOrWhiteSpace(category.CategoryName))
                return (false, "Invalid update data");

            category.UpdatedAt = DateTime.UtcNow;
            var result = await _repository.UpdateAsync(category);
            return result > 0 ? (true, "Updated successfully") : (false, "Update failed");
        }

        public async Task<(bool Success, string Message)> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return (false, "Invalid ID");

            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return (false, "Category not found");

            var result = await _repository.RemoveAsync(item);
            return result ? (true, "Deleted successfully") : (false, "Delete failed");
        }
    }
}
