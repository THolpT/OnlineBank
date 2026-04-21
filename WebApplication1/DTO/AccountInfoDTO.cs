namespace WebApplication1.DTO
{
    public record AccountInfoDTO
    (
        string AccountNumber,
        decimal Balance,
        string Currency,
        string AccountType,
        string Status,
        DateOnly CreatedAt
    );
}
