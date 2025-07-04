using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services
{
    public class ChatAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ChatAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetChatResponse(string userMessage)
        {
            var apiKey = _config["HuggingFace:ApiKey"];
            var url = "https://api-inference.huggingface.co/models/EleutherAI/gpt-neo-125M";
            var requestData = new
            {
                inputs = $"User: {userMessage}\nAI:"
            };

            var json = JsonSerializer.Serialize(requestData);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("== HuggingFace Response ==");
            Console.WriteLine(responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return $"Lỗi HTTP {response.StatusCode}: {responseContent}";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseContent);

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var generatedText = doc.RootElement[0].GetProperty("generated_text").GetString();
                    return generatedText?.Replace($"User: {userMessage}\nAI:", "").Trim() ?? "Không có phản hồi.";
                }
                else if (doc.RootElement.TryGetProperty("error", out var errorMsg))
                {
                    return "Lỗi từ HuggingFace: " + errorMsg.GetString();
                }
            }
            catch (Exception ex)
            {
                return "Lỗi JSON: " + ex.Message;
            }

            return "Không có phản hồi từ AI.";
        }
    }
}
