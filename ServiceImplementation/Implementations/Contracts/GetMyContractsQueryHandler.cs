using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Entities;
using Entities.Project;
using ServiceContracts.DTOs.Contract;
using Services;
using ServiceImplementation.Helpers;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public class GetMyContractsQueryHandler : IRequestHandler<GetMyContractsQuery, Result<Services.PagedResult<ContractReadDTO>>>
    {
        private readonly AppDbContext _context;
        private readonly ServiceContracts.Currency.ICurrencyConverterService _currencyConverter;

        public GetMyContractsQueryHandler(AppDbContext context, ServiceContracts.Currency.ICurrencyConverterService currencyConverter)
        {
            _context = context;
            _currencyConverter = currencyConverter;
        }

        public async Task<Result<Services.PagedResult<ContractReadDTO>>> Handle(GetMyContractsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return new Result<Services.PagedResult<ContractReadDTO>>
                {
                    Succeeded = false,
                    ErrorCode = ErrorCodes.AccountDeleted,
                    Message = "Account not found or is deleted."
                };
            }

            var query = _context.Contracts.AsQueryable();

            if (request.UserRole == "Client")
            {
                query = query.Where(c => c.ClientId == request.UserId);
            }
            else
            {
                query = query.Where(c => c.FreelancerId == request.UserId);
            }

            if (request.Status.HasValue)
            {
                query = query.Where(c => c.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var dbItems = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new 
                {
                    Id = c.Id,
                    ProposalId = c.ProposalId,
                    ClientId = c.ClientId,
                    FreelancerId = c.FreelancerId,
                    Proposal_Title = c.Proposal != null ? c.Proposal.JobPost.Title : (c.JobPost != null ? c.JobPost.Title : "Direct Offer"),
                    Client_Name = c.Client.FullName,
                    Freelancer_Name = c.Freelancer.FullName,
                    AgreedRate = c.AgreedRate,
                    OriginalCurrency = c.OriginalCurrency ?? "USD",
                    Status = c.Status,
                    StartedAt = c.StartedAt,
                    ClosedAt = c.ClosedAt,
                    CreatedAt = c.CreatedAt,
                    DueDate = c.DueDate,
                    MaxRevisions = c.MaxRevisions,
                    LatestDeliverySummary = c.ContractDeliveries
                        .OrderByDescending(d => d.SubmittedAt)
                        .Select(d => d.DeliveryNote)
                        .FirstOrDefault() ?? c.WorkDeliveries
                        .OrderByDescending(d => d.SubmittedAt)
                        .Select(d => d.Note)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var preferredCurrency = user.PreferredCurrency ?? "USD";
            var items = new List<ContractReadDTO>();

            foreach (var c in dbItems)
            {
                decimal? convertedAgreedRate = null;
                string? convertedCurrency = null;

                try
                {
                    convertedAgreedRate = await _currencyConverter.ConvertAsync(c.AgreedRate, c.OriginalCurrency, preferredCurrency);
                    convertedCurrency = preferredCurrency;
                }
                catch
                {
                    convertedAgreedRate = c.AgreedRate;
                    convertedCurrency = c.OriginalCurrency;
                }

                items.Add(new ContractReadDTO
                {
                    Id = c.Id,
                    ProposalId = c.ProposalId,
                    ClientId = c.ClientId,
                    FreelancerId = c.FreelancerId,
                    Proposal_Title = c.Proposal_Title,
                    Client_Name = c.Client_Name,
                    Freelancer_Name = c.Freelancer_Name,
                    AgreedRate = c.AgreedRate,
                    OriginalCurrency = c.OriginalCurrency,
                    ConvertedAgreedRate = convertedAgreedRate,
                    ConvertedCurrency = convertedCurrency,
                    Status = c.Status,
                    StartedAt = c.StartedAt,
                    ClosedAt = c.ClosedAt,
                    CreatedAt = c.CreatedAt,
                    DueDate = c.DueDate,
                    MaxRevisions = c.MaxRevisions,
                    LatestDeliverySummary = c.LatestDeliverySummary
                });
            }

            var pagedResult = new Services.PagedResult<ContractReadDTO>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return new Result<Services.PagedResult<ContractReadDTO>> { Succeeded = true, Data = pagedResult };
        }
    }
}
