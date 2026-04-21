using Microsoft.AspNetCore.Mvc;
using WebApplication1.Domains.Enums;
using WebApplication1.DTO;
using WebApplication1.Service;


namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankingController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IUserService _userService;

        public BankingController(
            IAccountService accountService,
            ITransactionService transactionService,
            IUserService userService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _userService = userService;
        }

        // ==================== USER ENDPOINTS ====================

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var user = await _userService.CreateAsync(dto);
                return Ok(new { user.Id, user.Email, user.FirstName, user.LastName });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.MiddleName,
                user.Email,
                user.Phone,
                user.Status,
                Accounts = user.Accounts?.Select(a => new { a.Id, a.AccountNumber, a.Balance, a.Currency, a.Type, a.Status })
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users.Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.Status }));
        }

        [HttpPut("users/{userId:guid}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto dto)
        {
            var user = await _userService.UpdateAsync(userId, dto);
            if (user == null)
                return NotFound(new { error = "User not found" });

            return Ok(new { user.Id, user.FirstName, user.LastName, user.Email, user.Status });
        }

        [HttpDelete("users/{userId:guid}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            var result = await _userService.DeleteAsync(userId);
            if (!result)
                return NotFound(new { error = "User not found" });

            return NoContent();
        }

        // ==================== ACCOUNT ENDPOINTS ====================

        [HttpGet("accounts/{accountId:guid}")]
        public async Task<IActionResult> GetAccount(Guid accountId)
        {
            var account = await _accountService.GetByIdAsync(accountId);
            if (account == null)
                return NotFound(new { error = "Account not found" });

            return Ok(new
            {
                account.Id,
                account.AccountNumber,
                account.Balance,
                account.Currency,
                account.Type,
                account.Status,
                account.CreatedAt,
                account.TransactionLimit,
                User = new { account.User?.Id, account.User?.Email, account.User?.FirstName, account.User?.LastName },
                Cards = account.Cards?.Select(c => new { c.Id, c.CardNumber, c.Type, c.Status })
            });
        }

        [HttpGet("users/{userId:guid}/accounts")]
        public async Task<IActionResult> GetUserAccounts(Guid userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound(new { error = "User not found" });

            var accounts = await _accountService.GetUserAccountsAsync(userId);
            return Ok(accounts.Select(a => new
            {
                a.Id,
                a.AccountNumber,
                a.Balance,
                a.Currency,
                a.Type,
                a.Status,
                a.CreatedAt,
                a.TransactionLimit
            }));
        }

        [HttpPost("users/{userId:guid}/accounts")]
        public async Task<IActionResult> CreateAccount(
            Guid userId,
            [FromQuery] string currency,
            [FromQuery] AccountType type)
        {
            try
            {
                var account = await _accountService.CreateAccountAsync(userId, currency, type);
                return Ok(new
                {
                    account.Id,
                    account.AccountNumber,
                    account.Balance,
                    account.Currency,
                    account.Type,
                    account.Status,
                    account.CreatedAt
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("accounts/{accountId:guid}/details")]
        public async Task<IActionResult> GetAccountDetails(Guid accountId)
        {
            try
            {
                var (balance, currency, status) = await _accountService.GetAccountDetailsAsync(accountId);
                return Ok(new { balance, currency, status });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("accounts/{accountId:guid}/block")]
        public async Task<IActionResult> BlockAccount(Guid accountId)
        {
            var result = await _accountService.BlockAccountAsync(accountId);
            if (!result)
                return BadRequest(new { error = "Cannot block account. Account may not exist or is already frozen/closed" });

            return Ok(new { message = "Account blocked successfully" });
        }

        [HttpPost("accounts/{accountId:guid}/unblock")]
        public async Task<IActionResult> UnblockAccount(Guid accountId)
        {
            var result = await _accountService.UnblockAccountAsync(accountId);
            if (!result)
                return BadRequest(new { error = "Cannot unblock account. Account may not exist or is not frozen" });

            return Ok(new { message = "Account unblocked successfully" });
        }

        [HttpPost("accounts/{accountId:guid}/close")]
        public async Task<IActionResult> CloseAccount(Guid accountId)
        {
            try
            {
                var result = await _accountService.CloseAccountAsync(accountId);
                if (!result)
                    return BadRequest(new { error = "Cannot close account" });

                return Ok(new { message = "Account closed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("accounts/{accountId:guid}/transaction-limit")]
        public async Task<IActionResult> SetTransactionLimit(Guid accountId, [FromQuery] decimal limit)
        {
            try
            {
                var result = await _accountService.SetTransactionLimitAsync(accountId, limit);
                if (!result)
                    return NotFound(new { error = "Account not found" });

                return Ok(new { message = $"Transaction limit set to {limit}" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("accounts/{accountId:guid}/transaction-limit")]
        public async Task<IActionResult> RemoveTransactionLimit(Guid accountId)
        {
            try
            {
                var result = await _accountService.RemoveTransactionLimitAsync(accountId);
                if (!result)
                    return NotFound(new { error = "Account not found" });

                return Ok(new { message = "Transaction limit removed successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ==================== TRANSACTION ENDPOINTS ====================

        [HttpPost("transactions/transfer")]
        public async Task<IActionResult> Transfer(
            [FromQuery] Guid fromAccountId,
            [FromQuery] Guid toAccountId,
            [FromQuery] decimal amount,
            [FromQuery] string currency,
            [FromQuery] string? description = null)
        {
            try
            {
                await _transactionService.Transfer(fromAccountId, toAccountId, amount, currency, description);
                return Ok(new { message = "Transfer initiated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transactions/{transactionId:guid}/complete")]
        public async Task<IActionResult> CompleteTransaction(Guid transactionId)
        {
            try
            {
                await _transactionService.CompleteTransaction(transactionId);
                return Ok(new { message = "Transaction completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transactions/{transactionId:guid}/cancel")]
        public async Task<IActionResult> CancelTransaction(Guid transactionId)
        {
            try
            {
                var result = await _transactionService.CancelTransaction(transactionId);
                if (!result)
                    return BadRequest(new { error = "Cannot cancel transaction" });

                return Ok(new { message = "Transaction cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("transactions/{transactionId:guid}")]
        public async Task<IActionResult> GetTransactionInfo(Guid transactionId)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionInfo(transactionId);
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("accounts/{accountId:guid}/transactions")]
        public async Task<IActionResult> GetTransactionsByAccount(
            Guid accountId,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            [FromQuery] TransactionStatus? status,
            [FromQuery] string? currency,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new TransactionFilter
                (
                    MinAmount: minAmount,
                    MaxAmount: maxAmount,
                    Status: status,
                    Currency: currency,
                    FromDate: fromDate,
                    ToDate: toDate
                );

                var transactions = await _transactionService.GetTransactionsByAccount(accountId, filter, page, pageSize);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet("users/{userId:guid}/transactions")]
        public async Task<IActionResult> GetTransactionsByUser(
            Guid userId,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            [FromQuery] TransactionStatus? status,
            [FromQuery] string? currency,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new TransactionFilter
                (
                    MinAmount: minAmount,
                    MaxAmount: maxAmount,
                    Status: status,
                    Currency: currency,
                    FromDate: fromDate,
                    ToDate: toDate
                );

                var transactions = await _transactionService.GetTransactionsByUser(userId, filter, page, pageSize);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}