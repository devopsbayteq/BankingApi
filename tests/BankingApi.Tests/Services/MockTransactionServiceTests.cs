using BankingApi.Services;
using FluentAssertions;
using Xunit;

namespace BankingApi.Tests.Services;

public class MockTransactionServiceTests
{
    private readonly MockTransactionService _sut = new();

    // ------------------------------------------------------------------ //
    //  GetAccountAsync
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("ACC-001")]
    [InlineData("ACC-002")]
    [InlineData("ACC-003")]
    public async Task GetAccountAsync_KnownId_ReturnsAccount(string accountId)
    {
        var account = await _sut.GetAccountAsync(accountId);

        account.Should().NotBeNull();
        account!.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task GetAccountAsync_UnknownId_ReturnsNull()
    {
        var account = await _sut.GetAccountAsync("ACC-999");

        account.Should().BeNull();
    }

    [Theory]
    [InlineData("acc-001")]
    [InlineData("Acc-001")]
    public async Task GetAccountAsync_CaseInsensitiveId_ReturnsAccount(string accountId)
    {
        var account = await _sut.GetAccountAsync(accountId);

        account.Should().NotBeNull();
    }

    // ------------------------------------------------------------------ //
    //  GetTransactionsAsync
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetTransactionsAsync_ValidAccount_ReturnsPagedResult()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 10, null, null, null);

        result.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactionsAsync_SecondPage_ReturnsDifferentItems()
    {
        var page1 = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);
        var page2 = await _sut.GetTransactionsAsync("ACC-001", 2, 5, null, null, null);

        var ids1 = page1.Data.Select(t => t.TransactionId).ToHashSet();
        var ids2 = page2.Data.Select(t => t.TransactionId).ToHashSet();

        ids1.Intersect(ids2).Should().BeEmpty("pages must not overlap");
    }

    [Fact]
    public async Task GetTransactionsAsync_FilterByCredit_ReturnsOnlyCreditTransactions()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, null, null, "CREDIT");

        result.Data.Should().OnlyContain(t => t.Type == "CREDIT");
    }

    [Fact]
    public async Task GetTransactionsAsync_FilterByDebit_ReturnsOnlyDebitTransactions()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, null, null, "DEBIT");

        result.Data.Should().OnlyContain(t => t.Type == "DEBIT");
    }

    [Fact]
    public async Task GetTransactionsAsync_UnknownAccount_ReturnsEmptyPage()
    {
        var result = await _sut.GetTransactionsAsync("ACC-999", 1, 10, null, null, null);

        result.TotalItems.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionsAsync_DateRange_FiltersCorrectly()
    {
        var from = DateTime.UtcNow.AddDays(-10);
        var to = DateTime.UtcNow.AddDays(-1);

        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, from, to, null);

        result.Data.Should().OnlyContain(t =>
            t.TransactionDate >= from && t.TransactionDate <= to);
    }

    [Fact]
    public async Task GetTransactionsAsync_ResultsAreOrderedDescendingByDate()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 30, null, null, null);
        var dates = result.Data.Select(t => t.TransactionDate).ToList();

        dates.Should().BeInDescendingOrder();
    }

    // ------------------------------------------------------------------ //
    //  GetTransactionByIdAsync
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetTransactionByIdAsync_ValidIds_ReturnsTransaction()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-001", "TXN-ACC-001-001");

        transaction.Should().NotBeNull();
        transaction!.AccountId.Should().Be("ACC-001");
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WrongAccount_ReturnsNull()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-002", "TXN-ACC-001-001");

        transaction.Should().BeNull();
    }

    [Fact]
    public async Task GetTransactionByIdAsync_UnknownTransactionId_ReturnsNull()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-001", "TXN-DOES-NOT-EXIST");

        transaction.Should().BeNull();
    }

    // ------------------------------------------------------------------ //
    //  PagedResponse computed properties
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task PagedResponse_TotalPages_IsCalculatedCorrectly()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);

        var expectedPages = (int)Math.Ceiling((double)result.TotalItems / 5);
        result.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public async Task PagedResponse_HasNextPage_IsTrueWhenMorePagesExist()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);

        if (result.TotalItems > 5)
            result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task PagedResponse_HasPreviousPage_IsFalseOnFirstPage()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 10, null, null, null);

        result.HasPreviousPage.Should().BeFalse();
    }
}
