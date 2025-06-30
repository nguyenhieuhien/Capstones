namespace LogiSimEduProject_BE_API.Controllers.DTO.Topic
{
    public class TopicDTOUpdate
    {
        public Guid SceneId { get; set; }
        public Guid CourseId { get; set; }
        public string TopicName { get; set; }
        public string ImgUrl { get; set; }
        public string Description { get; set; }
    }
}
