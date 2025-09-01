using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using LogiSimEduProject_BE_API.Controllers.Request.ChatRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;

namespace GeminiChatAPI.Controllers;

[ApiController]
[Route("chat")]
[Authorize] // chỉ cho user đã đăng nhập
public class ChatController : ControllerBase
{
    private const int MAX_HISTORY = 30; // số message gần nhất gửi lên model
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly LogiSimEduContext _db;

    public ChatController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        LogiSimEduContext dbContext)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["GoogleGemini:ApiKey"] ?? "";
        _db = dbContext;
    }

    /// <summary>
    /// Gửi tin nhắn và nhận phản hồi từ Gemini. Lưu toàn bộ theo đúng user đang đăng nhập.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Message))
            return BadRequest(new { error = "Message is required" });

        var userId = GetUserIdFromClaims(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Cannot determine user id from token" });

        if (string.IsNullOrEmpty(_apiKey))
            return StatusCode(500, new { error = "Missing GoogleGemini:ApiKey" });

        // 1) Lưu message của user
        _db.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            Role = "user",               // vai trò của tin nhắn
            Message = request.Message,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        // 2) Lấy lịch sử theo đúng user
        var history = await _db.ChatHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.Timestamp)
            .Take(MAX_HISTORY)
            .OrderBy(h => h.Timestamp)
            .ToListAsync(ct);

        var contents = history.Select(h => new
        {
            role = h.Role,                            // "user" | "model"
            parts = new[] { new { text = h.Message } }
        }).ToArray();

        var payload = new { contents };

        // 3) Gọi Gemini
        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

        var resp = await _httpClient.PostAsync(url, content, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
            return BadRequest(new { status = (int)resp.StatusCode, body });

        // 4) Trích xuất reply an toàn
        string reply = ParseGeminiReply(body);

        // 5) Lưu phản hồi của model
        _db.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            Role = "model",
            Message = reply,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);

        return Ok(new ChatResponse { Reply = reply });
    }

    /// <summary>
    /// Lấy lịch sử chat của chính user đang đăng nhập (kèm roles hiện tại từ token).
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult> GetHistory([FromQuery] int take = 50, [FromQuery] DateTime? before = null, CancellationToken ct = default)
    {
        var userId = GetUserIdFromClaims(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        take = Math.Clamp(take, 1, 200);

        var query = _db.ChatHistories.Where(h => h.UserId == userId);
        if (before.HasValue) query = query.Where(h => h.Timestamp < before.Value);

        var items = await query
            .OrderByDescending(h => h.Timestamp)
            .Take(take)
            .OrderBy(h => h.Timestamp)
            .Select(h => new ChatHistoryDto
            {
                Role = h.Role,              // "user" | "model"
                Message = h.Message,
                Timestamp = (DateTime)h.Timestamp
            })
            .ToListAsync(ct);

        var roles = GetAuthRoles(User);
        return Ok(new { userId, roles, items });
    }

    /// <summary>
    /// Trả về userId và roles của user hiện tại (tiện cho FE debug).
    /// </summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = GetUserIdFromClaims(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { error = "Cannot determine user id from token" });

        var roles = GetAuthRoles(User);
        return Ok(new { userId, roles });
    }

#if DEBUG
    /// <summary>
    /// (DEBUG) Xem toàn bộ claims trong token.
    /// </summary>
    [HttpGet("whoami")]
    public IActionResult WhoAmI() =>
        Ok(User.Claims.Select(c => new { c.Type, c.Value }));
#endif

    // ===== Helpers =====

    // Lấy userId từ nhiều biến thể claim phổ biến, KHÔNG cần đổi Program.cs hay chỗ phát token
    private static string? GetUserIdFromClaims(ClaimsPrincipal user)
    {
        // Firebase / OIDC khác
        var uid = user.FindFirst("uid")?.Value
               ?? user.FindFirst("user_id")?.Value;
        if (!string.IsNullOrEmpty(uid)) return uid;

        // OIDC/JWT chuẩn
        var sub = user.FindFirst("sub")?.Value
               ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!string.IsNullOrEmpty(sub)) return sub;

        // .NET / khi tắt map claims
        var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("nameid")?.Value
                  ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        if (!string.IsNullOrEmpty(nameId)) return nameId;

        // Token cũ tự cấp có thể chỉ có "id" / "UserId"
        var legacy = user.FindFirst("id")?.Value
                  ?? user.FindFirst("UserId")?.Value;
        if (!string.IsNullOrEmpty(legacy)) return legacy;

        // Cuối cùng (ít khuyến nghị): email / Name
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value
            ?? user.Identity?.Name;
    }

    // Lấy roles (quyền hệ thống) từ token
    private static string[] GetAuthRoles(ClaimsPrincipal user)
    {
        var raw = user.Claims
            .Where(c =>
                c.Type == ClaimTypes.Role ||
                c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value);

        return raw
            .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    // Parse an toàn reply của Gemini (tránh vỡ khi thiếu trường)
    private static string ParseGeminiReply(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                return "(No candidates)";

            var cand0 = candidates[0];
            if (!cand0.TryGetProperty("content", out var content)) return "(No content)";
            if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0) return "(No parts)";

            var first = parts[0];
            if (first.TryGetProperty("text", out var textProp))
                return textProp.GetString() ?? "";

            // fallback: nếu part dạng khác
            return first.ToString();
        }
        catch
        {
            return "(Failed to parse model reply)";
        }
    }
}

// DTO trả lịch sử
public class ChatHistoryDto
{
    public string Role { get; set; } = default!;      // "user" | "model"
    public string Message { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}
