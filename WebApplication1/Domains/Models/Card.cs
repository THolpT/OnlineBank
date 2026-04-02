using WebApplication1.Domains.Enums;

namespace WebApplication1.Domains.Models;

public class Card
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string CardNumber { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public DateOnly CVVHash { get; set; }
    public CardType Type { get; set; }
    public CardStatus Status { get; set; }
    public DateOnly CreatedAt { get; set; }
}