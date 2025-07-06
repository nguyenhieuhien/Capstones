using Swashbuckle.AspNetCore.Annotations;

namespace LogiSimEduProject_BE_API.Controllers.DTO.Message
{
    public class SendOneToOneMessageRequest
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }

        [SwaggerSchema(Format = "binary")]
        public IFormFile? AttachmentUrl { get; set; }
    }
}
