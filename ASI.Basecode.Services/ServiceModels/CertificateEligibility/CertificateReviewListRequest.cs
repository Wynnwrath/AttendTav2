namespace ASI.Basecode.Services.ServiceModels.CertificateEligibility;

public sealed record CertificateReviewListRequest(
    int EventId,
    string Page = null
)
{
    public const int PageSize = 25;

    public int NormalizedPage => NormalizePage(Page);

    public static int NormalizePage(string rawPage)
    {
        if (string.IsNullOrWhiteSpace(rawPage)) return 1;
        if (!int.TryParse(rawPage.Trim(), out var parsed)) return 1;
        return parsed < 1 ? 1 : parsed;
    }

    public static int ClampPage(int requestedPage, int totalCount)
    {
        if (totalCount <= 0) return 1;
        var totalPages = (int)System.Math.Ceiling((double)totalCount / PageSize);
        if (totalPages < 1) totalPages = 1;
        if (requestedPage > totalPages) return totalPages;
        return requestedPage < 1 ? 1 : requestedPage;
    }
}
