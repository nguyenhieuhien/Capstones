using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.CourseProgress
{
    public class CourseProgressDTO
    {
        public Guid Id { get; set; }

        public Guid? AccountId { get; set; }

        public Guid? CourseId { get; set; }

        public double? ProgressPercent { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }
    }
}
