using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface ICertificateService
    {
        Task<List<Certificate>> GetAll();
        Task<Certificate?> GetById(string id);
        //Task<(bool Success, string Message, byte[]? FileData, string? FileName)> DownloadCertificateAsync(Guid accountId, Guid courseId);
    }
}
