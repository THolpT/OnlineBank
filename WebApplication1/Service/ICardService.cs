using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;

namespace WebApplication1.Service
{
    public interface ICardService
    {
        Task<List<Card>> GetByAccountAsync(Guid accountId);
        Task<Card> CreateAsync(Guid accountId, CardType type);
        Task<bool> BlockAsync(Guid cardId);
    }
}
