using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Account
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [RegularExpression(@"^\d{20}$",
        ErrorMessage = "����� ����� ������ ��������� 20 ����")]
    public string AccountNumber { get; set; }
    public decimal Balance { get; set; } = 0;
    public decimal? TransactionLimit { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public AccountType Type { get; set; }

    [Required]
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LockedUntil { get; set; }

    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; }

    [InverseProperty("Account")]
    public ICollection<Card> Cards { get; set; }

    [InverseProperty("ToAccount")]
    public ICollection<Transaction> ReceivedTransactions { get; set; }

    [InverseProperty("FromAccount")]
    public ICollection<Transaction> GivenTransactions { get; set; }
}