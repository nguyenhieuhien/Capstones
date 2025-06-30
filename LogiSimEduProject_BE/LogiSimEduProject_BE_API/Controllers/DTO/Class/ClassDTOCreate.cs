namespace LogiSimEduProject_BE_API.Controllers.DTO.Class
{
    public class ClassDTOCreate
    {
        public Guid CourseId { get; set; }
        public string ClassName { get; set; }

        public int? NumberOfStudent { get; set; }
    }
}
