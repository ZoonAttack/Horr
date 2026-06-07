using MediatR;
using Services;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Collections.Generic;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.ClientImplementation.DiscoveryCQRS
{
    public record SearchFreelancersQuery(
        string? SearchQuery,
        List<string>? SkillIds,
        decimal? MinHourlyRate,
        decimal? MaxHourlyRate,
        int? MinYearsExperience,
        decimal? MinTrustScore,
        bool? IsVerified,
        string? SortBy,
        bool SortDescending,
        int Page,
        int PageSize,
        string? ClientId = null) : IRequest<Result<PagedResult<FreelancerSearchResultDTO>>>;

    public record GetSavedFreelancersQuery(
        string ClientId,
        int Page,
        int PageSize) : IRequest<Result<PagedResult<FreelancerSearchResultDTO>>>;
}
