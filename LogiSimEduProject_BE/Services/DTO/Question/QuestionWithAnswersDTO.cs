using Services.DTO.Answer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Question
{
    public class QuestionWithAnswersDTO
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public List<AnswerDTO> Answers { get; set; } = new();
    }
}
