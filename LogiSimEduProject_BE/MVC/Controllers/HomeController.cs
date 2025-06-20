using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
        {
            var client = _httpClientFactory.CreateClient();

            var requestContent = new StringContent(
                JsonSerializer.Serialize(new { idToken = model.IdToken }),
                Encoding.UTF8,
                "application/json");

            // Thay đổi URL này đúng với Web API endpoint thực tế
            var response = await client.PostAsync("https://localhost:5001/api/Account/GoogleLogin", requestContent);

            if (response.IsSuccessStatusCode)
            {
                var tokenJson = await response.Content.ReadAsStringAsync();
                return Content(tokenJson, "application/json");
            }

            return Unauthorized();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class GoogleLoginModel
        {
            public string IdToken { get; set; }
        }
    }
}
