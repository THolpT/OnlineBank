using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;

namespace WebApplication1.Service
{
    public interface IAccountService
    {
        Task<Account?> GetByIdAsync(Guid accountId);
        Task<List<Account>> GetUserAccountsAsync(Guid userId);
        Task<Account> CreateAccountAsync(Guid userId, string currency, AccountType type);
        Task<(decimal Balance, string Currency, AccountStatus Status)> GetAccountDetailsAsync(Guid accountId);
        Task<bool> BlockAccountAsync(Guid accountId);
        Task<bool> UnblockAccountAsync(Guid accountId);
        Task<bool> CloseAccountAsync(Guid accountId);
        Task<bool> SetTransactionLimitAsync(Guid accountId, decimal limit);
        Task<bool> RemoveTransactionLimitAsync(Guid accountId);
    }
}
