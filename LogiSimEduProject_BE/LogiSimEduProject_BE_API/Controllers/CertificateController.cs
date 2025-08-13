using LogiSimEduProject_BE_API.Controllers.Cloudinary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Services.IServices;
using System.Net.Http.Headers;
using System.Text;

[ApiController]
[Route("api/certificate")]
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

    [HttpGet("open-certificate/{id}")]
    public async Task<IActionResult> OpenCertificate(string id)
    {
        var certificate = await _service.GetCertificateAsync(id);

        if (certificate == null)
            return NotFound("Certificate not found or FileUrl empty");

        if (Request.Headers["Accept"].Any(a => a.Contains("text/html", StringComparison.OrdinalIgnoreCase)))
        {
            return Redirect(certificate.FileUrl); // Redirect để mở PDF luôn
        }

        return Ok(new { fileUrl = certificate.FileUrl });
    }

    //private readonly CloudinarySettings _opt;
    //private readonly HttpClient _http;

    //public CertificateController(IOptions<CloudinarySettings> opt, IHttpClientFactory httpFactory)
    //{
    //    _opt = opt.Value;
    //    _http = httpFactory.CreateClient();
    //}

    // Endpoint chẩn đoán: kiểm tra config & quyền Admin API
    //[HttpGet("diag")]
    //public async Task<IActionResult> Diag()
    //{
    //    var cloud = _opt.CloudName?.Trim();
    //    var key = _opt.ApiKey?.Trim();
    //    var secret = _opt.ApiSecret?.Trim();

    //    if (string.IsNullOrWhiteSpace(cloud) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
    //    {
    //        return new JsonResult(new
    //        {
    //            ok = false,
    //            message = "Cloudinary config is missing (CloudName/ApiKey/ApiSecret)."
    //        });
    //    }

    //    // Admin API list 1 resource để test auth: /resources/{resource_type}/{type}
    //    var testUrl = $"https://api.cloudinary.com/v1_1/{cloud}/resources/raw/upload?max_results=1";
    //    var req = new HttpRequestMessage(HttpMethod.Get, testUrl);
    //    var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{key}:{secret}"));
    //    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

    //    var res = await _http.SendAsync(req);
    //    var body = await res.Content.ReadAsStringAsync();

    //    return new JsonResult(new
    //    {
    //        ok = res.IsSuccessStatusCode,
    //        cloud,
    //        apiKeyMasked = key.Length >= 8 ? $"{key[..4]}...{key[^4..]}" : key,
    //        secretConfigured = !string.IsNullOrEmpty(secret),
    //        status = (int)res.StatusCode,
    //        response = body
    //    });
    //}
    //[HttpGet("download")]
    //public async Task<IActionResult> Download(
    //    [FromQuery] string publicId,
    //    [FromQuery] string resourceType = "raw",
    //    [FromQuery] string type = "upload",
    //    [FromQuery] string? format = "pdf")
    //{
    //    if (string.IsNullOrWhiteSpace(_opt.CloudName) ||
    //        string.IsNullOrWhiteSpace(_opt.ApiKey) ||
    //        string.IsNullOrWhiteSpace(_opt.ApiSecret))
    //        return StatusCode(500, "Cloudinary config is missing.");

    //    var url = $"https://api.cloudinary.com/v1_1/{_opt.CloudName}/{resourceType}/download";

    //    // publicId có sẵn extension?
    //    var hasExt = System.IO.Path.HasExtension(publicId);

    //    var kv = new Dictionary<string, string> {
    //    { "public_id", publicId },
    //    { "type", type }
    //};
    //    if (!hasExt && !string.IsNullOrWhiteSpace(format))
    //        kv["format"] = format; // chỉ gửi khi publicId KHÔNG có đuôi

    //    var form = new FormUrlEncodedContent(kv);

    //    var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = form };
    //    var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_opt.ApiKey.Trim()}:{_opt.ApiSecret.Trim()}"));
    //    req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

    //    var res = await _http.SendAsync(req);
    //    var body = await res.Content.ReadAsStringAsync();
    //    if (!res.IsSuccessStatusCode) return StatusCode((int)res.StatusCode, body);

    //    var json = System.Text.Json.JsonDocument.Parse(body).RootElement;
    //    var downloadUrl = json.GetProperty("url").GetString();

    //    var bytes = await _http.GetByteArrayAsync(downloadUrl!);

    //    // đặt tên file theo đuôi thực tế
    //    var fileName = hasExt
    //        ? System.IO.Path.GetFileName(publicId)
    //        : $"{publicId.Split('/').Last()}.{(format ?? "bin")}";

    //    // đoán content-type tối thiểu cho pdf
    //    var contentType = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
    //        ? "application/pdf" : "application/octet-stream";

    //    return File(bytes, contentType, fileName);
    //}
}