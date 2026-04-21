namespace WebApplication1.DTO
{
    public record class TransactionInfoDTO
    (
        string Description,
        decimal Amount,
        string Currency,
        string Type,
        string Status,
        string FromAccountNumber,
        string ToAccountNumber,
        DateOnly CreatedAt
    );
}
