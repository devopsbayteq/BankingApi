namespace BankingApi.Models;

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
/// <typeparam name="T">Type of items in the data collection.</typeparam>
public class PagedResponse<T>
{
    /// <summary>Collection of items for the current page.</summary>
    public IEnumerable<T> Data { get; set; } = [];

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of items across all pages.</summary>
    public int TotalItems { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    /// <summary>Indicates whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Indicates whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;
}
