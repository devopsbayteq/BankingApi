using BankingApi.Models;

namespace BankingApi.Services;

/// <summary>Contract for account transaction operations.</summary>
public interface ITransactionService
{
    /// <summary>Returns a paginated list of transactions for the given account.</summary>
    Task<PagedResponse<Transaction>> GetTransactionsAsync(
        string accountId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? toDate,
        string? type);

    /// <summary>Returns a single transaction by its ID.</summary>
    Task<Transaction?> GetTransactionByIdAsync(string accountId, string transactionId);

    /// <summary>Returns the account summary (no transactions).</summary>
    Task<Account?> GetAccountAsync(string accountId);
}
