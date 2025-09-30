using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Object
{
    public class ObjectDto
    {
        public Guid ScenarioId { get; set; }
        public string ObjectName { get; set; }
        public string Description { get; set; }
        public List<MethodDto> Methods { get; set; }
    }

    public class MethodDto
    {
        public string MethodName { get; set; }
        public string Description { get; set; }
        public List<ScriptDto> Scripts { get; set; }
    }

    public class ScriptDto
    {
        public string ScriptName { get; set; }
        public string Code { get; set; }
    }

}
