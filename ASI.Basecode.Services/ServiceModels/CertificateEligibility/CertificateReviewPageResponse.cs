namespace ASI.Basecode.Services.ServiceModels.CertificateEligibility;

public sealed record CertificateReviewPageResponse(
    System.Collections.Generic.IReadOnlyList<CertificateReviewSummaryResponse> Items,
    int Page,
    int TotalCount
)
{
    public int PageSize => 25;
    public int TotalPages => TotalCount <= 0 ? 0 : (int)System.Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
