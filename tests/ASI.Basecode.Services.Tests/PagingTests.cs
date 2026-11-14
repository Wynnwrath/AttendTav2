using ASI.Basecode.Services.ServiceModels;
using Xunit;

namespace ASI.Basecode.Services.Tests;

public class PagingTests
{
    [Theory]
    [InlineData(null, 5)]
    [InlineData(0, 1)]
    [InlineData(-3, 1)]
    [InlineData(3, 3)]
    [InlineData(25, 25)]
    [InlineData(200, 25)]
    public void RequestedPageSizeIsBoundedAndDefaultsToFive(int? size, int expected)
    {
        var request = new PageRequest(2, size);
        Assert.Equal(expected, request.PageSize);
        Assert.Equal(expected, request.GetOffset(2));
        var result = new PagedResult<int>(Array.Empty<int>(), int.MaxValue, int.MaxValue, size);
        Assert.Equal(expected, result.PageSize);
        Assert.Equal(int.MaxValue, result.LastItemNumber);
    }

    [Theory]
    [InlineData(null, 26, 1)]
    [InlineData(0, 26, 1)]
    [InlineData(-10, 26, 1)]
    [InlineData(1, 26, 1)]
    [InlineData(2, 26, 2)]
    [InlineData(int.MaxValue, 26, 6)]
    [InlineData(50, 0, 1)]
    public void PageRequestNormalizesInvalidAndOutOfRangePages(int? requested, int totalCount, int expected)
    {
        Assert.Equal(expected, new PageRequest(requested).NormalizePageNumber(totalCount));
    }

    [Fact]
    public void PagedResultUsesRequestedSizeAndReportsNavigationForTwentySixRows()
    {
        var firstPage = new PagedResult<int>(Enumerable.Range(1, 25).ToArray(), 1, 26, 25);
        var lastPage = new PagedResult<int>(new[] { 26 }, 2, 26, 25);

        Assert.Equal(25, firstPage.PageSize);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.True(firstPage.HasNextPage);
        Assert.False(firstPage.HasPreviousPage);
        Assert.Equal(1, firstPage.FirstItemNumber);
        Assert.Equal(25, firstPage.LastItemNumber);
        Assert.True(lastPage.HasPreviousPage);
        Assert.False(lastPage.HasNextPage);
        Assert.Equal(26, lastPage.FirstItemNumber);
        Assert.Equal(26, lastPage.LastItemNumber);
    }

    [Fact]
    public void EmptyPagedResultHasOneNormalizedPageAndNoNavigation()
    {
        var empty = new PagedResult<string>(Array.Empty<string>(), int.MaxValue, 0);

        Assert.Equal(1, empty.PageNumber);
        Assert.Equal(0, empty.TotalPages);
        Assert.False(empty.HasPreviousPage);
        Assert.False(empty.HasNextPage);
        Assert.Equal(0, empty.FirstItemNumber);
        Assert.Equal(0, empty.LastItemNumber);
    }

    [Fact]
    public void HistoryTabPreservesEffectiveSizeAndLastPageRange()
    {
        var source = new PagedResult<MyEventRegistrationCardViewModel>(Array.Empty<MyEventRegistrationCardViewModel>(), 2, 26, 25);
        var tab = MyEventsViewModel.FromRegistrations(source).Registrations;
        Assert.Equal(25, tab.PageSize);
        Assert.Equal(2, tab.TotalPages);
        Assert.Equal(26, tab.FirstItemNumber);
        Assert.Equal(26, tab.LastItemNumber);
    }

    [Fact]
    public void ExtremeCountsKeepPageAndOffsetArithmeticInRange()
    {
        var request = new PageRequest(int.MaxValue, 25);
        var normalizedPage = request.NormalizePageNumber(int.MaxValue);

        Assert.Equal(85_899_346, normalizedPage);
        Assert.Equal(2_147_483_625, request.GetOffset(normalizedPage));
    }
}
