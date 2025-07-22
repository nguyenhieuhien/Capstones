using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.IServices
{
    public interface IReviewService
    {
        Task<List<Review>> GetAll();
        Task<Review?> GetById(string id);
        Task<int> Create(Review review);
        Task<int> Update(Review review);
        Task<bool> Delete(string id);
    }
}
