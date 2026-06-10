using BankingApi.Services;
using FluentAssertions;
using Xunit;

namespace BankingApi.Tests.Services;

public class MockTransactionServiceTests
{
    private readonly MockTransactionService _sut = new();

    [Theory]
    [InlineData("ACC-001")]
    [InlineData("ACC-002")]
    [InlineData("ACC-003")]
    public async Task GetAccountAsyncKnownIdReturnsAccount(string accountId)
    {
        var account = await _sut.GetAccountAsync(accountId);
        account.Should().NotBeNull();
        account!.AccountId.Should().Be(accountId);
    }

    [Fact]
    public async Task GetAccountAsyncUnknownIdReturnsNull()
    {
        var account = await _sut.GetAccountAsync("ACC-999");
        account.Should().BeNull();
    }

    [Theory]
    [InlineData("acc-001")]
    [InlineData("Acc-001")]
    public async Task GetAccountAsyncCaseInsensitiveIdReturnsAccount(string accountId)
    {
        var account = await _sut.GetAccountAsync(accountId);
        account.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTransactionsAsyncValidAccountReturnsPagedResult()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 10, null, null, null);

        result.Should().NotBeNull();
        result.Data.Should().NotBeEmpty();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalItems.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTransactionsAsyncSecondPageReturnsDifferentItems()
    {
        var page1 = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);
        var page2 = await _sut.GetTransactionsAsync("ACC-001", 2, 5, null, null, null);

        var ids1 = page1.Data.Select(t => t.TransactionId).ToHashSet();
        var ids2 = page2.Data.Select(t => t.TransactionId).ToHashSet();

        ids1.Intersect(ids2).Should().BeEmpty("pages must not overlap");
    }

    [Fact]
    public async Task GetTransactionsAsyncFilterByCreditReturnsOnlyCreditTransactions()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, null, null, "CREDIT");
        result.Data.Should().OnlyContain(t => t.Type == "CREDIT");
    }

    [Fact]
    public async Task GetTransactionsAsyncFilterByDebitReturnsOnlyDebitTransactions()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, null, null, "DEBIT");
        result.Data.Should().OnlyContain(t => t.Type == "DEBIT");
    }

    [Fact]
    public async Task GetTransactionsAsyncUnknownAccountReturnsEmptyPage()
    {
        var result = await _sut.GetTransactionsAsync("ACC-999", 1, 10, null, null, null);
        result.TotalItems.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionsAsyncDateRangeFiltersCorrectly()
    {
        var from = DateTime.UtcNow.AddDays(-10);
        var toDate = DateTime.UtcNow.AddDays(-1);

        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 100, from, toDate, null);

        result.Data.Should().OnlyContain(t =>
            t.TransactionDate >= from && t.TransactionDate <= toDate);
    }

    [Fact]
    public async Task GetTransactionsAsyncResultsAreOrderedDescendingByDate()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 30, null, null, null);
        var dates = result.Data.Select(t => t.TransactionDate).ToList();
        dates.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetTransactionByIdAsyncValidIdsReturnsTransaction()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-001", "TXN-ACC-001-001");
        transaction.Should().NotBeNull();
        transaction!.AccountId.Should().Be("ACC-001");
    }

    [Fact]
    public async Task GetTransactionByIdAsyncWrongAccountReturnsNull()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-002", "TXN-ACC-001-001");
        transaction.Should().BeNull();
    }

    [Fact]
    public async Task GetTransactionByIdAsyncUnknownTransactionIdReturnsNull()
    {
        var transaction = await _sut.GetTransactionByIdAsync("ACC-001", "TXN-DOES-NOT-EXIST");
        transaction.Should().BeNull();
    }

    [Fact]
    public async Task PagedResponseTotalPagesIsCalculatedCorrectly()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);
        var expectedPages = (int)Math.Ceiling((double)result.TotalItems / 5);
        result.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public async Task PagedResponseHasNextPageIsTrueWhenMorePagesExist()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 5, null, null, null);
        if (result.TotalItems > 5)
            result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task PagedResponseHasPreviousPageIsFalseOnFirstPage()
    {
        var result = await _sut.GetTransactionsAsync("ACC-001", 1, 10, null, null, null);
        result.HasPreviousPage.Should().BeFalse();
    }
}
