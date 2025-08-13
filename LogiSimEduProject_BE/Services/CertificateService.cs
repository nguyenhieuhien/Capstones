using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Models;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CertificateService : ICertificateService
    {
        private readonly CertificateRepository _repository;

        public CertificateService(CertificateRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Certificate>> GetAll() => await _repository.GetAll();

        public async Task<Certificate> GetById(string id) => await _repository.GetByIdAsync(id);

        public async Task<Stream?> DownloadCertificateAsync(string certificateId)
        {
            var certificate = await _repository.GetByIdAsync(certificateId);

            if (certificate == null || string.IsNullOrEmpty(certificate.FileUrl))
                return null;

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(certificate.FileUrl);
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Cannot download file from URL: {certificate.FileUrl} - Status: {response.StatusCode}");

                var ms = new MemoryStream();
                await response.Content.CopyToAsync(ms);
                ms.Position = 0;
                return ms;
            }
        }
    }
}
