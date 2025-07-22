using Microsoft.AspNetCore.Http;

namespace Services.DTO.Message
{
    public class MessageDTOUpdate
    {
        public Guid ConversationId { get; set; }

        public Guid SenderId { get; set; }

        public string MessageType { get; set; }

        public string Content { get; set; }

        public IFormFile? AttachmentUrl { get; set; }

        public bool? IsEdited { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
