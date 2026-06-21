using Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Mappings;
using ServiceImplementation.Exceptions;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceContracts.Currency;
using Entities.Users;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class GetJobDetailsQueryHandler : IRequestHandler<GetJobDetailsQuery, Result<JobDetailsDto>>
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyConverterService _currencyConverter;

        public GetJobDetailsQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<Result<JobDetailsDto>> Handle(GetJobDetailsQuery request, CancellationToken cancellationToken)
        {
            User? user = null;
            if (!string.IsNullOrEmpty(request.CurrentUserId))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);
                if (user == null || user.IsDeleted)
                {
                    return new Result<JobDetailsDto>
                    {
                        Succeeded = false,
                        ErrorCode = ErrorCodes.AccountDeleted,
                        Message = "Account not found or is deleted."
                    };
                }
            }

            var jobQuery = _context.JobPosts
                .Include(j => j.Client)
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(j => j.SavedByFreelancers);

            var job = await jobQuery.FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

            if (job == null)
            {
                return new Result<JobDetailsDto>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.JobNotFound,
                    Message = $"Job with ID {request.Id} not found"
                };
            }

            var dto = job.ToDetailsDto(request.CurrentUserId);
            dto.HasApplied = !string.IsNullOrEmpty(request.CurrentUserId) &&
                             await _context.Proposals.AnyAsync(p => p.JobPostId == job.Id && p.FreelancerId == request.CurrentUserId, cancellationToken);

            string targetCurrency = user?.PreferredCurrency ?? "USD";
            if (dto.BudgetCurrency != targetCurrency)
            {
                dto.ConvertedBudget = await _currencyConverter.ConvertAsync(dto.Budget, dto.BudgetCurrency, targetCurrency);
                dto.ConvertedCurrency = targetCurrency;
            }
            else
            {
                dto.ConvertedBudget = dto.Budget;
                dto.ConvertedCurrency = dto.BudgetCurrency;
            }

            // If the requester is the owner, populate stats
            if (request.CurrentUserId == job.ClientId)
            {
                dto.Stats = new JobStatsDto
                {
                    Proposals = await _context.Proposals.CountAsync(p => p.JobPostId == job.Id, cancellationToken),
                    Invited = await _context.JobInvitations.CountAsync(i => i.JobPostId == job.Id, cancellationToken),
                    Hired = await _context.Contracts.CountAsync(c => c.JobPostId == job.Id && 
                        c.Status != Entities.Enums.ContractStatus.Rejected && 
                        c.Status != Entities.Enums.ContractStatus.Terminated, cancellationToken),
                    Messaged = await _context.Conversations.CountAsync(c => c.JobPostId == job.Id, cancellationToken)
                };
            }

            return new Result<JobDetailsDto> { Succeeded = true, Data = dto };
        }
    }
}
