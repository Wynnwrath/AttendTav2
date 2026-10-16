namespace ASI.Basecode.Services.CertificateEligibility;

public sealed record CertificateEligibilityDecision(
    bool Allowed,
    bool Changed,
    string ProposedStatus = null,
    string FailureCode = null,
    string NormalizedReason = null
)
{
    public static CertificateEligibilityDecision Reject(string code) =>
        new(Allowed: false, Changed: false, FailureCode: code);

    public static CertificateEligibilityDecision NoChange(string currentStatus, string reason = null) =>
        new(Allowed: true, Changed: false, ProposedStatus: currentStatus, NormalizedReason: reason);

    public static CertificateEligibilityDecision Transition(string nextStatus, string reason = null) =>
        new(Allowed: true, Changed: true, ProposedStatus: nextStatus, NormalizedReason: reason);
}
