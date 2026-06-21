using Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTOs.JobManagement;
using ServiceImplementation.Mappings;
using ServiceContracts.DTOs.Responses;
using ServiceImplementation.Helpers;
using ServiceContracts.Currency;

namespace ServiceImplementation.Implementations.JobManagement
{
    public class GetSavedJobsQueryHandler : IRequestHandler<GetSavedJobsQuery, Result<SearchJobsQueryResponse>>
    {
        private readonly AppDbContext _context;
        private readonly ICurrencyConverterService _currencyConverter;

        public GetSavedJobsQueryHandler(AppDbContext context, ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<Result<SearchJobsQueryResponse>> Handle(GetSavedJobsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.FreelancerId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<SearchJobsQueryResponse>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var query = _context.SavedJobs
                .Where(sj => sj.FreelancerId == request.FreelancerId)
                .Include(sj => sj.JobPost).ThenInclude(j => j.Client)
                .Include(sj => sj.JobPost).ThenInclude(j => j.Category)
                .Include(sj => sj.JobPost).ThenInclude(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Include(sj => sj.JobPost).ThenInclude(j => j.SavedByFreelancers)
                .OrderByDescending(sj => sj.SavedAt)
                .Select(sj => sj.JobPost);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(j => j.ToSummaryDto(request.FreelancerId)).ToList();
            
            string targetCurrency = user?.PreferredCurrency ?? "USD";
            foreach (var dto in dtos)
            {
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
            }

            var response = new SearchJobsQueryResponse
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return new Result<SearchJobsQueryResponse> { Succeeded = true, Data = response };
        }
    }
}
