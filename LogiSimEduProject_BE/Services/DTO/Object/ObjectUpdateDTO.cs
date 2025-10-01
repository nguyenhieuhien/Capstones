using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Object
{
    public class ObjectUpdateListDto
    {          // áp dụng cho cả batch
        public List<ObjectUpdateDto> ObjectUpdates { get; set; }
    }

    public class ObjectUpdateDto
    {
        public Guid Id { get; set; }
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public List<MethodUpdateDto> MethodUpdates { get; set; }
    }

    public class MethodUpdateDto
    {
        public Guid? Id { get; set; }  // null = thêm mới
        public string MethodName { get; set; }
        public string Description { get; set; }
        public List<ScriptUpdateDto> ScriptUpdates { get; set; }
    }

    public class ScriptUpdateDto
    {
        public Guid? Id { get; set; }  // null = thêm mới
        public string ScriptName { get; set; }
        public string Code { get; set; }
    }
}
