using LogiSimEduProject_BE_API.Controllers.DTO.Category;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController()
        {
            _categoryService = new CategoryService();
        }

        [HttpGet("get_all_category")]
        [SwaggerOperation(Summary = "Get all categories", Description = "Returns a list of all active categories.")]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            var categories = await _categoryService.GetAll();

            // Nếu null (trong trường hợp service bị sai), thì trả về danh sách rỗng
            if (categories == null)
                return Ok(new List<Category>());

            return Ok(categories); // Trả về [] nếu danh sách rỗng
        }


        [HttpGet("Get_category/{id}")]
        [SwaggerOperation(Summary = "Get category by ID", Description = "Retrieve a single category by its unique ID.")]
        public async Task<ActionResult<Category>> GetById(string id)
        {
            var category = await _categoryService.GetById(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }

        [HttpPost("create_category")]
        [SwaggerOperation(Summary = "Create new category", Description = "Create a new category and return its ID.")]
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

        [HttpPut("update_category/{id}")]
        [SwaggerOperation(Summary = "Update category", Description = "Update an existing category by ID.")]
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

        [HttpDelete("delete_category/{id}")]
        [SwaggerOperation(Summary = "Delete category", Description = "Delete a category by its ID.")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            var result = await _categoryService.Delete(id);
            if (!result)
                return NotFound();
            return Ok(result);
        }
    }
}
