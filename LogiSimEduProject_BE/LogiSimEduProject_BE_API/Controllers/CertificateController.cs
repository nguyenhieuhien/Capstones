using Microsoft.AspNetCore.Mvc;
using Services;
using Services.IServices;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _service;

        public CertificateController(ICertificateService service)
        {
            _service = service;
        }

        [HttpGet("get_all")]
        //[SwaggerOperation(Summary = "Get all categories", Description = "Returns a list of all active categories.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        //[SwaggerOperation(Summary = "Get category by ID", Description = "Retrieve a single category by its unique ID.")]
        public async Task<IActionResult> GetById(string id)
        {
            var certificate = await _service.GetById(id);
            if (certificate == null)
                return NotFound("Certificate not found");
            return Ok(certificate);
        }

        //[HttpGet("download")]
        //public async Task<IActionResult> DownloadCertificate([FromQuery] Guid accountId, [FromQuery] Guid courseId)
        //{
        //    var (success, message, fileData, fileName) = await _service.DownloadCertificateAsync(accountId, courseId);

        //    if (!success)
        //        return NotFound(new { Message = message });

        //    return File(fileData, "application/pdf", fileName);
        //}
    }
}
