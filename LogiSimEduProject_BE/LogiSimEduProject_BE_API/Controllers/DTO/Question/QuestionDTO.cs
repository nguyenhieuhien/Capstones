using LogiSimEduProject_BE_API.Controllers.DTO.Answer;

namespace LogiSimEduProject_BE_API.Controllers.DTO.Question
{
    public class QuestionDTO
    {
        public string Description { get; set; }
        public List<AnswerDTO> Answers { get; set; }
    }
}
