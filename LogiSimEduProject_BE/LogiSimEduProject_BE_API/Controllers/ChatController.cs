using LogiSimEduProject_BE_API.Controllers.Request.ChatRequest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GeminiChatAPI.Controllers;

[ApiController]
[Route("chat")]
public class ChatController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly LogiSimEduContext _dbContext;

    public ChatController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        LogiSimEduContext dbContext)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["GoogleGemini:ApiKey"];
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        // Giả sử mỗi user có UserId, nếu chưa có thì gán tạm
        var userId = "guest"; // ← bạn có thể dùng JWT để lấy từ claim

        // 1. Lưu message user vào DB
        _dbContext.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            Role = "user",
            Message = request.Message,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        // 2. Lấy toàn bộ lịch sử của user để gửi lại
        var history = await _dbContext.ChatHistories
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.Timestamp)
            .ToListAsync();

        var contents = history.Select(h => new
        {
            role = h.Role,
            parts = new[] { new { text = h.Message } }
        }).ToArray();

        var payload = new { contents };

        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);

        var response = await _httpClient.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return BadRequest(new
            {
                status = response.StatusCode,
                body = responseString
            });
        }

        // 3. Trích xuất phản hồi từ Gemini
        using var doc = JsonDocument.Parse(responseString);
        var reply = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        // 4. Lưu phản hồi của model vào DB
        _dbContext.ChatHistories.Add(new ChatHistory
        {
            UserId = userId,
            Role = "model",
            Message = reply,
            Timestamp = DateTime.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        return Ok(new ChatResponse { Reply = reply });
    }
}
