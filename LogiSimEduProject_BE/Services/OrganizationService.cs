using Repositories;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services
{
    public interface IOrganizationService
    {
        Task<List<Organization>> GetAll();
        Task<Organization> GetById(string id);
        Task<int> Create(Organization organization);
        Task<int> Update(Organization organization);
        Task<bool> Delete(string id);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly OrganizationRepository _repository;

        public OrganizationService()
        {
            _repository = new OrganizationRepository();
        }

        public async Task<int> Create(Organization organization)
        {
            if (organization == null || string.IsNullOrEmpty(organization.OrganizationName))
            {
                return 0;
            }

            organization.Id = Guid.NewGuid();
            organization.IsActive = true;
            organization.CreatedAt = DateTime.UtcNow;
            organization.UpdatedAt = null;
            organization.DeleteAt = null;

            var result = await _repository.CreateAsync(organization);
            return result;
        }

        public async Task<bool> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            var org = await _repository.GetByIdAsync(id);
            if (org != null)
            {
                org.IsActive = false;
                org.DeleteAt = DateTime.UtcNow;

                var result = await _repository.RemoveAsync(org);
                return result;
            }

            return false;
        }

        public async Task<List<Organization>> GetAll()
        {
            var organizations = await _repository.GetAll();
            return organizations ?? new List<Organization>();
        }

        public async Task<Organization> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var organization = await _repository.GetByIdAsync(id);
            return organization;
        }

        public async Task<int> Update(Organization organization)
        {
            if (organization == null || organization.Id == Guid.Empty || string.IsNullOrEmpty(organization.OrganizationName))
            {
                return 0;
            }

            organization.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(organization);
            return result;
        }
    }
}
