using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Users.FreelancerHelpers;

namespace ServiceImplementation.Repositories.FreelancerProfile
{
    public interface IPortfolioRepository
    {
        Task<IEnumerable<PortfolioItem>> GetByUserIdAsync(string userId);
        Task<PortfolioItem> AddAsync(PortfolioItem item);
    }
    
    public interface IExperienceRepository
    {
        Task<IEnumerable<ProfessionalExperience>> GetByUserIdAsync(string userId);
        Task<ProfessionalExperience> GetByIdAsync(string id);
        Task UpdateAsync(ProfessionalExperience experience);
    }
}
