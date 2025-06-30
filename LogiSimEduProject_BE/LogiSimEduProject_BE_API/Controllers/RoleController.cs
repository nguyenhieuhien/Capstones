using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using LogiSimEduProject_BE_API.Controllers.DTO.Role;
using LogiSimEduProject_BE_API.Controllers.DTO.Account;
using Newtonsoft.Json.Linq;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _service;
        public RoleController(IRoleService service) => _service = service;
        [HttpGet("GetAll")]
        public async Task<IEnumerable<Role>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<Role> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
        public async Task<IActionResult> Create(RoleDTOCreate request)
        {
            var role = new Role
            {
                RoleName = request.RoleName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(role);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });


        }

        //[Authorize(Roles = "1")]
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id,RoleDTOUpdate request)
        {
            var existingRole = await _service.GetById(id);
            if (existingRole == null)
            {
                return NotFound(new { Message = $"Role with ID {id} was not found." });
            }
            existingRole.RoleName = request.RoleName;
            
            await _service.Update(existingRole);

            return Ok(new
            {
                Message = "Account updated successfully.",
                Data = new
                {
                    RoleName = existingRole.RoleName,
                    IsActive = existingRole.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
