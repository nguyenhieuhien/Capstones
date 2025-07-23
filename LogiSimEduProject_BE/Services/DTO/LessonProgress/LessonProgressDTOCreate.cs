using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.LessonProgress
{
    public class LessonProgressDTOCreate
    {

        public Guid? AccountId { get; set; }

        public Guid? LessonId { get; set; }

        public int? Status { get; set; }
    }
}
