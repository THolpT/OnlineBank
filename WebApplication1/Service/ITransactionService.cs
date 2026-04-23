using Library.API.Domains;
using WebApplication1.DTO;

namespace WebApplication1.Service
{
    public interface ITransactionService
    {
        Task CompleteTransaction(Guid transactionId);
        Task<bool> CancelTransaction(Guid transactionId);
        Task<TransactionInfoDTO> GetTransactionInfo(Guid transactionId);

        Task<PagedList<TransactionInfoDTO>> GetTransactionsByAccount(
            Guid accountId,
            TransactionFilter filter,
            int page = 1,
            int pageSize = 10);

        Task<PagedList<TransactionInfoDTO>> GetTransactionsByUser(
            Guid userId,
            TransactionFilter filter,
            int page = 1,
            int pageSize = 10);
        Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount, string currency, string? description);
        
        Task Deposit(Guid accountId, decimal amount, string currency, string? description);
        
        Task Withdraw(Guid accountId, decimal amount, string currency, string? description);
    }
}
