using LogiSimEduProject_BE_API.Controllers.Request.ChatRequest;
using Microsoft.AspNetCore.Mvc;
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

    public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["GoogleGemini:ApiKey"]; // 👈 lấy từ appsettings.json
    }


    [HttpPost]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request)
    {
        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = request.Message }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        // ✅ Gửi API key trong header
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

        // ✅ Trích xuất phần trả lời
        using var doc = JsonDocument.Parse(responseString);
        var reply = doc.RootElement
                       .GetProperty("candidates")[0]
                       .GetProperty("content")
                       .GetProperty("parts")[0]
                       .GetProperty("text")
                       .GetString();

        return Ok(new ChatResponse { Reply = reply });
    }
}
