using Azure.Core;
using LogiSimEduProject_BE_API.Controllers.DTO.Scenario;
using LogiSimEduProject_BE_API.Controllers.DTO.Scene;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScenarioController : ControllerBase
    {
        private readonly IScenarioService _service;

        public ScenarioController(IScenarioService service) => _service = service;
        // GET: api/<ScenarioController>
        [HttpGet("GetAllScenario")]
        public async Task<IEnumerable<Scenario>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetScenario/{id}")]
        public async Task<Scenario> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateScenario")]
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
        [HttpPut("UpdateScenario/{id}")]
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
        [HttpDelete("DeleteScenario/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
