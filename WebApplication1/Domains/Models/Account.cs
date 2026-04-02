using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Account
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; }
    public AccountType Type { get; set; }
    public AccountStatus Status { get; set; }
    public decimal CreatedAt { get; set; }
}