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
        [HttpGet]
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
        [HttpPost]
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
        public async Task<int> Put(Role role)
        {
            return await _service.Update(role);
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
