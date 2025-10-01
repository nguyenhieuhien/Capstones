using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Object
{
    public class ObjectGetDto
    {
        public Guid Id { get; set; }
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public List<MethodGetDto> MethodGets { get; set; }
    }

    public class MethodGetDto
    {
        public Guid Id { get; set; }
        public string MethodName { get; set; }
        public string Description { get; set; }
        public List<ScriptGetDto> ScriptGets { get; set; }
    }

    public class ScriptGetDto
    {
        public Guid Id { get; set; }
        public string ScriptName { get; set; }
        public string Code { get; set; }
    }
}
