using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.LessonSubmission
{
    public class StudentSubmissionDTO
    {
        public Guid SubmissionId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public double? TotalScore { get; set; }
        public DateTime SubmitTime { get; set; }
    }

    public class ClassSubmissionDTO
    {
        public string ClassName { get; set; }
        public List<StudentSubmissionDTO> Students { get; set; }
    }

}
