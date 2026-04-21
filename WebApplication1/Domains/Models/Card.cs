using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Card
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$",
    ErrorMessage = "������ ������ ���� XXXX-XXXX-XXXX-XXXX, ������ �����")]
    public string CardNumber { get; set; }

    [Required]
    public DateOnly ExpirationDate { get; set; }
    [Required, MaxLength(3)]
    public string CVVHash { get; set; }

    [Required]
    public CardType Type { get; set; }

    [Required]
    public CardStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid AccountId { get; set; }

    [ForeignKey("AccountId")]
    public Account Account { get; set; }
}