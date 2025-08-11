using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace LogiSimEduProject_BE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudinaryController : ControllerBase
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryController(CloudinaryDotNet.Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected");

            // (Optional) Giới hạn định dạng được phép
            // var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov", ".pdf", ".docx", ".zip" };
            // if (!allowed.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())) return BadRequest("Unsupported file type");

            await using var stream = file.OpenReadStream();

            // Xác định loại
            var contentType = file.ContentType?.ToLowerInvariant() ?? "";
            var isImage = contentType.StartsWith("image/");
            var isVideo = contentType.StartsWith("video/") || contentType.StartsWith("audio/"); // audio cũng dùng resource_type=video

            UploadResult result;

            if (isImage)
            {
                var p = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "LogiSimEdu_File",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true,
                    // (Optional) Chỉ cho phép một số định dạng ảnh
                    // AllowedFormats = new List<string> { "jpg","jpeg","png","webp","heic","avif","gif","svg" }
                };
                result = await _cloudinary.UploadAsync(p, ct);
            }
            else if (isVideo)
            {
                var p = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "LogiSimEdu_File",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                result = await _cloudinary.UploadAsync(p, ct);
            }
            else
            {
                // Tài liệu, zip, json, csv, pdf, docx, xlsx, v.v. => RAW
                var p = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "LogiSimEdu_File",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                result = await _cloudinary.UploadAsync(p, "auto", ct);
            }

            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // Determine resource type based on file content type
                string resourceType = isImage ? "image" : isVideo ? "video" : "raw";
                return Ok(new
                {
                    Url = result.SecureUrl?.ToString(),
                    PublicId = result.PublicId,
                    ResourceType = resourceType,   // "image" | "video" | "raw"
                    Bytes = result.Bytes,
                    Format = result.Format
                });
            }

            return StatusCode((int)result.StatusCode, result.Error?.Message ?? "Upload failed");
        }
    }
}
