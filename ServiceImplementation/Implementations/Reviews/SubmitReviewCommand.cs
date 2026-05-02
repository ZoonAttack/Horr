using MediatR;
using ServiceContracts.DTOs.Review;

namespace ServiceImplementation.Implementations.Reviews
{
    public record SubmitReviewCommand(int ContractId, ContractReviewCreateDTO Dto, string ReviewerId) : IRequest<ContractReviewReadDTO>;
}
