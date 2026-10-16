namespace ASI.Basecode.Services.ServiceModels.CertificateEligibility;

public sealed record CertificateReviewSummaryResponse(
    int CertificateId,
    int RegistrationId,
    int EventId,
    int MemberId,
    string FullName,
    string StudentNumber,
    System.DateTimeOffset? AttendedAt,
    string Status,
    System.Collections.Generic.IReadOnlyList<string> PermittedActions
);
