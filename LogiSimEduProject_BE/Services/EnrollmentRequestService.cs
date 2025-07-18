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
        Task<List<AccountOfCourse>> GetAll();
        Task<AccountOfCourse> GetById(string id);
        Task<List<AccountOfCourse>> GetByCourseId(string courseId);
        Task<int> Create(AccountOfCourse request);
        Task<int> Update(AccountOfCourse request);
        Task<bool> Delete(string id);
    }

    public class EnrollmentRequestService : IEnrollmentRequestService
    {
        private EnrollmentRequestRepository _repository;

        public EnrollmentRequestService()
        {
            _repository = new EnrollmentRequestRepository();
        }

        public async Task<int> Create(AccountOfCourse request)
        {
            request.Id = Guid.NewGuid();
            request.Status = 1;
            request.CreatedAt = DateTime.UtcNow;
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

        public async Task<List<AccountOfCourse>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<AccountOfCourse> GetById(string id)
        {
            return await _repository.GetById(id);
        }

        public async Task<List<AccountOfCourse>> GetByCourseId(string courseId)
        {
            return await _repository.GetByCourseId(courseId);
        }

        public async Task<int> Update(AccountOfCourse request)
        {
            return await _repository.UpdateAsync(request);
        }
    }
}
