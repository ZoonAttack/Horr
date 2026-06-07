using MediatR;
using ServiceContracts.DTOs.Proposal;
using Services;
using ServiceContracts.DTOs.Responses;

namespace ServiceImplementation.Implementations.Proposals
{
    public class GetProposalsForJobQuery : IRequest<Result<PagedResult<ProposalSummaryForClientDto>>>
    {
        public string JobId { get; set; }
        public string ClientId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public GetProposalsForJobQuery(string jobId, string clientId, int page = 1, int pageSize = 10)
        {
            JobId = jobId;
            ClientId = clientId;
            Page = page;
            PageSize = pageSize;
        }
    }
}
