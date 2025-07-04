using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Models
{
    public class ChatAIRequest
    {
        public string Message { get; set; }
    }
    public class ChatAIResponse
    {
        public string Response { get; set; }
    }
}
