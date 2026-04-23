using Library.API.Domains;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Domains;
using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;
using WebApplication1.DTO;
using WebApplication1.Utils;

namespace WebApplication1.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly Converter _converter = new Converter();
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task CompleteTransaction(Guid transactionId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {

                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Id == transactionId);

                if (transaction is null)
                    throw new Exception("Транзакция не найдена");

                if (transaction.Status != TransactionStatus.Pending)
                    throw new Exception("Транзакция уже завершена");

                var fromAccount = transaction.FromAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == transaction.FromAccountId.Value)
                    : null;

                var toAccount = transaction.ToAccountId.HasValue
                    ? await _context.Accounts.FirstOrDefaultAsync(a => a.Id == transaction.ToAccountId.Value)
                    : null;
                
                Console.WriteLine($"TYPE: {transaction.Type}");
                Console.WriteLine($"FROM: {transaction.FromAccountId}");
                Console.WriteLine($"TO: {transaction.ToAccountId}");

                switch (transaction.Type)
                {
                    case TransactionType.Transfer:
                        if (fromAccount is null || toAccount is null)
                            throw new Exception("Счета не найдены");

                        var amountFrom = await _converter.ConvertCurrency(transaction.Amount, transaction.Currency,
                            fromAccount.Currency);
                        var amountTo = await _converter.ConvertCurrency(transaction.Amount, transaction.Currency,
                            toAccount.Currency);

                        if (fromAccount.Balance < amountFrom)
                        {
                            await FailTransaction(transaction, "Недостаточно средств");
                            await _context.SaveChangesAsync();
                            await dbTransaction.CommitAsync();
                            return;
                        }

                        fromAccount.Balance -= amountFrom;
                        toAccount.Balance += amountTo;
                        break;

                    case TransactionType.Deposit:
                        if (toAccount is null)
                            throw new Exception("Счет не найден");
                        
                        Console.WriteLine($"AMOUNT: {transaction.Amount}");
                        Console.WriteLine($"FROM CUR: {transaction.Currency}");
                        Console.WriteLine($"TO CUR: {toAccount?.Currency}");

                        toAccount.Balance += await _converter.ConvertCurrency(transaction.Amount, transaction.Currency,
                            toAccount.Currency);
                        break;

                    case TransactionType.Withdraw:
                        if (fromAccount is null)
                            throw new Exception("Счет не найден");

                        var withdrawAmount = await _converter.ConvertCurrency(transaction.Amount, transaction.Currency,
                            fromAccount.Currency);

                        if (fromAccount.Balance < withdrawAmount)
                        {
                            await FailTransaction(transaction, "Недостаточно средств");
                            await _context.SaveChangesAsync();
                            await dbTransaction.CommitAsync();
                            return;
                        }

                        fromAccount.Balance -= withdrawAmount;
                        break;
                }

                if (transaction.Status != TransactionStatus.Failed)
                {
                    transaction.Status = TransactionStatus.Completed;
                    transaction.CompletedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"Transaction failed: {ex.Message}", ex);
            }
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

        public async Task Deposit(Guid accountId, decimal amount, string currency, string? description)
        {
            if (amount <= 0)
                throw new Exception("Сумма должна быть больше 0");

            var account = await _context.Accounts.FindAsync(accountId);

            if (account is null)
                throw new Exception("Счет не найден");

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                Currency = currency,
                Description = description,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Deposit,
                ToAccountId = accountId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task Withdraw(Guid accountId, decimal amount, string currency, string? description)
        {
            if (amount <= 0)
                throw new Exception("Сумма должна быть больше 0");

            var account = await _context.Accounts.FindAsync(accountId);

            if (account is null)
                throw new Exception("Счет не найден");

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = amount,
                Currency = currency,
                Description = description,
                Status = TransactionStatus.Pending,
                Type = TransactionType.Withdraw,
                FromAccountId = accountId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
