using Repositories.Models;
using Services.DTO.CertificateTemplete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ICertificateTemplateService
    {
        Task<List<CertificateTemplete>> GetAll();
        Task<(bool Success, string Message, Guid? Id)> Create(CertificateTemplateDto request);
        Task<CertificateTemplete?> GetById(Guid id);
        Task<List<CertificateTemplete>> GetAllByOrgId(Guid organizationId);
    }
}
