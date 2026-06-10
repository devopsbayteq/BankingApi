namespace BankingApi.Models;

/// <summary>
/// Represents a bank account.
/// </summary>
public class Account
{
    /// <summary>Unique account identifier.</summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>Account holder's full name.</summary>
    public string HolderName { get; set; } = string.Empty;

    /// <summary>Account type (e.g., Checking, Savings).</summary>
    public string AccountType { get; set; } = string.Empty;

    /// <summary>Current balance in USD.</summary>
    public decimal Balance { get; set; }

    /// <summary>Account currency code (ISO 4217).</summary>
    public string Currency { get; set; } = "USD";
}
