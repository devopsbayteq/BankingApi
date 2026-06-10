namespace BankingApi.Models;

/// <summary>
/// Standard error response envelope.
/// </summary>
public class ErrorResponse
{
    /// <summary>HTTP status code.</summary>
    public int StatusCode { get; set; }

    /// <summary>Human-readable error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional detail or inner error information.</summary>
    public string? Detail { get; set; }

    /// <summary>UTC timestamp of the error.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
