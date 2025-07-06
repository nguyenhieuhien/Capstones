using LogiSimEduProject_BE_API.Controllers.DTO.Conversation;
using LogiSimEduProject_BE_API.Controllers.DTO.ConversationParticipant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/conversationParticipant")]
    [ApiController]
    public class ConversationParticipantController : ControllerBase
    {
        private readonly IConversationParticipantService _service;

        public ConversationParticipantController(IConversationParticipantService service) => _service = service;

        [HttpGet("get_all_conversationParticipant")]
        [SwaggerOperation(Summary = "Get all conversation participants", Description = "Return a list of all participants in conversations.")]
        public async Task<IEnumerable<ConversationParticipant>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_conversationParticipant/{id}")]
        [SwaggerOperation(Summary = "Get participant by ID", Description = "Return a conversation participant by their ID.")]
        public async Task<ConversationParticipant> Get(string id)
        {
            return await _service.GetById(id);
        }


        //[Authorize(Roles = "1")]
        [HttpPost("create_conversationParticipant")]
        [SwaggerOperation(Summary = "Add a participant to a conversation", Description = "Create a new participant entry for a conversation.")]
        public async Task<IActionResult> Post(ConversationParticipantDTOCreate request)
        {
            var conPar = new ConversationParticipant
            {
                ConversationId = request.ConversationId,
                AccountId = request.AccountId,
                LastReadAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(conPar);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_conversationParticipant/{id}")]
        [SwaggerOperation(Summary = "Update a participant", Description = "Update information about a participant in a conversation.")]
        public async Task<IActionResult> Put(string id, ConversationParticipantDTOUpdate request)
        {
            var existingconPar = await _service.GetById(id);
            if (existingconPar == null)
            {
                return NotFound(new { Message = $"ConversationParticipant with ID {id} was not found." });
            }

            existingconPar.AccountId = request.AccountId;
            existingconPar.ConversationId = request.ConversationId;
            existingconPar.JoinedAt = DateTime.UtcNow;
            existingconPar.LastReadAt = DateTime.UtcNow;
            existingconPar.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingconPar);

            return Ok(new
            {
                Message = "ConversationParticipant updated successfully.",
                Data = new
                {
                    AccountId = existingconPar.AccountId,
                    ConversationId = existingconPar.ConversationId,
                    JoinedAt = existingconPar.JoinedAt,
                    LastReadAt = existingconPar.LastReadAt,
                    IsActive = existingconPar.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("delete_conversationParticipant/{id}")]
        [SwaggerOperation(Summary = "Delete a participant", Description = "Remove a participant from a conversation by ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
