using Microsoft.AspNetCore.Mvc;
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
        Task<Certificate?> GetCertificateAsync(string id);
        //Task<Stream?> DownloadCertificateAsync(string certificateId);
        //Task<IActionResult> DownloadPdf(string publicId);
    }
}
