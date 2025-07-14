using LogiSimEduProject_BE_API.Controllers.DTO.Organization;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Controllers
{
    [ApiController]
    [Route("api/organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController()
        {
            _organizationService = new OrganizationService();
        }

        [HttpGet("get_all_organization")]
        [SwaggerOperation(Summary = "Get all organizations", Description = "Returns a list of all active organizations.")]
        public async Task<ActionResult<List<Organization>>> GetAll()
        {
            var organizations = await _organizationService.GetAll();
            if (organizations == null)
                return Ok(new List<Organization>());
            return Ok(organizations);
        }

        [HttpGet("get_organization/{id}")]
        [SwaggerOperation(Summary = "Get organization by ID", Description = "Retrieve a single organization by its unique ID.")]
        public async Task<ActionResult<Organization>> GetById(string id)
        {
            var organization = await _organizationService.GetById(id);
            if (organization == null)
                return NotFound();
            return Ok(organization);
        }

        [HttpPost("create_organization")]
        [SwaggerOperation(Summary = "Create new organization", Description = "Create a new organization and return its ID.")]
        public async Task<ActionResult<int>> Create([FromBody] OrganizationCreateDTO dto)
        {
            if (dto == null)
                return BadRequest();

            var organization = new Organization
            {
                OrganizationName = dto.OrganizationName,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null,
                DeleteAt = null
            };

            var result = await _organizationService.Create(organization);
            if (result == 0)
                return BadRequest();
            return Ok(result);
        }

        [HttpPut("update_organization/{id}")]
        [SwaggerOperation(Summary = "Update organization", Description = "Update an existing organization by ID.")]
        public async Task<ActionResult<int>> Update(string id, [FromBody] OrganizationUpdateDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(id))
                return BadRequest();

            var existingOrg = await _organizationService.GetById(id);
            if (existingOrg == null)
                return NotFound();

            existingOrg.OrganizationName = dto.OrganizationName;
            existingOrg.Email = dto.Email;
            existingOrg.Phone = dto.Phone;
            existingOrg.Address = dto.Address;
            existingOrg.UpdatedAt = DateTime.UtcNow;

            var result = await _organizationService.Update(existingOrg);
            if (result == 0)
                return BadRequest();
            return Ok(result);
        }

        [HttpDelete("delete_organization/{id}")]
        [SwaggerOperation(Summary = "Delete organization", Description = "Delete an organization by its ID.")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            var result = await _organizationService.Delete(id);
            if (!result)
                return NotFound();
            return Ok(result);
        }
    }
}
