//using Microsoft.AspNetCore.Mvc;
//using Repositories.Models;
//using Services;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace LogiSimEduProject_BE_API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ChatAIController : ControllerBase
//    {
//        private readonly ChatAIService _chatService;

//        public ChatAIController(ChatAIService chatService)
//        {
//            _chatService = chatService;
//        }
//        [HttpPost]
//        public async Task<IActionResult> Post([FromBody] ChatAIRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.Message))
//                return BadRequest("Message is required.");

//            var aiReply = await _chatService.GetChatResponse(request.Message);

//            return Ok(new ChatAIResponse { Response = aiReply });
//        }

//    }
//}
