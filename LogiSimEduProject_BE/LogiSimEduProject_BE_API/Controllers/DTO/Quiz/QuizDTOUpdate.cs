namespace LogiSimEduProject_BE_API.Controllers.DTO.Account
{
    public class QuizDTOUpdate
    {
        public Guid LessonId { get; set; }
        public string QuizName { get; set; }
        public double? TotalScore { get; set; }
        public string Status { get; set; }
    }
}
