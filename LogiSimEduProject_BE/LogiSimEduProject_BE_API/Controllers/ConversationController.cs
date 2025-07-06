using LogiSimEduProject_BE_API.Controllers.DTO.Class;
using LogiSimEduProject_BE_API.Controllers.DTO.Conversation;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/conversation")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _service;

        public ConversationController(IConversationService service) => _service = service;

        [HttpGet("get_all_conversation")]
        [SwaggerOperation(Summary = "Get all conversations", Description = "Return a list of all conversations.")]
        public async Task<IEnumerable<Conversation>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_conversation/{id}")]
        [SwaggerOperation(Summary = "Get conversation by ID", Description = "Return a specific conversation by its ID.")]
        public async Task<Conversation> Get(string id)
        {
            return await _service.GetById(id);
        }


        //[Authorize(Roles = "1")]
        [HttpPost("create_conversation")]
        [SwaggerOperation(Summary = "Create a conversation", Description = "Create a new conversation (group or private).")]
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
        [HttpPut("update_conversation/{id}")]
        [SwaggerOperation(Summary = "Update a conversation", Description = "Update a conversation's group status or title.")]
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
        [HttpDelete("delete_conversation/{id}")]
        [SwaggerOperation(Summary = "Delete a conversation", Description = "Delete a conversation using its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
