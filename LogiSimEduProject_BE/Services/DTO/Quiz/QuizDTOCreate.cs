namespace Services.DTO.Quiz
{
    public class QuizDTOCreate
    {
        public Guid LessonId { get; set; }
        public string QuizName { get; set; }
        public double? TotalScore { get; set; }
        //public string Status { get; set; }

    }
}
