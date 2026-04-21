using Library.API.Domains;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Domains;
using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;
using WebApplication1.DTO;

namespace WebApplication1.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task CompleteTransaction(Guid transactionId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null)
                throw new Exception("Транзакция не найдена");

            if (transaction.Status != TransactionStatus.Pending)
                throw new Exception("Транзакция уже завершена");

            var fromAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == transaction.FromAccountId);

            var toAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == transaction.ToAccountId);

            if (fromAccount is null || toAccount is null)
                throw new Exception("Один из счетов не найден");

            if (fromAccount.Balance < transaction.Amount)
            {
                await FailTransaction(transaction, "Недостаточно средств");
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
                return;
            }

            fromAccount.Balance -= transaction.Amount;
            toAccount.Balance += transaction.Amount;

            transaction.Status = TransactionStatus.Completed;
            transaction.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();
        }

        private Task FailTransaction(Transaction transaction, string reason)
        {
            transaction.Status = TransactionStatus.Failed;
            transaction.FailureReason = reason;
            return Task.CompletedTask;
        }

        public async Task<bool> CancelTransaction(Guid transactionId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
            
            if (transaction is null) throw new Exception("Транзакций с таким номером не существует");
            if (transaction.Status == TransactionStatus.Completed || transaction.Status == TransactionStatus.Failed) 
                throw new Exception("Данная транзакция уже завершена");

            transaction.Status = TransactionStatus.Canceled;

            return true;
        }
        
        // Готово
        public async Task<TransactionInfoDTO> GetTransactionInfo(Guid transactionId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction is null) throw new Exception("Транзакций с таким номером не существует");

            var result = transaction.Adapt<TransactionInfoDTO>();

            return result;
        }
        
        // Готово
        public async Task<PagedList<TransactionInfoDTO>> GetTransactionsByAccount(
            Guid accountId,
            TransactionFilter filter,
            int page = 1,
            int pageSize = 10)
        {
            if (await _context.Accounts.FindAsync(accountId) is null) 
                throw new Exception("Данного счёта не существует");

            var query = _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .AsQueryable();
            
            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Currency))
                query = query.Where(t => t.Currency == filter.Currency);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
            
            query = query.OrderByDescending(t => t.CreatedAt);
            
            var result = PagedList<TransactionInfoDTO>.Create(query.Select(t => t.Adapt<TransactionInfoDTO>()), page, pageSize);

            return result;
        }

        // Готово
        public async Task<PagedList<TransactionInfoDTO>> GetTransactionsByUser(
            Guid userId,
            TransactionFilter filter,
            int page = 1,
            int pageSize = 10)
        {
            if (await _context.Users.FindAsync(userId) is null) 
                throw new Exception("Данного пользователя не существует");

            var query = _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccount.UserId == userId || t.ToAccount.UserId == userId)
                .AsQueryable();
            
            if (filter.MinAmount.HasValue)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Currency))
                query = query.Where(t => t.Currency == filter.Currency);

            if (filter.FromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(t => t.CreatedAt <= filter.ToDate.Value);
            
            query = query.OrderByDescending(t => t.CreatedAt);
            
            var result = PagedList<TransactionInfoDTO>.Create(query.Select(t => t.Adapt<TransactionInfoDTO>()), page, pageSize);

            return result;
        }
        
        // Требуются доработки
        public async Task Transfer(Guid fromAccountId, Guid toAccountId, decimal amount, string currency, string? description)
        {
            if (amount <= 0)
                throw new Exception("Сумма должна быть больше 0");

            if (fromAccountId == toAccountId)
                throw new Exception("Нельзя перевести самому себе");

            var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
            var toAccount = await _context.Accounts.FindAsync(toAccountId);

            if (fromAccount is null || toAccount is null)
                throw new Exception("Счет не найден");

            if (fromAccount.Currency != currency || toAccount.Currency != currency)
                throw new Exception("Несовпадение валют");

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                Currency = currency,
                Description = description,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Transfer,
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
