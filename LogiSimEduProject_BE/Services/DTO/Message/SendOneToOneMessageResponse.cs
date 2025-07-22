using Microsoft.AspNetCore.Http;

namespace Services.DTO.Message
{
    public class SendOneToOneMessageResponse
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public IFormFile? AttachmentUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public ConversationDto Conversation { get; set; }
        public SenderDto Sender { get; set; }
    }

    public class ConversationDto
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public List<ConversationParticipantDto> Participants { get; set; }
    }

    public class ConversationParticipantDto
    {
        public Guid AccountId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class SenderDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
