using Repositories;
using Repositories.Models;
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

        public async Task<List<Certificate>> GetAll() => await _repository.GetAll();

        public async Task<Certificate> GetById(string id) => await _repository.GetByIdAsync(id);

        //public async Task<(bool Success, string Message, byte[]? FileData, string? FileName)> DownloadCertificateAsync(Guid accountId, Guid courseId)
        //{
        //    var certificateList = await _repository.GetByAccountAndCourse(accountId, courseId);
        //    if (certificateList == null || !certificateList.Any())
        //        return (false, "Certificate not found", null, null);

        //    var cert = certificateList.First();

        //    using (var httpClient = new HttpClient())
        //    {
        //        httpClient.DefaultRequestHeaders.Authorization =
        //        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "YOUR_ACCESS_TOKEN");

        //        try
        //        {
        //            var fileBytes = await httpClient.GetByteArrayAsync(cert.FileUrl);
        //            return (true, "Download successful", fileBytes, $"{cert.CertificateName}.pdf");
        //        }
        //        catch (Exception ex)
        //        {
        //            return (false, $"Download failed: {ex.Message}", null, null);
        //        }
        //    }
        //}
    }
}
