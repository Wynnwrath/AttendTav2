using static ASI.Basecode.Services.CertificateEligibility.CertificateEligibilityCodes;

namespace ASI.Basecode.Services.CertificateEligibility;

/// <summary>
/// Pure consequence rules for attendance corrections, defining how an attendance invalidation
/// impacts an existing certificate record. For later coordination with the Attendance owner.
/// </summary>
public static class AttendanceCorrectionCertificateRules
{
    public static CertificateEligibilityDecision EvaluateCorrectionConsequence(
        CertificateStateInput certificate,
        string correctionReason)
    {
        // 1. Validate correction reason first
        if (string.IsNullOrWhiteSpace(correctionReason))
            return CertificateEligibilityDecision.Reject(InvalidReason);

        var trimmedReason = correctionReason.Trim();

        // 2. If no certificate exists (e.g. before certificate preparation), allow attendance correction with no certificate mutation or creation
        if (certificate == null)
            return CertificateEligibilityDecision.NoChange(null, trimmedReason);

        // 3. Evaluate by current certificate status
        switch (certificate.Status)
        {
            case PendingApproval:
            case Eligible:
                // Both unissued states transition to not_eligible with correction reason
                return CertificateEligibilityDecision.Transition(NotEligible, trimmedReason);

            case NotEligible:
                // Already declined / not_eligible: preserve existing decision and reason; no duplicate mutation
                return CertificateEligibilityDecision.NoChange(NotEligible, certificate.ReviewNotes);

            case Issued:
                // Issued certificates block ordinary attendance correction
                return CertificateEligibilityDecision.Reject(IssuedCannotMutate);

            default:
                // Unknown or corrupt status: fail closed
                return CertificateEligibilityDecision.Reject(InconsistentData);
        }
    }
}
