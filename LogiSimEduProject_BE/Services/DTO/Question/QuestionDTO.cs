
using Services.DTO.Answer;

namespace Services.DTO.Question
{
    public class QuestionDTO
    {
        public string Description { get; set; }
        public List<AnswerDTO> Answers { get; set; }
    }
}
