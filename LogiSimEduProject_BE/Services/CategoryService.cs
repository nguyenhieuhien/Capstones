    using Repositories;
    using Repositories.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace Services
    {
        public interface ICategoryService
        {
            Task<List<Category>> GetAll();
            Task<Category> GetById(string id);
            Task<int> Create(Category category);
            Task<int> Update(Category category);
            Task<bool> Delete(string id);
        }
        public class CategoryService : ICategoryService
        {
            private readonly CategoryRepository _repository;

            public CategoryService()
            {
                _repository = new CategoryRepository();
            }

            public async Task<int> Create(Category category)
            {
                if (category == null || string.IsNullOrEmpty(category.CategoryName))
                {
                    return 0;
                }
                var result = await _repository.CreateAsync(category);
                return result;
            }

            public async Task<bool> Delete(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return false;
                }
                var item = await _repository.GetByIdAsync(id);
                if (item != null)
                {
                    var result = await _repository.RemoveAsync(item);
                    return result;
                }
                return false;
            }

            public async Task<List<Category>> GetAll()
            {
                var categories = await _repository.GetAll();
                return categories ?? new List<Category>();
            }

            public async Task<Category> GetById(string id)
            {
                if (string.IsNullOrEmpty(id))
                {
                    return null;
                }
                var category = await _repository.GetByIdAsync(id);
                return category;
            }

            public async Task<int> Update(Category category)
            {
                if (category == null || category.Id == Guid.Empty || string.IsNullOrEmpty(category.CategoryName))
                {
                    return 0;
                }
                var result = await _repository.UpdateAsync(category);
                return result;
            }
        }
    }
