namespace LogiSimEduProject_BE_API.Controllers.DTO.Course
{
    public class CourseDTOCreate
    {
        public Guid CategoryId { get; set; }

        public Guid WorkSpaceId { get; set; }

        public string CourseName { get; set; }

        public string Description { get; set; }

        public double? RatingAverage { get; set; }

        public string ImgUrl { get; set; }
    }
}
