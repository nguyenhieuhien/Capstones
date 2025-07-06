using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LogiSimEduProject_BE_API.Controllers.DTO.Message;
using LogiSimEduProject_BE_API.Controllers.DTO.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LogiSimEduProject_BE_API.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _service;
        private readonly LogiSimEduContext _dbContext;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        public MessageController(LogiSimEduContext dbContext, IMessageService service, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _dbContext = dbContext;
            _service = service;
            _cloudinary = cloudinary;
        }
        [HttpGet("get_all_message")]
        [SwaggerOperation(Summary = "Get all messages", Description = "Retrieve all messages in the system.")]
        public async Task<IEnumerable<Message>> Get()
        {
            return await _service.GetAll();
        }

        [HttpGet("get_message/{id}")]
        [SwaggerOperation(Summary = "Get message by ID", Description = "Retrieve a specific message using its ID.")]
        public async Task<Message> Get(string id)
        {
            return await _service.GetById(id);
        }

        //[Authorize(Roles = "1")]
        [HttpPost("create_message")]
        [SwaggerOperation(Summary = "Create a new message", Description = "Create a new message and store it in the database.")]
        public async Task<IActionResult> Post([FromForm] MessageDTOCreate request)
        {
            string attachmentUrl = null;

            if (request.AttachmentUrl != null)
            {
                await using var stream = request.AttachmentUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.AttachmentUrl.FileName, stream),
                    Folder = "LogiSimEdu_Messages",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    attachmentUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                MessageType = request.MessageType,
                Content = request.Content,
                AttachmentUrl = attachmentUrl,
                IsEdited = request.IsEdited,
                IsDeleted = request.IsDeleted,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _service.Create(message);

            if (result <= 0)
                return BadRequest("Fail Create");

            return Ok(new
            {
                Data = message
            });
        }

        [HttpPost("send_message_one_to_one")]
        [SwaggerOperation(Summary = "Send private message", Description = "Send a one-on-one private message between two users.")]
        public async Task<IActionResult> SendMessageOneToOne([FromForm] SendOneToOneMessageRequest request)
        {
            string attachmentUrl = null;

            if (request.AttachmentUrl != null)
            {
                await using var stream = request.AttachmentUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.AttachmentUrl.FileName, stream),
                    Folder = "LogiSimEdu_Messages",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return StatusCode((int)result.StatusCode, result.Error?.Message);
                }

                attachmentUrl = result.SecureUrl.ToString();
            }

            var message = await _service.SendMessageOneToOne(
                request.SenderId,
                request.ReceiverId,
                request.MessageType,
                request.Content,
                attachmentUrl
            );

            await _dbContext.Entry(message).Reference(m => m.Sender).LoadAsync();
            await _dbContext.Entry(message).Reference(m => m.Conversation).LoadAsync();
            await _dbContext.Entry(message.Conversation)
                .Collection(c => c.ConversationParticipants).Query()
                .Include(cp => cp.Account).LoadAsync();

            var response = new SendOneToOneMessageResponse
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ConversationId = message.ConversationId,
                MessageType = message.MessageType,
                Content = message.Content,
                //AttachmentUrl = attachmentUrl,
                CreatedAt = message.CreatedAt,
                Conversation = new ConversationDto
                {
                    Id = message.Conversation.Id,
                    IsGroup = message.Conversation.IsGroup,
                    Participants = message.Conversation.ConversationParticipants
                .Select(p => new ConversationParticipantDto
                {
                    AccountId = p.AccountId,
                    UserName = p.Account?.UserName,
                    Email = p.Account?.Email
                }).ToList()
                },
                Sender = new SenderDto
                {
                    Id = message.Sender.Id,
                    UserName = message.Sender.UserName,
                    Email = message.Sender.Email
                }
            };

            return Ok(response);
        }

        [HttpPost("send_message_group")]
        [SwaggerOperation(Summary = "Send message to group", Description = "Send a message to all members in a group conversation.")]
        public async Task<IActionResult> SendMessageGroup(SendGroupMessageRequest request)
        {
            string attachmentUrl = null;

            if (request.AttachmentUrl != null)
            {
                await using var stream = request.AttachmentUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.AttachmentUrl.FileName, stream),
                    Folder = "LogiSimEdu_Messages",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    attachmentUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            var message = await _service.SendMessageToGroup(
                request.ConversationId,
                request.SenderId,
                request.MessageType,
                request.Content,
                attachmentUrl
            );

            return Ok(message);
        }

        //[Authorize(Roles = "1")]
        [HttpPut("update_message/{id}")]
        [SwaggerOperation(Summary = "Update message", Description = "Edit an existing message using its ID.")]
        public async Task<IActionResult> Put(string id, [FromForm] MessageDTOUpdate request)
        {
            var existingMessage = await _service.GetById(id);
            if (existingMessage == null)
                return NotFound(new { Message = $"Message with ID {id} was not found." });

            string attachmentUrl = existingMessage.AttachmentUrl;

            if (request.AttachmentUrl != null)
            {
                await using var stream = request.AttachmentUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.AttachmentUrl.FileName, stream),
                    Folder = "LogiSimEdu_Messages",
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    attachmentUrl = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return StatusCode((int)uploadResult.StatusCode, uploadResult.Error?.Message);
                }
            }

            existingMessage.ConversationId = request.ConversationId;
            existingMessage.SenderId = request.SenderId;
            existingMessage.MessageType = request.MessageType;
            existingMessage.AttachmentUrl = attachmentUrl;
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
        [HttpDelete("detele_message/{id}")]
        [SwaggerOperation(Summary = "Delete message", Description = "Delete a message by its ID.")]
        public async Task<bool> Delete(string id)
        {
            return await _service.Delete(id);
        }
    }
}
