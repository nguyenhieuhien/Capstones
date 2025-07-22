
using Services.DTO.Question;
namespace Services.DTO.Quiz
{
    public class QuizDTO
    {
        public Guid LessonId { get; set; }
        public string QuizName { get; set; }
        public double TotalScore { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}
