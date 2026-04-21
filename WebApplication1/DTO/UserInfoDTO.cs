namespace WebApplication1.DTO
{
    public record class UserInfoDTO
    (
        string FirstName,
        string LastName,
        string MiddleName,
        string Email,
        string Phone,
        string Status,
        DateOnly CreatedAt
    );
}
