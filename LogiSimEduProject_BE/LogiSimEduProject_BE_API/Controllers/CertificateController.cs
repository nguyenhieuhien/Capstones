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

        [HttpGet("download/{certificateId}")]
        [SwaggerOperation(Summary = "Download certificate file", Description = "Download certificate file from Cloudinary by certificateId")]
        public async Task<IActionResult> DownloadCertificate(string certificateId)
        {
            try
            {
                var fileStream = await _service.DownloadCertificateAsync(certificateId);
                if (fileStream == null)
                    return NotFound("Certificate not found or file not available.");

                // Lấy tên file từ certificate (có thể lưu tên khi upload hoặc tự đặt)
                var fileName = $"{certificateId}.pdf"; // hoặc .jpg, .png tùy định dạng

                return File(fileStream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error downloading file: {ex.Message}");
            }
        }
    }
}
