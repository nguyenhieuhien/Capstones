using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Repositories;
using Repositories.Models;
using Services.DTO.CertificateTemplete;
using Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class CertificateTemplateService : ICertificateTemplateService
    {
        private readonly CertificateTemplateRepository _repository;
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CertificateTemplateService(CertificateTemplateRepository repository, CloudinaryDotNet.Cloudinary cloudinary)
        {
            _repository = repository;
            _cloudinary = cloudinary;
        }

        public async Task<(bool Success, string Message, Guid? Id)> Create(CertificateTemplateDto request)
        {
            try
            {
                // Upload background image
                string bgUrl = null;
                if (request.BackgroundFile != null)
                {
                    await using var stream = request.BackgroundFile.OpenReadStream();
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(request.BackgroundFile.FileName, stream),
                        Folder = "LogiSimEdu_CertificateTempletes",
                        UseFilename = true,
                        UniqueFilename = false,
                        Overwrite = true
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.StatusCode == HttpStatusCode.OK)
                    {
                        bgUrl = uploadResult.SecureUrl.ToString();
                    }
                    else
                    {
                        return (false, uploadResult.Error?.Message, null);
                    }
                }

                var entity = new CertificateTemplete
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = request.OrganizationId,
                    CourseId = request.CourseId,
                    TemplateName = request.TemplateName,
                    BackgroundUrl = bgUrl,
                    HtmlTemplate = request.HtmlTemplate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _repository.CreateAsync(entity);
                return result > 0
                    ? (true, "Certificate template created successfully", entity.Id)
                    : (false, "Failed to create certificate template", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<CertificateTemplete?> GetById(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<CertificateTemplete>> GetAllByOrgId(Guid organizationId)
        {
            return await _repository.GetAllByOrgIdAsync(organizationId);
        }
    }
}
