using LogiSimEduProject_BE_API.Controllers.DTO.Category;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all categories", Description = "Returns a list of all active categories.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAll();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        [SwaggerOperation(Summary = "Get category by ID", Description = "Retrieve a single category by its unique ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var category = await _categoryService.GetById(id);
            if (category == null)
                return NotFound("Category not found");
            return Ok(category);
        }

        [HttpPost("create")]
        [SwaggerOperation(Summary = "Create new category", Description = "Create a new category and return its ID.")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CategoryName))
                return BadRequest("Invalid input");

            var category = new Category
            {
                CategoryName = dto.CategoryName
            };

            var (success, message, id) = await _categoryService.Create(category);
            if (!success)
                return BadRequest(message);

            return Ok(new { Message = message, Id = id });
        }

        [HttpPut("update/{id}")]
        [SwaggerOperation(Summary = "Update category", Description = "Update an existing category by ID.")]
        public async Task<IActionResult> Update(string id, [FromBody] CategoryUpdateDTO dto)
        {
            var existing = await _categoryService.GetById(id);
            if (existing == null)
                return NotFound("Category not found");

            existing.CategoryName = dto.CategoryName;

            var (success, message) = await _categoryService.Update(existing);
            return success ? Ok(message) : BadRequest(message);
        }

        [HttpDelete("delete/{id}")]
        [SwaggerOperation(Summary = "Delete category", Description = "Delete a category by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _categoryService.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
