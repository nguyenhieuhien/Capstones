using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
namespace LogiSimEduProject_BE_API.Controllers.DTO.Quiz
{
    public class QuizDTO
    {
        public Guid LessonId { get; set; }
        public string QuizName { get; set; }
        public double TotalScore { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}
