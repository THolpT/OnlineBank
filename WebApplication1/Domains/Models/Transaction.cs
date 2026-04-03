using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    public string Descrription { get; set; }

    [Required]
    public decimal Amount { get; set; } = 0;

    [Required, MaxLength(3)]
    public string Currency { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    public TransactionStatus Status { get; set; }

    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public Guid FromAccountId { get; set; }


    [ForeignKey("FromAccountId")]
    public Account FromAccount { get; set; }

    public Guid ToAccountId { get; set; }

    [ForeignKey("ToAccountId")]
    public Account ToAccount { get; set; }
}