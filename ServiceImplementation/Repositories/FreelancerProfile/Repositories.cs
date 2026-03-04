using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities.Users.FreelancerHelpers;

namespace ServiceImplementation.Repositories.FreelancerProfile
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly List<PortfolioItem> _mockDb = new List<PortfolioItem>();

        public async Task<IEnumerable<PortfolioItem>> GetByUserIdAsync(Guid userId)
        {
            return await Task.FromResult(_mockDb.Where(p => p.UserId == userId && !p.IsDeleted));
        }

        public async Task<PortfolioItem> AddAsync(PortfolioItem item)
        {
            _mockDb.Add(item);
            return await Task.FromResult(item);
        }
    }
    
    public class ExperienceRepository : IExperienceRepository
    {
        private readonly List<ProfessionalExperience> _mockDb = new List<ProfessionalExperience>();

        public async Task<IEnumerable<ProfessionalExperience>> GetByUserIdAsync(Guid userId)
        {
            return await Task.FromResult(_mockDb.Where(e => e.UserId == userId && !e.IsDeleted));
        }

        public async Task<ProfessionalExperience> GetByIdAsync(Guid id)
        {
            return await Task.FromResult(_mockDb.FirstOrDefault(e => e.Id == id && !e.IsDeleted));
        }

        public async Task UpdateAsync(ProfessionalExperience experience)
        {
            await Task.CompletedTask;
        }
    }
}
