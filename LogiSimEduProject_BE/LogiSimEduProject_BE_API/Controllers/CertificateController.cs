using Microsoft.AspNetCore.Mvc;
using Services.IServices;

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

        [HttpGet("certificate/download")]
        public async Task<IActionResult> DownloadCertificate([FromQuery] Guid accountId, [FromQuery] Guid courseId)
        {
            var (success, message, fileData, fileName) = await _service.DownloadCertificateAsync(accountId, courseId);

            if (!success)
                return NotFound(new { Message = message });

            return File(fileData, "application/pdf", fileName);
        }
    }
}
