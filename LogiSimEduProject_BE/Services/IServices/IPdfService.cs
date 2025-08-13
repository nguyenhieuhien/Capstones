using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IPdfService
    {
        byte[] GenerateCertificate(string fullName, string courseName, string backgroundUrl);
    }
}
