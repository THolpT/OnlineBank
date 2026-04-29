using Microsoft.EntityFrameworkCore;
using WebApplication1.Domains;
using WebApplication1.Domains.Enums;
using WebApplication1.Domains.Models;

namespace WebApplication1.Service
{
    public class AccountService : IAccountService
    {
            private readonly ApplicationDbContext _context;
    private readonly Random _random;

    public AccountService(ApplicationDbContext context)
    {
        _context = context;
        _random = new Random();
    }

    public async Task<Account?> GetByIdAsync(Guid accountId)
    {
        return await _context.Accounts
            .Include(a => a.User)
            .Include(a => a.ReceivedTransactions)
            .Include(a => a.GivenTransactions)
            .FirstOrDefaultAsync(a => a.Id == accountId);
    }

    public async Task<List<Account>> GetUserAccountsAsync(Guid userId)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<Account> CreateAccountAsync(Guid userId, string currency, AccountType type)
    {
        // Check if user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }
        
        if (string.IsNullOrWhiteSpace(currency) || currency.Length > 3)
        {
            throw new ArgumentException("Currency must be a valid 3-letter currency code");
        }
        
        var accountNumber = await GenerateUniqueAccountNumberAsync();

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = accountNumber,
            Balance = 0,
            TransactionLimit = null,
            Currency = currency.ToUpperInvariant(),
            Type = type,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            LockedUntil = null,
            UserId = userId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task<(decimal Balance, string Currency, AccountStatus Status)> GetAccountDetailsAsync(Guid accountId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null)
        {
            throw new ArgumentException($"Account with ID {accountId} not found");
        }

        return (account.Balance, account.Currency, account.Status);
    }

    public async Task<bool> BlockAccountAsync(Guid accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        if (account.Status == AccountStatus.Frozen || account.Status == AccountStatus.Closed)
        {
            return false;
        }

        account.Status = AccountStatus.Frozen;
        account.LockedUntil = null;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnblockAccountAsync(Guid accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        if (account.Status != AccountStatus.Frozen)
        {
            return false;
        }

        account.Status = AccountStatus.Active;
        account.LockedUntil = null;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CloseAccountAsync(Guid accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        if (account.Balance > 0)
        {
            throw new InvalidOperationException("Cannot close account with positive balance. Please withdraw all funds first.");
        }

        if (account.Status == AccountStatus.Closed)
        {
            return false;
        }

        account.Status = AccountStatus.Closed;
        account.LockedUntil = null;

        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetTransactionLimitAsync(Guid accountId, decimal limit)
    {
        if (limit < 0)
        {
            throw new ArgumentException("Transaction limit cannot be negative");
        }

        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Cannot set transaction limit on {account.Status} account");
        }

        account.TransactionLimit = limit;
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveTransactionLimitAsync(Guid accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
        {
            return false;
        }

        if (account.Status != AccountStatus.Active)
        {
            throw new InvalidOperationException($"Cannot remove transaction limit on {account.Status} account");
        }

        account.TransactionLimit = null;
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<string> GenerateUniqueAccountNumberAsync()
    {
        string accountNumber;
        bool isUnique;

        do
        {
            var first18Digits = _random.NextInt64(100000000000000000, 999999999999999999);
            var last2Digits = _random.Next(10, 99);
            accountNumber = $"{first18Digits}{last2Digits:D2}";

            if (accountNumber.Length > 20)
            {
                accountNumber = accountNumber[^20..];
            }
            else if (accountNumber.Length < 20)
            {
                accountNumber = accountNumber.PadLeft(20, '0');
            }

            isUnique = !await _context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
        }
        while (!isUnique);

        return accountNumber;
    }
    }
}
