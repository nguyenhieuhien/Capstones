using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.Certificate
{
    public class CertificateDTO
    {
        public Guid Id { get; set; }                // Id certificate
        public string FileUrl { get; set; }         // Link file PDF/ảnh certificate
        public DateTime IssuedAt { get; set; }      // Ngày cấp
    }
}
