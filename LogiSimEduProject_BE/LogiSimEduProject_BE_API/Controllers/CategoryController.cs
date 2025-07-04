using LogiSimEduProject_BE_API.Controllers.DTO.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController()
        {
            _categoryService = new CategoryService();
        }

        [HttpGet("GetAllCategory")]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            var categories = await _categoryService.GetAll();
            return Ok(categories);
        }

        [HttpGet("GetCategory/{id}")]
        public async Task<ActionResult<Category>> GetById(string id)
        {
            var category = await _categoryService.GetById(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost("CreateCategory")]
        public async Task<ActionResult<int>> Create([FromBody] CategoryCreateDTO categoryDto)
        {
            if (categoryDto == null)
                return BadRequest();

            var category = new Category
            {
                //Id = Guid.NewGuid(),
                CategoryName = categoryDto.CategoryName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                DeleteAt = null
            };

            var result = await _categoryService.Create(category);
            if (result == 0)
                return BadRequest();
            return Ok(result);
        }

        [HttpPut("UpdateCategory/{id}")]
        public async Task<ActionResult<int>> Update(string id, [FromBody] CategoryUpdateDTO categoryDto)
        {
            if (categoryDto == null || string.IsNullOrEmpty(id))
                return BadRequest();
            var existingCategory = await _categoryService.GetById(id);
            if (existingCategory == null)
                return NotFound();
            existingCategory.CategoryName = categoryDto.CategoryName;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            var result = await _categoryService.Update(existingCategory);
            if (result == 0)
                return BadRequest();
            return Ok(result);

        }

        [HttpDelete("DeleteCategory/{id}")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            var result = await _categoryService.Delete(id);
            if (!result)
                return NotFound();
            return Ok(result);
        }
    }
}
