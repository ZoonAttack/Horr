using MediatR;
using ServiceContracts.DTOs.Review;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Reviews
{
    public record SubmitReviewCommand(int ContractId, ContractReviewCreateDTO Dto, string ReviewerId) : IRequest<Result<ContractReviewReadDTO>>;
}
