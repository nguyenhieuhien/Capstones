using System.Text.Json.Serialization;

namespace LogiSimEduProject_BE_API.Controllers.DTO.Topic
{
    public class TopicDTOCreate
    {
        public Guid CourseId { get; set; }
        public string TopicName { get; set; }
        [JsonIgnore]
        public IFormFile ImgUrl { get; set; }
        public string Description { get; set; }
    }
}
