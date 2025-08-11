using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.DTO.CertificateTemplete
{
    public class CertificateTemplateDto
    {
        public Guid? OrganizationId { get; set; }
        public Guid? CourseId { get; set; }
        public string TemplateName { get; set; }
        public IFormFile BackgroundFile { get; set; }
        public string HtmlTemplate { get; set; }
    }
}
