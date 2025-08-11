using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.QuizSubmission
{
    public class AnswerReviewDTO
    {
        public Guid AnswerId { get; set; }
        public string Description { get; set; }
        public bool IsCorrect { get; set; }
    }
}
