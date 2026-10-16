using ASI.Basecode.Services.ServiceModels.CertificateEligibility;
using Xunit;

namespace ASI.Basecode.CertificateEligibility.UnitTests;

public class RequestValidationAndPagingTests
{
    [Theory]
    [InlineData(null, 1)]
    [InlineData("", 1)]
    [InlineData("   ", 1)]
    [InlineData("invalid", 1)]
    [InlineData("-5", 1)]
    [InlineData("0", 1)]
    [InlineData("1", 1)]
    [InlineData("3", 3)]
    [InlineData("999999999999999999999", 1)] // Overflow protection
    public void CertificateReviewListRequest_NormalizePage_NormalizesSafely(string rawPage, int expectedPage)
    {
        var request = new CertificateReviewListRequest(EventId: 1, Page: rawPage);

        Assert.Equal(expectedPage, request.NormalizedPage);
    }

    [Theory]
    [InlineData(1, 0, 1)]   // Empty results: clamp to 1
    [InlineData(1, 10, 1)]  // 10 items (1 page): clamp to 1
    [InlineData(5, 50, 2)]  // 50 items (2 pages of 25): requested page 5 clamps to page 2
    [InlineData(2, 60, 2)]  // 60 items (3 pages): requested page 2 stays page 2
    [InlineData(0, 50, 1)]  // Page 0 clamps to 1
    [InlineData(-1, 50, 1)] // Negative clamps to 1
    public void CertificateReviewListRequest_ClampPage_CalculatesBoundaryProperly(int requestedPage, int totalCount, int expectedClampedPage)
    {
        var clamped = CertificateReviewListRequest.ClampPage(requestedPage, totalCount);

        Assert.Equal(expectedClampedPage, clamped);
    }

    [Fact]
    public void CertificateReviewPageResponse_CalculatesNavigationPropertiesCorrectly()
    {
        // 0 items
        var emptyResponse = new CertificateReviewPageResponse(System.Array.Empty<CertificateReviewSummaryResponse>(), Page: 1, TotalCount: 0);
        Assert.Equal(0, emptyResponse.TotalPages);
        Assert.False(emptyResponse.HasPreviousPage);
        Assert.False(emptyResponse.HasNextPage);

        // 60 items (3 pages), current page 2
        var midResponse = new CertificateReviewPageResponse(System.Array.Empty<CertificateReviewSummaryResponse>(), Page: 2, TotalCount: 60);
        Assert.Equal(3, midResponse.TotalPages);
        Assert.True(midResponse.HasPreviousPage);
        Assert.True(midResponse.HasNextPage);

        // 60 items, current page 3 (last page)
        var lastResponse = new CertificateReviewPageResponse(System.Array.Empty<CertificateReviewSummaryResponse>(), Page: 3, TotalCount: 60);
        Assert.Equal(3, lastResponse.TotalPages);
        Assert.True(lastResponse.HasPreviousPage);
        Assert.False(lastResponse.HasNextPage);
    }

    [Theory]
    [InlineData(0, 0, 1)]   // 0 items: 0 total pages, clamp returns 1
    [InlineData(1, 1, 1)]   // 1 item: 1 total page, page 1
    [InlineData(25, 1, 1)]  // 25 items: exactly 1 total page
    [InlineData(26, 2, 2)]  // 26 items: exactly 2 total pages
    [InlineData(50, 2, 2)]  // 50 items: exactly 2 total pages
    [InlineData(51, 3, 3)]  // 51 items: exactly 3 total pages
    public void CertificateReviewPageResponse_ExactBoundaries_CalculatesTotalPagesAndClamping(int totalCount, int expectedTotalPages, int requestedLastPage)
    {
        var response = new CertificateReviewPageResponse(System.Array.Empty<CertificateReviewSummaryResponse>(), Page: requestedLastPage, TotalCount: totalCount);

        Assert.Equal(expectedTotalPages, response.TotalPages);

        var clamped = CertificateReviewListRequest.ClampPage(requestedLastPage + 5, totalCount);
        var expectedClamp = expectedTotalPages == 0 ? 1 : expectedTotalPages;
        Assert.Equal(expectedClamp, clamped);
    }
}
