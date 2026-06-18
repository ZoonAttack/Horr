using System;
using MediatR;
using ServiceContracts.DTOs.Contract;
using Entities.Enums;

namespace ServiceImplementation.Implementations.Contracts
{
    public record SubmitHumanSpecialistReviewCommand(
        Guid ReviewId,
        string SpecialistId,
        ReviewVerdict Verdict,
        string ReviewNote
    ) : IRequest<ContractSpecialistReviewReadDto>;
}
