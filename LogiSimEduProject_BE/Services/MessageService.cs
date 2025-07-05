using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.DBContext;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IMessageService
    {
        Task<List<Message>> GetAll();
        Task<Message> GetById(string id);
        Task<int> Create(Message message);
        Task<Message> SendMessageOneToOne(Guid senderId, Guid receiverId, string messageType, string content, string attachmentUrl);
        Task<Message> SendMessageToGroup(Guid conversationId, Guid senderId, string messageType, string content, string attachmentUrl);
        Task<int> Update(Message message);
        Task<bool> Delete(string id);
    }

    public class MessageService : IMessageService
    {
        private MessageRepository _repository;
        private readonly ConversationRepository _conversationRepo;
        private readonly ConversationParticipantRepository _participantRepo;
        private LogiSimEduContext _dbContext;

        public MessageService(LogiSimEduContext dbContext)
        {
            _repository = new MessageRepository();
            _conversationRepo = new ConversationRepository();
            _participantRepo = new ConversationParticipantRepository();
            _dbContext = dbContext;
        }
        public async Task<int> Create(Message message)
        {
            message.Id = Guid.NewGuid();
            return await _repository.CreateAsync(message);
        }
        public async Task<Message> SendMessageOneToOne(Guid senderId, Guid receiverId, string messageType, string content, string attachmentUrl)
        {
            // Tìm cuộc trò chuyện 1-1
            var conversation = await _conversationRepo.FindOneToOneConversation(senderId, receiverId);

            Message message;

            if (conversation == null)
            {
                // Tạo mới conversation
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    IsGroup = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ConversationParticipants = new List<ConversationParticipant>(),
                    Messages = new List<Message>()
                }; 


                conversation.ConversationParticipants.Add(new ConversationParticipant
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    AccountId = senderId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                conversation.ConversationParticipants.Add(new ConversationParticipant
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    AccountId = receiverId,
                    IsActive = true,
                    JoinedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                });

                message = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    SenderId = senderId,
                    MessageType = messageType,
                    Content = content,
                    AttachmentUrl = attachmentUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                conversation.Messages.Add(message);

                await _dbContext.Conversations.AddAsync(conversation);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // conversation có sẵn → chỉ cần tạo message
                message = new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    SenderId = senderId,
                    MessageType = messageType,
                    Content = content,
                    AttachmentUrl = attachmentUrl,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.CreateAsync(message);
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.Entry(message)
                .Reference(m => m.Sender)
                .LoadAsync();

            await _dbContext.Entry(message)
                .Reference(m => m.Conversation)
                .LoadAsync();

            if (message.Conversation != null)
            {
                message.Conversation.ConversationParticipants = await _dbContext.ConversationParticipants
                    .Where(cp => cp.ConversationId == message.Conversation.Id && cp.IsActive == true)
                    .Include(cp => cp.Account)
                    .ToListAsync();
            }

            return message;

        }
        public async Task<Message> SendMessageToGroup(Guid conversationId, Guid senderId, string messageType, string content, string attachmentUrl)
        {
            var conversation = await _conversationRepo.GetByIdAsync(conversationId.ToString());
            if (conversation == null || !conversation.IsGroup)
                throw new Exception("Conversation not found or is not a group");

            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = senderId,
                MessageType = messageType,
                Content = content,
                AttachmentUrl = attachmentUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.CreateAsync(message);

            return message;
        }
        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                return await _repository.RemoveAsync(item);
            }

            return false;
        }

        public async Task<List<Message>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<Message> GetById(string id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> Update(Message message)
        {
            return await _repository.UpdateAsync(message);
        }

    }
}
