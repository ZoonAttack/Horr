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
                ProjectId = contract.ProjectId,
                ClientId = contract.ClientId,
                FreelancerId = contract.FreelancerId,
                Terms = contract.Terms,
                Status = contract.Status,
                SignedAt = contract.SignedAt,
                CreatedAt = contract.CreatedAt,
                UpdatedAt = contract.UpdatedAt
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
                ProjectId = createDto.ProjectId,
                ClientId = createDto.ClientId,
                FreelancerId = createDto.FreelancerId,
                Terms = createDto.Terms,
                Status = ContractStatus.Draft
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

            if (!string.IsNullOrEmpty(updateDto.Terms))
                contract.Terms = updateDto.Terms;

            contract.Status = updateDto.Status;

            // Set SignedAt when contract is signed
            if (updateDto.Status == ContractStatus.Signed && contract.SignedAt == null)
            {
                contract.SignedAt = DateTime.UtcNow;
            }
        }
    }
}
