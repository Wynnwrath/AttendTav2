namespace ASI.Basecode.Services.ServiceModels.CertificateEligibility;

public sealed record ReviewCertificateRequest(
    int CertificateId,
    string Decision,
    string ExpectedStatus,
    string DeclineReason = null
);
