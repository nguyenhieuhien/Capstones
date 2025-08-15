namespace Services.DTO.Class
{
    public class ClassDTOCreate
    {
        public Guid CourseId { get; set; }
        public string ClassName { get; set; }
        public Guid? InstructorId { get; set; }
        public int? NumberOfStudent { get; set; }
    }
}
