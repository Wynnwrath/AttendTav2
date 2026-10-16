namespace ASI.Basecode.Services.ServiceModels.CertificateEligibility;

public sealed record CertificateReviewResponse(
    int CertificateId,
    string ResultingStatus,
    int ReviewerId,
    System.DateTimeOffset ReviewedAt,
    string NormalizedReason
);
