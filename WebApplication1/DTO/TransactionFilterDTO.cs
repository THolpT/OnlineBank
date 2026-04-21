using WebApplication1.Domains.Enums;

namespace WebApplication1.DTO;

public record TransactionFilter
(
    decimal? MinAmount,
    decimal? MaxAmount,
    TransactionStatus? Status,
    string? Currency,
    DateTime? FromDate,
    DateTime? ToDate
);