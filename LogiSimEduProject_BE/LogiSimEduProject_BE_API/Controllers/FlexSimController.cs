using LogiSimEduProject_BE_API.Controllers.Request;
using Microsoft.AspNetCore.Mvc;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/flexsim")]
    public class FlexSimController : ControllerBase
    {
        private readonly string _downloads = @"C:\Users\karsa\Downloads\TestCodeFlexSim\downloads";
        private readonly string _inbox = @"C:\Users\karsa\Downloads\TestCodeFlexSim\inbox";

        // 1. Phát hành mô hình cho sinh viên tải
        [HttpGet("download-model")]
        public IActionResult DownloadModel()
        {
            var zipPath = Path.Combine(_downloads, "simple_line_model.zip");
            if (!System.IO.File.Exists(zipPath))
                return NotFound("Model zip not found.");
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            return File(bytes, "application/zip", "simple_line_model.zip");
        }



        // 2. Upload file kết quả kpi.csv từ sinh viên
        [HttpPost("upload-results")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadResults([FromForm] UploadResultRequest req)
        {
            if (req.File == null || req.File.Length == 0)
                return BadRequest("No file uploaded.");

            Directory.CreateDirectory(_inbox);
            var fname = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{req.StudentId}_kpi.csv";
            var save = Path.Combine(_inbox, fname);

            using var fs = System.IO.File.Create(save);
            await req.File.CopyToAsync(fs);

            return Ok(new { message = "Uploaded successfully", path = save });
        }
    }
}
