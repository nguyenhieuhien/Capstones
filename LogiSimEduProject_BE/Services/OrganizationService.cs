    // File: Services/OrganizationService.cs
    using Repositories;
    using Repositories.Models;
    using Services.IServices;

    namespace Services
    {

        public class OrganizationService : IOrganizationService
        {
            private readonly OrganizationRepository _repository;

            public OrganizationService()
            {
                _repository = new OrganizationRepository();
            }

            public async Task<List<Organization>> GetAll()
            {
                return await _repository.GetAll() ?? new List<Organization>();
            }

            public async Task<Organization?> GetById(string id)
            {
                return await _repository.GetByIdAsync(id);
            }



            public async Task<(bool Success, string Message, Guid? Id)> Create(Organization organization)
            {
                try
                {
                    organization.Id = Guid.NewGuid();
                    organization.IsActive = false;
                    organization.CreatedAt = DateTime.UtcNow;
                    organization.UpdatedAt = null;
                    organization.DeleteAt = null;

                    var result = await _repository.CreateAsync(organization);
                    if (result > 0)
                        return (true, "Organization created successfully", organization.Id);
                    return (false, "Failed to create organization", null);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message, null);
                }
            }

            public async Task<(bool Success, string Message)> Update(Organization organization)
            {
                try
                {
                    organization.UpdatedAt = DateTime.UtcNow;
                    var result = await _repository.UpdateAsync(organization);
                    if (result > 0)
                        return (true, "Organization updated successfully");
                    return (false, "Failed to update organization");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        public async Task<bool> UpdateActiveAsync(Guid organizationId, bool isActive)
        {
            // repo đang dùng string id, convert từ Guid -> string
            var org = await _repository.GetByIdAsync(organizationId.ToString());
            if (org == null) return false;

            if (org.IsActive == isActive) return true; // idempotent
            org.IsActive = isActive;
            org.UpdatedAt = DateTime.UtcNow;

            return (await _repository.UpdateAsync(org)) > 0;
        }


        public async Task<(bool Success, string Message)> Delete(string id)
            {
                try
                {
                    var organization = await _repository.GetByIdAsync(id);
                    if (organization == null)
                        return (false, "Organization not found");

                    organization.IsActive = false;
                    organization.DeleteAt = DateTime.UtcNow;

                    var result = await _repository.RemoveAsync(organization);
                    if (result)
                        return (true, "Organization deleted successfully");
                    return (false, "Failed to delete organization");
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }
    }
