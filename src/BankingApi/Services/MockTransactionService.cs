using BankingApi.Models;

namespace BankingApi.Services;

/// <summary>
/// In-memory mock implementation of <see cref="ITransactionService"/>.
/// Returns deterministic seed data — no external dependencies.
/// </summary>
public class MockTransactionService : ITransactionService
{
    private static readonly List<Account> _accounts = GenerateAccounts();
    private static readonly List<Transaction> _transactions = GenerateTransactions();

    public Task<Account?> GetAccountAsync(string accountId)
    {
        var account = _accounts.FirstOrDefault(a =>
            a.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(account);
    }

    public Task<PagedResponse<Transaction>> GetTransactionsAsync(
        string accountId,
        int page,
        int pageSize,
        DateTime? from,
        DateTime? toDate,
        string? type)
    {
        var query = _transactions
            .Where(t => t.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase));

        if (from.HasValue)
            query = query.Where(t => t.TransactionDate >= from.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.TransactionDate <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        var ordered = query.OrderByDescending(t => t.TransactionDate).ToList();
        var totalItems = ordered.Count;
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize);

        var response = new PagedResponse<Transaction>
        {
            Data = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        return Task.FromResult(response);
    }

    public Task<Transaction?> GetTransactionByIdAsync(string accountId, string transactionId)
    {
        var transaction = _transactions.FirstOrDefault(t =>
            t.AccountId.Equals(accountId, StringComparison.OrdinalIgnoreCase) &&
            t.TransactionId.Equals(transactionId, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(transaction);
    }

    private static List<Account> GenerateAccounts() =>
    [
        new Account { AccountId = "ACC-001", HolderName = "Maria Garcia",  AccountType = "Checking", Balance = 4_250.75m,  Currency = "USD" },
        new Account { AccountId = "ACC-002", HolderName = "Carlos Mendez", AccountType = "Savings",  Balance = 12_800.00m, Currency = "USD" },
        new Account { AccountId = "ACC-003", HolderName = "Ana Torres",    AccountType = "Checking", Balance = 800.20m,    Currency = "USD" },
    ];

    private static List<Transaction> GenerateTransactions()
    {
        var transactions = new List<Transaction>();

        // Fixed seed → deterministic results (mock data only, no security use)
        var rng = new Random(42);

        var seeds = new[] { ("ACC-001", 4_250.75m), ("ACC-002", 12_800.00m), ("ACC-003", 800.20m) };

        string[] categories    = ["Transfer", "Payment", "Deposit", "Withdrawal", "Fee", "Interest"];
        string[] creditDescs   = ["Salary deposit", "Transfer received", "Interest earned", "Refund", "Deposit ATM"];
        string[] debitDescs    = ["Utility payment", "Online purchase", "ATM withdrawal", "Service fee", "Wire transfer"];

        foreach (var (accountId, startBalance) in seeds)
        {
            var balance = startBalance;
            var baseDate = DateTime.UtcNow.Date;

            for (int i = 1; i <= 30; i++)
            {
                var isCredit = rng.NextDouble() > 0.5;
                var amount = Math.Round((decimal)(rng.NextDouble() * 1_000 + 10), 2);
                balance += isCredit ? amount : -amount;
                var amount = Math.Round((decimal)(rng.NextDouble() * 1_000 + 10), 2);
                balance += isCredit ? amount : -amount;

                transactions.Add(new Transaction
                {
                    TransactionId = $"TXN-{accountId}-{i:D3}",
                    AccountId = accountId,
                    TransactionId = $"TXN-{accountId}-{i:D3}",
                    AccountId = accountId,
                    TransactionDate = baseDate.AddDays(-i).AddHours(rng.Next(0, 23)).AddMinutes(rng.Next(0, 59)),
                    Description = isCredit ? creditDescs[rng.Next(creditDescs.Length)] : debitDescs[rng.Next(debitDescs.Length)],
                    Amount = isCredit ? amount : -amount,
                    RunningBalance = Math.Round(balance, 2),
                    Type = isCredit ? "CREDIT" : "DEBIT",
                    Category = categories[rng.Next(categories.Length)],
                    Status = "COMPLETED",
                    Currency = "USD"
                });
            }
        }

        return transactions;
    }
}
