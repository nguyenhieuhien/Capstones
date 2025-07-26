//using Microsoft.Extensions.Configuration;
//using Services.DTO.Order;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Services
//{
//    public class PaymentService
//    {
//        private readonly HttpClient _http;
//        private readonly IConfiguration _config;

//        public PaymentService(HttpClient http, IConfiguration config)
//        {
//            _http = http;
//            _config = config;
//        }

//        public async Task<string> CreatePaymentLinkAsync(CreatePayOSOrderRequest request)
//        {
//            var apiUrl = $"{_config["PayOS:BaseUrl"]}/payment-requests";

//            var body = new
//            {
//                orderCode = request.OrderCode,
//                amount = request.Amount,
//                description = request.Description,
//                returnUrl = request.ReturnUrl,
//                cancelUrl = request.CancelUrl
//            };

//            var req = new HttpRequestMessage(HttpMethod.Post, apiUrl);
//            req.Headers.Add("x-client-id", _config["PayOS:ClientId"]);
//            req.Headers.Add("x-api-key", _config["PayOS:ApiKey"]);
//            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

//            var res = await _http.SendAsync(req);
//            var content = await res.Content.ReadAsStringAsync();

//            if (!res.IsSuccessStatusCode)
//                throw new Exception("PayOS payment failed: " + content);

//            var json = JsonSerializer.Deserialize<JsonElement>(content);
//            return json.GetProperty("checkoutUrl").GetString(); // ✅ URL thật
//        }
//    }

//}
