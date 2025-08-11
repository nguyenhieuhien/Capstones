using Microsoft.AspNetCore.Mvc;
using Services.DTO.CertificateTemplete;
using Services.IServices;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateTemplateController : ControllerBase
    {
        private readonly ICertificateTemplateService _service;

        public CertificateTemplateController(ICertificateTemplateService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CertificateTemplateDto request)
        {
            var result = await _service.Create(request);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var template = await _service.GetById(id);
            return template != null ? Ok(template) : NotFound();
        }

        [HttpGet("org/{organizationId}")]
        public async Task<IActionResult> GetAllByOrgId(Guid organizationId)
        {
            var templates = await _service.GetAllByOrgId(organizationId);
            return Ok(templates);
        }

    }
}
