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
    ErrorMessage = "Формат должен быть XXXX-XXXX-XXXX-XXXX, только цифры")]
    public string CardNumber { get; set; }

    [Required]
    public DateOnly ExpirationDate { get; set; }
    [Required, MaxLength(3)]
    public DateOnly CVVHash { get; set; }

    [Required]
    public CardType Type { get; set; }

    [Required]
    public CardStatus Status { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public Guid AccountId { get; set; }

    [ForeignKey("AccountId")]
    public Account Account { get; set; }
}