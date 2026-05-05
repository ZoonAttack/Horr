namespace ServiceContracts.DTOs.UserDTOs
{
    public record ClientOnboardingDto(
        bool EmailVerified,
        bool BillingAdded,
        bool PhoneVerified
    );
}
