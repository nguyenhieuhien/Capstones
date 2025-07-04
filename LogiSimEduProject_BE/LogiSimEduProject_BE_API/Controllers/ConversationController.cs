using LogiSimEduProject_BE_API.Controllers.DTO.Class;
using LogiSimEduProject_BE_API.Controllers.DTO.Conversation;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _service;

        public ConversationController(IConversationService service) => _service = service;

        [HttpGet("GetAllConversation")]
        public async Task<IEnumerable<Conversation>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetConversation/{id}")]
        public async Task<Conversation> Get(string id)
        {
            return await _service.GetById(id);
        }


        //[Authorize(Roles = "1")]
        [HttpPost("CreateConversation")]
        public async Task<IActionResult> Post(ConversationDTOCreate request)
        {
            var course = new Conversation
            {
                //MessageId = request.MessageId,
                IsGroup = request.IsGroup,
                Title = request.Title,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(course);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdateConversation/{id}")]
        public async Task<IActionResult> Put(string id, ConversationDTOUpdate request)
        {
            var existingConversation = await _service.GetById(id);
            if (existingConversation == null)
            {
                return NotFound(new { Message = $"Conversation with ID {id} was not found." });
            }

            //existingConversation.MessageId = request.MessageId;
            existingConversation.IsGroup = request.IsGroup;
            existingConversation.Title = request.Title;
            existingConversation.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingConversation);

            return Ok(new
            {
                Message = "Conversation updated successfully.",
                Data = new
                {
                    MessageId = existingConversation.MessageId,
                    IsGroup = existingConversation.IsGroup,
                    Title = existingConversation.Title,
                    IsActive = existingConversation.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeleteConversation/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
