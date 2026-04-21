using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? FailureReason { get; set; }

    [Required]
    public decimal Amount { get; set; } = 0;

    [Required, MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public TransactionStatus Status { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Guid FromAccountId { get; set; }

    [ForeignKey("FromAccountId")]
    public Account? FromAccount { get; set; }

    public Guid ToAccountId { get; set; }

    [ForeignKey("ToAccountId")]
    public Account? ToAccount { get; set; }
}