using LogiSimEduProject_BE_API.Controllers.DTO.Message;
using LogiSimEduProject_BE_API.Controllers.DTO.Notification;
using Microsoft.AspNetCore.Mvc;
using Repositories.Models;
using Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _service;
        public MessageController(IMessageService service) => _service = service;
        [HttpGet("GetAllMessage")]
        public async Task<IEnumerable<Message>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("GetMessage/{id}")]
        public async Task<Message> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("CreateMessage")]
        public async Task<IActionResult> Post(MessageDTOCreate request)
        {
            var question = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                MessageType = request.MessageType,
                AttachmentUrl = request.AttachmentUrl,
                Content = request.Content,
                IsEdited = request.IsEdited,
                IsDeleted = request.IsDeleted,  
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(question);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = request
            });
        }

        //[Authorize(Roles = "1")]
        [HttpPut("UpdateMessage/{id}")]
        public async Task<IActionResult> Put(string id, MessageDTOUpdate request)
        {
            var existingMessage = await _service.GetById(id);
            if (existingMessage == null)
            {
                return NotFound(new { Message = $"Message with ID {id} was not found." });
            }

            existingMessage.ConversationId = request.ConversationId;
            existingMessage.SenderId = request.SenderId;
            existingMessage.MessageType = request.MessageType;
            existingMessage.AttachmentUrl = request.AttachmentUrl;
            existingMessage.Content = request.Content;
            existingMessage.IsEdited = request.IsEdited;
            existingMessage.IsDeleted = request.IsDeleted;
            existingMessage.UpdatedAt = DateTime.UtcNow;

            await _service.Update(existingMessage);

            return Ok(new
            {
                Message = "Message updated successfully.",
                Data = new
                {
                    ConversationId = existingMessage.ConversationId,
                    SenderId = existingMessage.SenderId,
                    MessageType = existingMessage.MessageType,
                    AttachmentUrl = existingMessage.AttachmentUrl,
                    Content = existingMessage.Content,
                    IsEdited = existingMessage.IsEdited,
                    IsDeleted = existingMessage.IsDeleted,
                    IsActive = existingMessage.IsActive,
                }
            });
        }

        //[Authorize(Roles = "1")]
        [HttpDelete("DeteleMessage/{id}")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
