namespace WebApplication1.DTO
{
    public record CardInfoDTO
    (
        string CardNumber,
        string CVVHash,
        DateOnly ExpirationDate,
        string Type,
        string Status,
        DateOnly CreatedAt
    );
}
