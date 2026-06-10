namespace BankingApi.Models;

/// <summary>
/// Represents a single account transaction / movement.
/// </summary>
public class Transaction
{
    /// <summary>Unique transaction identifier.</summary>
    public string TransactionId { get; set; } = string.Empty;

    /// <summary>Account identifier this transaction belongs to.</summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>Transaction date and time (UTC).</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>Short description of the transaction.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Transaction amount (positive = credit, negative = debit).</summary>
    public decimal Amount { get; set; }

    /// <summary>Running balance after this transaction.</summary>
    public decimal RunningBalance { get; set; }

    /// <summary>Transaction type: DEBIT or CREDIT.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Transaction category (e.g., Transfer, Payment, Deposit).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Transaction status: COMPLETED, PENDING, FAILED.</summary>
    public string Status { get; set; } = "COMPLETED";

    /// <summary>Currency code (ISO 4217).</summary>
    public string Currency { get; set; } = "USD";
}
