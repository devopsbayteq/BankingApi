using BankingApi.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using Xunit;

namespace BankingApi.Tests.Controllers;

public class TransactionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TransactionsControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ------------------------------------------------------------------ //
    //  GET /api/v1/accounts/{accountId}/transactions
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetTransactions_ValidAccount_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactions_ValidAccount_ReturnsPagedResponse()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions");
        var body = await response.Content.ReadAsStringAsync();
        var paged = JsonSerializer.Deserialize<PagedResponse<Transaction>>(body, _jsonOptions);

        paged.Should().NotBeNull();
        paged!.Data.Should().NotBeEmpty();
        paged.Page.Should().Be(1);
        paged.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactions_UnknownAccount_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-999/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactions_InvalidPage_Returns400()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions?page=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactions_InvalidPageSize_Returns400()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions?pageSize=200");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactions_InvalidType_Returns400()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions?type=INVALID");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("CREDIT")]
    [InlineData("DEBIT")]
    public async Task GetTransactions_FilterByType_Returns200(string type)
    {
        var response = await _client.GetAsync($"/api/v1/accounts/ACC-001/transactions?type={type}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactions_Pagination_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions?page=2&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ------------------------------------------------------------------ //
    //  GET /api/v1/accounts/{accountId}/transactions/{transactionId}
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetTransactionById_ValidIds_Returns200()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions/TXN-ACC-001-001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionById_ValidIds_ReturnsTransactionObject()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions/TXN-ACC-001-001");
        var body = await response.Content.ReadAsStringAsync();
        var transaction = JsonSerializer.Deserialize<Transaction>(body, _jsonOptions);

        transaction.Should().NotBeNull();
        transaction!.TransactionId.Should().Be("TXN-ACC-001-001");
        transaction.AccountId.Should().Be("ACC-001");
    }

    [Fact]
    public async Task GetTransactionById_UnknownTransaction_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-001/transactions/TXN-NOPE");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactionById_WrongAccount_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/accounts/ACC-002/transactions/TXN-ACC-001-001");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
