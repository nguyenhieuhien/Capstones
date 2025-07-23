using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.LessonProgress
{
    public class LessonProgressDTO
    {
        public Guid Id { get; set; }

        public Guid? AccountId { get; set; }

        public Guid? LessonId { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }
    }
}
