// File: Controllers/OrganizationController.cs
using LogiSimEduProject_BE_API.Controllers.DTO.Organization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

namespace Controllers
{
    [ApiController]
    [Route("api/organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get_all_organization")]
        [SwaggerOperation(Summary = "Get all organizations", Description = "Returns a list of all active organizations.")]
        public async Task<IActionResult> GetAll()
        {
            var organizations = await _organizationService.GetAll();
            return Ok(organizations);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("get_organization/{id}")]
        [SwaggerOperation(Summary = "Get organization by ID", Description = "Retrieve a single organization by its unique ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var organization = await _organizationService.GetById(id);
            if (organization == null)
                return NotFound(new { Message = "Organization not found." });
            return Ok(organization);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create_organization")]
        [SwaggerOperation(Summary = "Create new organization", Description = "Create a new organization and return its ID.")]
        public async Task<IActionResult> Create([FromBody] OrganizationCreateDTO dto)
        {
            if (dto == null)
                return BadRequest(new { Message = "Invalid request body." });

            var organization = new Organization
            {
                OrganizationName = dto.OrganizationName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            var (success, message, id) = await _organizationService.Create(organization);
            if (!success)
                return BadRequest(new { Message = message });

            return Ok(new { Message = message, OrganizationId = id });
        }

        [Authorize(Roles = "Admin,Organization_Admin")]
        [HttpPut("update_organization/{id}")]
        [SwaggerOperation(Summary = "Update organization", Description = "Update an existing organization by ID.")]
        public async Task<IActionResult> Update(string id, [FromBody] OrganizationUpdateDTO dto)
        {
            var existingOrg = await _organizationService.GetById(id);
            if (existingOrg == null)
                return NotFound(new { Message = "Organization not found." });

            existingOrg.OrganizationName = dto.OrganizationName;
            existingOrg.Email = dto.Email;
            existingOrg.Phone = dto.Phone;
            existingOrg.Address = dto.Address;

            var (success, message) = await _organizationService.Update(existingOrg);
            if (!success)
                return BadRequest(new { Message = message });

            return Ok(new { Message = message });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete_organization/{id}")]
        [SwaggerOperation(Summary = "Delete organization", Description = "Delete an organization by its ID.")]
        public async Task<IActionResult> Delete(string id)
        {
            var (success, message) = await _organizationService.Delete(id);
            if (!success)
                return NotFound(new { Message = message });

            return Ok(new { Message = message });
        }
    }
}
