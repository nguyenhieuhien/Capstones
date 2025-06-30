namespace LogiSimEduProject_BE_API.Controllers.DTO.Review
{
    public class ReviewCreateDTO
    {

        public Guid AccountId { get; set; }

        public Guid CourseId { get; set; }

        public string Description { get; set; }

        public int? Rating { get; set; }
    }
}
