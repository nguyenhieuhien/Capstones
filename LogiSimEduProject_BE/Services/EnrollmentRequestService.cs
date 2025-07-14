using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IEnrollmentRequestService
    {
        Task<List<EnrollmentRequest>> GetAll();
        Task<EnrollmentRequest> GetById(string id);
        Task<List<EnrollmentRequest>> GetByCourseId(string courseId);
        Task<int> Create(EnrollmentRequest request);
        Task<int> Update(EnrollmentRequest request);
        Task<bool> Delete(string id);
    }

    public class EnrollmentRequestService : IEnrollmentRequestService
    {
        private EnrollmentRequestRepository _repository;

        public EnrollmentRequestService()
        {
            _repository = new EnrollmentRequestRepository();
        }

        public async Task<int> Create(EnrollmentRequest request)
        {
            request.Id = Guid.NewGuid();
            request.StatusId = 1;
            request.RequestedAt = DateTime.UtcNow;
            return await _repository.CreateAsync(request);
        }

        public async Task<bool> Delete(string id)
        {
            var item = await _repository.GetById(id);
            if (item != null)
            {
                return await _repository.RemoveAsync(item);
            }
            return false;
        }

        public async Task<List<EnrollmentRequest>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<EnrollmentRequest> GetById(string id)
        {
            return await _repository.GetById(id);
        }

        public async Task<List<EnrollmentRequest>> GetByCourseId(string courseId)
        {
            return await _repository.GetByCourseId(courseId);
        }

        public async Task<int> Update(EnrollmentRequest request)
        {
            return await _repository.UpdateAsync(request);
        }
    }
}
