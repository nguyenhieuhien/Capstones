using LogiSimEduProject_BE_API.Controllers.DTO.Question;
using LogiSimEduProject_BE_API.Controllers.DTO.Question;
namespace LogiSimEduProject_BE_API.Controllers.DTO.Quiz
{
    public class QuizDTO
    {
        public Guid TopicId { get; set; }
        public string QuizName { get; set; }
        public double Score { get; set; }
        public List<QuestionDTO> Questions { get; set; }
    }
}
