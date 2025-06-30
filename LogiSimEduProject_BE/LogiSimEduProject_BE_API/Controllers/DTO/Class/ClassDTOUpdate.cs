namespace LogiSimEduProject_BE_API.Controllers.DTO.Class

{
    public class ClassDTOUpdate
    {
        public Guid CourseId { get; set; }
        public string ClassName { get; set; }

        public int? NumberOfStudent { get; set; }
    }
}
