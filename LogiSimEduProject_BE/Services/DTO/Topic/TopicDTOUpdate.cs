using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Services.DTO.Topic
{
    public class TopicDTOUpdate
    {
        public Guid CourseId { get; set; }
        public string TopicName { get; set; }
        public int OrderIndex { get; set; }
        [JsonIgnore]
        public IFormFile ImgUrl { get; set; }
        public string Description { get; set; }
    }
}
