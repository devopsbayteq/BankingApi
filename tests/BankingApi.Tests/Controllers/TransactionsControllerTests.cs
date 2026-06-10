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

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TransactionsControllerTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactionsValidAccountReturns200()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionsValidAccountReturnsPagedResponse()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();
        var paged = JsonSerializer.Deserialize<PagedResponse<Transaction>>(body, JsonOptions);

        paged.Should().NotBeNull();
        paged!.Data.Should().NotBeEmpty();
        paged.Page.Should().Be(1);
        paged.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactionsUnknownAccountReturns404()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-999/transactions", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactionsInvalidPageReturns400()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions?page=0", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactionsInvalidPageSizeReturns400()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions?pageSize=200", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetTransactionsInvalidTypeReturns400()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions?type=INVALID", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("CREDIT")]
    [InlineData("DEBIT")]
    public async Task GetTransactionsFilterByTypeReturns200(string type)
    {
        var response = await _client.GetAsync(new Uri($"/api/v1/accounts/ACC-001/transactions?type={type}", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionsPaginationReturns200()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions?page=2&pageSize=5", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionByIdValidIdsReturns200()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions/TXN-ACC-001-001", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTransactionByIdValidIdsReturnsTransactionObject()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions/TXN-ACC-001-001", UriKind.Relative));
        var body = await response.Content.ReadAsStringAsync();
        var transaction = JsonSerializer.Deserialize<Transaction>(body, JsonOptions);

        transaction.Should().NotBeNull();
        transaction!.TransactionId.Should().Be("TXN-ACC-001-001");
        transaction.AccountId.Should().Be("ACC-001");
    }

    [Fact]
    public async Task GetTransactionByIdUnknownTransactionReturns404()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-001/transactions/TXN-NOPE", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTransactionByIdWrongAccountReturns404()
    {
        var response = await _client.GetAsync(new Uri("/api/v1/accounts/ACC-002/transactions/TXN-ACC-001-001", UriKind.Relative));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
