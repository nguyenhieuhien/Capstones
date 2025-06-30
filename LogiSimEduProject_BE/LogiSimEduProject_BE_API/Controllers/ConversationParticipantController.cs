using LogiSimEduProject_BE_API.Controllers.DTO.Conversation;
using LogiSimEduProject_BE_API.Controllers.DTO.ConversationParticipant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationParticipantController : ControllerBase
    {
        private readonly IConversationParticipantService _service;

        public ConversationParticipantController(IConversationParticipantService service) => _service = service;

        [HttpGet("GetAll")]
        public async Task<IEnumerable<ConversationParticipant>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("{id}")]
        public async Task<ConversationParticipant> Get(string id)
        {
            return await _service.GetById(id);
        }


        //[Authorize(Roles = "1")]
        [HttpPost("Create")]
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
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
