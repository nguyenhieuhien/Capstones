using Repositories;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<(bool Success, string Message, byte[]? FileData, string? FileName)> DownloadCertificateAsync(Guid accountId, Guid courseId)
        {
            var certificateList = await _repository.GetByAccountAndCourse(accountId, courseId);
            if (certificateList == null || !certificateList.Any())
                return (false, "Certificate not found", null, null);

            var cert = certificateList.First();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var fileBytes = await httpClient.GetByteArrayAsync(cert.FileUrl);
                    return (true, "Download successful", fileBytes, $"{cert.CertificateName}.pdf");
                }
                catch (Exception ex)
                {
                    return (false, $"Download failed: {ex.Message}", null, null);
                }
            }
        }
    }
}
