using Entities.Enums;

namespace ServiceContracts.DTOs.Contract
{
    public static class ContractExtensions
    {
        /// <summary>
        /// Converts Contract entity to ContractReadDTO
        /// </summary>
        public static ContractReadDTO Contract_To_ContractRead(this Entities.Project.Contract contract)
        {
            if (contract == null)
            {
                return null;
            }

            return new ContractReadDTO
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                ClosedAt = contract.ClosedAt,
                CreatedAt = contract.CreatedAt
            };
        }

        /// <summary>
        /// Converts ContractCreateDTO to Contract entity
        /// </summary>
        public static Entities.Project.Contract ContractCreate_To_Contract(this ContractCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Project.Contract
            {
                ProposalId = createDto.ProposalId,
                ClientId = createDto.ClientId,
                FreelancerId = createDto.FreelancerId,
                AgreedRate = createDto.AgreedRate,
                Status = ContractStatus.Active,
                StartedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Applies ContractUpdateDTO to an existing Contract entity
        /// </summary>
        public static void ContractUpdate_To_Contract(this Entities.Project.Contract contract, ContractUpdateDTO updateDto)
        {
            if (contract == null || updateDto == null)
            {
                return;
            }

            contract.Status = updateDto.Status;

            if (updateDto.AgreedRate.HasValue)
            {
                contract.AgreedRate = updateDto.AgreedRate.Value;
            }

            if (updateDto.Status == ContractStatus.Closed && contract.ClosedAt == null)
            {
                contract.ClosedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Converts Contract entity to ContractDto
        /// </summary>
        public static ContractDto ToDto(this Entities.Project.Contract contract)
        {
            if (contract == null) return null!;

            return new ContractDto
            {
                Id = contract.Id,
                ProposalId = contract.ProposalId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                AgreedRate = contract.AgreedRate,
                Status = contract.Status,
                StartedAt = contract.StartedAt,
                ClosedAt = contract.ClosedAt
            };
        }
    }
}
