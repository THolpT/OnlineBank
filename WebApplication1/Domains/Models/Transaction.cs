using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public string Descrription { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public DateOnly CreatedAt { get; set; }
}