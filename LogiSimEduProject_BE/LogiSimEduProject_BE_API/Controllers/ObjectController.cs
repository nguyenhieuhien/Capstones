using Microsoft.AspNetCore.Mvc;
using Services;
using Services.DTO.Object;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/object")]
    [ApiController]
    public class ObjectController : ControllerBase
    {
        private readonly IObjectService _objectService;

        public ObjectController(IObjectService objectService)
        {
            _objectService = objectService;
        }

        [HttpGet("get_all_object")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _objectService.GetAll();
            return Ok(result);
        }

        [HttpGet("get-all/{scenarioId}")]
        public async Task<IActionResult> GetAll(Guid scenarioId)
        {
            var result = await _objectService.GetAllByScenarioIdAsync(scenarioId);

            if (result == null || !result.Any())
                return NotFound("No objects found for this scenario.");

            return Ok(result);
        }

        [HttpPost("bulk")]
        public async Task<IActionResult> CreateObjects([FromBody] List<ObjectDto> dtos)
        {
            if (dtos == null || !dtos.Any())
                return BadRequest("No objects provided");

            var created = await _objectService.CreateObjectsAsync(dtos);
            return Ok(created);
        }

        [HttpPut("update-many/{scenarioId}")]
        public async Task<IActionResult> UpdateMany(Guid scenarioId, [FromBody] ObjectUpdateListDto dto)
        {
            var result = await _objectService.UpdateManyByScenarioAsync(scenarioId, dto);
            return Ok(result);
        }

        [HttpDelete("{objectId}")]
        public async Task<IActionResult> DeleteObject(Guid objectId)
        {
            var result = await _objectService.SoftDeleteObjectAsync(objectId);
            if (!result)
                return NotFound("Object not found or already deleted");

            return Ok("Object and related Method/Script soft deleted successfully");
        }

    }
}
