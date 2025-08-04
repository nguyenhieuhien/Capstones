using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.LessonSubmission
{
    public class LessonSubmissionDTOCreate
    {
        public Guid LessonId { get; set; }
        public Guid AccountId { get; set; }
        public IFormFile FileUrl { get; set; } 
        public string Note { get; set; }
    }
}
