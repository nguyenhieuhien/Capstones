
//using LogiSimEduProject_BE_API.Controllers.DTO.SubscriptionPlan;
//using Microsoft.AspNetCore.Mvc;
//using Repositories.Models;
//using Services.IServices;

//namespace LogiSimEduProject_BE_API.Controllers
//{
//    [ApiController]
//    [Route("api/subscription-plan")]
//    public class SubscriptionPlanController : ControllerBase
//    {
//        private readonly ISubscriptionPlanService _service;

//        public SubscriptionPlanController(ISubscriptionPlanService service)
//        {
//            _service = service;
//        }

//        [HttpGet("get_all")]
//        public async Task<ActionResult<List<SubscriptionPlanDTO>>> GetAll()
//        {
//            var result = await _service.GetAllAsync();
//            return Ok(result);
//        }

//        [HttpGet("get/{id}")]
//        public async Task<ActionResult<SubscriptionPlanDTO>> GetById(Guid id)
//        {
//            var plan = await _service.GetByIdAsync(id);
//            if (plan == null) return NotFound();
//            return Ok(plan);
//        }

//        [HttpPost("create")]
//        public async Task<ActionResult<int>> Create([FromBody] SubscriptionPlanDTO dto)
//        {
//            var result = await _service.CreateAsync(dto);
//            return Ok(result);
//        }

//        [HttpPut("update/{id}")]
//        public async Task<ActionResult<int>> Update(Guid id, [FromBody] SubscriptionPlanDTO dto)
//        {
//            if (id != dto.Id) return BadRequest("ID mismatch");
//            var result = await _service.UpdateAsync(dto);
//            return Ok(result);
//        }

//        [HttpDelete("delete/{id}")]
//        public async Task<ActionResult<bool>> Delete(Guid id)
//        {
//            var result = await _service.DeleteAsync(id);
//            return Ok(result);
//        }
//    }

//}
