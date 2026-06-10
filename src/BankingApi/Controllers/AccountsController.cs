using BankingApi.Models;
using BankingApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BankingApi.Controllers;

/// <summary>Provides account summary information.</summary>
[ApiController]
[Route("api/v1/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly ITransactionService _service;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(ITransactionService service, ILogger<AccountsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Returns the details of a specific account.</summary>
    /// <param name="accountId">Unique account identifier (e.g., ACC-001, ACC-002, ACC-003).</param>
    [HttpGet("{accountId}")]
    [SwaggerOperation(
        Summary = "Get account details",
        Description = "Returns summary information for the specified account.",
        OperationId = "GetAccount",
        Tags = ["Accounts"])]
    [SwaggerResponse(200, "Account retrieved successfully.", typeof(Account))]
    [SwaggerResponse(404, "Account not found.", typeof(ErrorResponse))]
    [SwaggerResponse(500, "Internal server error.", typeof(ErrorResponse))]
    public async Task<IActionResult> GetAccount([FromRoute] string accountId)
    {
        _logger.LogInformation("Fetching account {AccountId}", accountId);

        var account = await _service.GetAccountAsync(accountId);
        if (account is null)
        {
            _logger.LogWarning("Account {AccountId} not found", accountId);
            return NotFound(new ErrorResponse { StatusCode = 404, Message = $"Account '{accountId}' not found." });
        }

        return Ok(account);
    }
}
