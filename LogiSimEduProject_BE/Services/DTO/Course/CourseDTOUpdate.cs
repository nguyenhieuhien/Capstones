using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace Services.DTO.Course
{
    public class CourseDTOUpdate
    {
        public Guid? CategoryId { get; set; }

        public Guid? WorkSpaceId { get; set; }

        public Guid? InstructorId { get; set; }

        public string? CourseName { get; set; }

        public string? Description { get; set; }

        public double? RatingAverage { get; set; }

        [JsonIgnore]
        public IFormFile? ImgUrl { get; set; }
    }
}
