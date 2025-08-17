using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Course
{
    public class CourseDTO
    {
        public Guid Id { get; set; }
        public Guid? InstructorId { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? WorkSpaceId { get; set; }

        public string CourseName { get; set; }

        public string Description { get; set; }

        public double? RatingAverage { get; set; }

        public string ImgUrl { get; set; }

        public bool? IsActive { get; set; }
        public string InstructorFullName { get; set; }
    }
}
