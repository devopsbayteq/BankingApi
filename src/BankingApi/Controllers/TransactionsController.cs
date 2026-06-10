using BankingApi.Models;
using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BankingApi.Controllers;

/// <summary>
/// Manages account transactions / movements.
/// </summary>
[ApiController]
[Route("api/v1/accounts/{accountId}/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService service, ILogger<TransactionsController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // ------------------------------------------------------------------ //
    //  GET /api/v1/accounts/{accountId}/transactions
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Retrieves a paginated list of transactions for a specific account.
    /// </summary>
    /// <param name="accountId">Unique account identifier (e.g., ACC-001).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page, max 100 (default: 10).</param>
    /// <param name="from">Filter transactions from this date (UTC, ISO 8601).</param>
    /// <param name="to">Filter transactions up to this date (UTC, ISO 8601).</param>
    /// <param name="type">Filter by type: DEBIT or CREDIT.</param>
    [HttpGet]
    [SwaggerOperation(
        Summary     = "List account transactions",
        Description = "Returns a paginated, filterable list of movements for the specified account.",
        OperationId = "GetTransactions",
        Tags        = ["Transactions"])]
    [SwaggerResponse(200, "Transactions retrieved successfully.", typeof(PagedResponse<Transaction>))]
    [SwaggerResponse(400, "Invalid request parameters.",           typeof(ErrorResponse))]
    [SwaggerResponse(404, "Account not found.",                    typeof(ErrorResponse))]
    [SwaggerResponse(500, "Internal server error.",                typeof(ErrorResponse))]
    public async Task<IActionResult> GetTransactions(
        [FromRoute] string accountId,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to   = null,
        [FromQuery] string? type   = null)
    {
        _logger.LogInformation("Fetching transactions for account {AccountId}, page {Page}", accountId, page);

        if (page < 1)
            return BadRequest(BuildError(400, "Page must be greater than 0."));

        if (pageSize is < 1 or > 100)
            return BadRequest(BuildError(400, "PageSize must be between 1 and 100."));

        if (type is not null && type is not "DEBIT" and not "CREDIT")
            return BadRequest(BuildError(400, "Type must be DEBIT or CREDIT."));

        var account = await _service.GetAccountAsync(accountId);
        if (account is null)
        {
            _logger.LogWarning("Account {AccountId} not found", accountId);
            return NotFound(BuildError(404, $"Account '{accountId}' not found."));
        }

        var result = await _service.GetTransactionsAsync(accountId, page, pageSize, from, to, type);
        return Ok(result);
    }

    // ------------------------------------------------------------------ //
    //  GET /api/v1/accounts/{accountId}/transactions/{transactionId}
    // ------------------------------------------------------------------ //

    /// <summary>
    /// Retrieves a single transaction by its ID.
    /// </summary>
    /// <param name="accountId">Unique account identifier.</param>
    /// <param name="transactionId">Unique transaction identifier.</param>
    [HttpGet("{transactionId}")]
    [SwaggerOperation(
        Summary     = "Get a single transaction",
        Description = "Returns the full details of one transaction belonging to the specified account.",
        OperationId = "GetTransactionById",
        Tags        = ["Transactions"])]
    [SwaggerResponse(200, "Transaction retrieved successfully.", typeof(Transaction))]
    [SwaggerResponse(404, "Account or transaction not found.",   typeof(ErrorResponse))]
    [SwaggerResponse(500, "Internal server error.",              typeof(ErrorResponse))]
    public async Task<IActionResult> GetTransactionById(
        [FromRoute] string accountId,
        [FromRoute] string transactionId)
    {
        _logger.LogInformation("Fetching transaction {TransactionId} for account {AccountId}", transactionId, accountId);

        var account = await _service.GetAccountAsync(accountId);
        if (account is null)
            return NotFound(BuildError(404, $"Account '{accountId}' not found."));

        var transaction = await _service.GetTransactionByIdAsync(accountId, transactionId);
        if (transaction is null)
            return NotFound(BuildError(404, $"Transaction '{transactionId}' not found for account '{accountId}'."));

        return Ok(transaction);
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private static ErrorResponse BuildError(int code, string message, string? detail = null) =>
        new() { StatusCode = code, Message = message, Detail = detail };
}
