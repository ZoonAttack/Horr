namespace ServiceContracts.DTOs.UserDTOs
{
    public record ClientMeDto(
        string Id,
        string FirstName,
        string LastName,
        string? AvatarUrl,
        bool HasNotifications
    );
}
