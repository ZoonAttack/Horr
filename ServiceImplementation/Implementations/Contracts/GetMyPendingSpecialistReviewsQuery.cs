using System.Collections.Generic;
using MediatR;
using ServiceContracts.DTOs.Contract;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Contracts
{
    public record GetMyPendingSpecialistReviewsQuery(string SpecialistId)
        : IRequest<Result<List<ContractSpecialistReviewReadDto>>>;
}
