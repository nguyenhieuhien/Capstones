using Services.DTO.Answer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.QuizSubmission
{
    public class QuizReviewDTO
    {
        public Guid QuestionId { get; set; }
        public string QuestionDescription { get; set; }
        public List<AnswerReviewDTO> Answers { get; set; }
        public Guid? SelectedAnswerId { get; set; }
    }
}
