// File: Controllers/ScenarioController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.Controllers.DTO.Scenario;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/scenario")]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _service;

        public ScenarioController(IScenarioService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_all_scenario")]
        [SwaggerOperation(Summary = "Get all scenarios", Description = "Returns all active scenarios.")]
        public async Task<ActionResult<IEnumerable<Scenario>>> GetAll()
        {
            var scenarios = await _service.GetAll();
            return Ok(scenarios);
        }

        [Authorize(Roles = "Student,Instructor")]
        [HttpGet("get_scenario/{id}")]
        [SwaggerOperation(Summary = "Get scenario by ID", Description = "Retrieve a scenario using its unique identifier.")]
        public async Task<ActionResult<Scenario>> GetById(string id)
        {
            var scenario = await _service.GetById(id);
            if (scenario == null)
                return NotFound(new { Message = $"Scenario with ID {id} not found." });

            return Ok(scenario);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("create_scenario")]
        [SwaggerOperation(Summary = "Create new scenario", Description = "Instructor can create a new simulation scenario.")]
        public async Task<IActionResult> Create([FromBody] ScenarioDTOCreate dto)
        {
            var scenario = new Scenario
            {
                SceneId = dto.SceneId,
                ScenarioName = dto.ScenarioName,
                Description = dto.Description
            };

            var result = await _service.Create(scenario);
            if (result <= 0)
                return BadRequest("Failed to create scenario.");

            return Ok(new { Message = "Scenario created successfully." });
        }

        [Authorize(Roles = "Instructor")]
        [HttpPut("update_scenario/{id}")]
        [SwaggerOperation(Summary = "Update scenario", Description = "Update existing scenario by ID.")]
        public async Task<IActionResult> Update(string id, [FromBody] ScenarioDTOUpdate dto)
        {
            var existing = await _service.GetById(id);
            if (existing == null)
                return NotFound(new { Message = $"Scenario with ID {id} not found." });

            existing.SceneId = dto.SceneId;
            existing.ScenarioName = dto.ScenarioName;
            existing.Description = dto.Description;

            var result = await _service.Update(existing);
            if (result <= 0)
                return BadRequest("Failed to update scenario.");

            return Ok(new { Message = "Scenario updated successfully." });
        }

        [Authorize(Roles = "Instructor")]
        [HttpDelete("delete_scenario/{id}")]
        [SwaggerOperation(Summary = "Delete scenario", Description = "Deletes a scenario by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.Delete(id);
            if (!result)
                return NotFound(new { Message = $"Scenario with ID {id} not found or already deleted." });

            return Ok(new { Message = "Scenario deleted successfully." });
        }
    }
}
