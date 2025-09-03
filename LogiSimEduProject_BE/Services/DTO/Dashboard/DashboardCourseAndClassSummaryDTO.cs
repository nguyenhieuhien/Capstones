using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Dashboard
{
    public class DashboardCourseAndClassSummaryDTO
    {
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public int TotalStudentsDistinct { get; set; } // DISTINCT AccountId trên toàn hệ thống (theo IsActive/Status filter)
        public List<CourseStudentCountDto> PerCourse { get; set; } = new();
        public List<ClassStudentCountDto> PerClass { get; set; } = new();
        public ChartSeriesDto CourseChart { get; set; } = new();
        public ChartSeriesDto ClassChart { get; set; } = new();
    }

    public class ChartSeriesDto
    {
        public List<string> Labels { get; set; } = new();
        public List<int> Data { get; set; } = new();
    }

    public class CourseStudentCountDto
    {
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public int StudentCount { get; set; } // DISTINCT AccountId theo course
    }

    public class ClassStudentCountDto
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public int StudentCount { get; set; } // DISTINCT AccountId theo class
    }
}
