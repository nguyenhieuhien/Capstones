using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services.DTO.SubscriptionPlan;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/subscription-plan")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ISubscriptionPlanService _service;

        public SubscriptionPlanController(ISubscriptionPlanService service)
        {
            _service = service;
        }


        [HttpGet("get_all")]
        [SwaggerOperation(Summary = "Get all subscription plans", Description = "Retrieve all subscription plans")]
        public async Task<IActionResult> GetAll()
        {
            var plans = await _service.GetAll();
            return Ok(plans);
        }

        [HttpGet("get_all_active")]
        public async Task<IActionResult> GetAllActive()
        {
            var plans = await _service.GetAllActive();
            return Ok(plans);
        }

        [HttpGet("get/{id}")]
        [SwaggerOperation(Summary = "Get a subscription plan by ID", Description = "Retrieve a subscription plan by its ID")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _service.GetById(id);
            return plan != null ? Ok(plan) : NotFound($"Subscription plan with ID {id} not found.");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        [SwaggerOperation(Summary = "Create a new subscription plan", Description = "Add a new subscription plan")]
        public async Task<IActionResult> Create([FromBody] SubscriptionPlanDTOCreate request)
        {
            var plan = new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Price = (double)request.Price,
                DurationInMonths = request.DurationInMonths,
                MaxWorkSpaces = request.MaxWorkSpaces,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            var (success, message) = await _service.Create(plan);
            return success ? Ok(new { message, data = plan }) : BadRequest(message);
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("update/{id}")]
        [SwaggerOperation(Summary = "Update a subscription plan", Description = "Update properties of a subscription plan")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SubscriptionPlanDTOUpdate request)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound($"Subscription plan with ID {id} not found.");

            // Strings: bỏ qua nếu null/white-space, có thì Trim
            if (!string.IsNullOrWhiteSpace(request.Name))
                existing.Name = request.Name.Trim();

            if (!string.IsNullOrWhiteSpace(request.Description))
                existing.Description = request.Description.Trim();
            // Numbers/booleans: chỉ gán khi HasValue
            if (request.Price.HasValue)
                existing.Price = (double)request.Price.Value;

            if (request.DurationInMonths.HasValue)
                existing.DurationInMonths = request.DurationInMonths.Value;

            if (request.MaxWorkSpaces.HasValue)
                existing.MaxWorkSpaces = request.MaxWorkSpaces.Value;

            // ✅ IsActive
            if (request.IsActive.HasValue)
                existing.IsActive = request.IsActive.Value;

            // UpdatedAt
            existing.UpdatedAt = DateTime.UtcNow;

            var (success, message) = await _service.Update(existing);
            return success ? Ok(new { message, data = existing }) : BadRequest(message);
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        [SwaggerOperation(Summary = "Delete a subscription plan", Description = "Soft delete a subscription plan by ID")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var (success, message) = await _service.Delete(id);
            return success ? Ok(message) : NotFound(message);
        }
    }
}
