using MediatR;
using Services;
using ServiceContracts.DTOs.UserDTOs.FreelancerManagement;
using System.Collections.Generic;

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
        int PageSize) : IRequest<PagedResult<FreelancerReadDTO>>;

    public record GetSavedFreelancersQuery(
        string ClientId,
        int Page,
        int PageSize) : IRequest<PagedResult<FreelancerReadDTO>>;
}
