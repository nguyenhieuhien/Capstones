using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Scenario;
using LogiSimEduProject_BE_API.Controllers.DTO.Scene;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/scenario")]
    [ApiController]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _service;

        public ScenarioController(IScenarioService service) => _service = service;
        // GET: api/<ScenarioController>
        [HttpGet("get_all_scenario")]
        [SwaggerOperation(Summary = "Get all scenarios", Description = "Returns a list of all scenarios.")]
        public async Task<IEnumerable<Scenario>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_scenario/{id}")]
        [SwaggerOperation(Summary = "Get a scenario by ID", Description = "Returns a scenario based on its unique ID.")]
        public async Task<Scenario> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_scenario")]
        [SwaggerOperation(Summary = "Create a new scenario", Description = "Creates a new scenario with provided data.")]
        public async Task<IActionResult> Post(ScenarioDTOCreate request)
        {
            var scenario = new Scenario
            {
                SceneId = request.SceneId,
                ScenarioName = request.ScenarioName,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(scenario);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_scenario/{id}")]
        [SwaggerOperation(Summary = "Update a scenario", Description = "Updates an existing scenario by ID.")]
        public async Task<IActionResult> Put(string id,ScenarioDTOUpdate request)
        {
            var existingScenario = await _service.GetById(id);
            if (existingScenario == null)
            {
                return NotFound(new { Message = $"Scenario with ID {id} was not found." });
            }
            existingScenario.SceneId = request.SceneId;
            existingScenario.ScenarioName = request.ScenarioName;
            existingScenario.Description = request.Description;
            existingScenario.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingScenario);

            return Ok(new
            {
                Message = "Scenario updated successfully.",
                Data = new
                {
                    SceneId = existingScenario.SceneId,
                    ScenarioName = existingScenario.ScenarioName,
                    Description = existingScenario.Description,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("delete_scenario/{id}")]
        [SwaggerOperation(Summary = "Delete a scenario", Description = "Deletes a scenario based on its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
