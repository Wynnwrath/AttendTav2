using ASI.Basecode.Services.ServiceModels.CertificateEligibility;
using static ASI.Basecode.Services.CertificateEligibility.CertificateEligibilityCodes;

namespace ASI.Basecode.Services.CertificateEligibility;

public static class CertificateEligibilityRules
{
    public const int MaxReasonLength = 1000;

    /// <summary>
    /// Pure decision for pending-review creation (triggered upon event completion).
    /// </summary>
    public static CertificateEligibilityDecision EvaluateCreation(
        CertificateParticipationInput participation,
        CertificateStateInput existingCertificate)
    {
        if (participation == null || participation.RegistrationId <= 0 || participation.EventId <= 0)
            return CertificateEligibilityDecision.Reject(InvalidInput);

        // Event must be explicitly completed
        if (participation.EventStatus != EventCompleted)
            return CertificateEligibilityDecision.Reject(EventNotCompleted);

        // Valid attendance required: retained attendance row AND not invalidated
        if (!participation.HasRetainedAttendance || participation.IsAttendanceInvalidated)
            return CertificateEligibilityDecision.Reject(InvalidAttendance);

        // If a certificate record already exists, validate its integrity before returning NoChange
        if (existingCertificate != null)
        {
            if (existingCertificate.CertificateId <= 0 ||
                existingCertificate.RegistrationId != participation.RegistrationId ||
                !IsValidCertificateStatus(existingCertificate.Status))
            {
                return CertificateEligibilityDecision.Reject(InconsistentData);
            }

            return CertificateEligibilityDecision.NoChange(existingCertificate.Status);
        }

        // Eligible for pending review creation
        return CertificateEligibilityDecision.Transition(PendingApproval);
    }

    /// <summary>
    /// Pure decision for reviewing (approve or decline) an existing certificate review.
    /// </summary>
    public static CertificateEligibilityDecision EvaluateReview(
        CertificateReviewerInput reviewer,
        CertificateParticipationInput participation,
        CertificateStateInput certificate,
        ReviewCertificateRequest request)
    {
        // 1. Check reviewer authorization
        var authError = CheckReviewer(reviewer);
        if (authError != null) return CertificateEligibilityDecision.Reject(authError);

        // 2. Validate request
        if (request == null || request.CertificateId <= 0)
            return CertificateEligibilityDecision.Reject(InvalidInput);

        if (!IsValidDecision(request.Decision))
            return CertificateEligibilityDecision.Reject(InvalidInput);

        // ExpectedStatus must be provided and valid
        if (!IsValidCertificateStatus(request.ExpectedStatus))
            return CertificateEligibilityDecision.Reject(InvalidInput);

        // 3. Validate target certificate exists
        if (certificate == null || certificate.CertificateId <= 0)
            return CertificateEligibilityDecision.Reject(TargetMissing);

        if (certificate.CertificateId != request.CertificateId)
            return CertificateEligibilityDecision.Reject(InconsistentData);

        // Certificate registration ID must be positive
        if (certificate.RegistrationId <= 0)
            return CertificateEligibilityDecision.Reject(InconsistentData);

        // 4. Stale / concurrency check: current status must match expected status
        if (certificate.Status != request.ExpectedStatus)
            return CertificateEligibilityDecision.Reject(ConflictStale);

        // In this release, reviews can only transition from pending_approval
        if (certificate.Status != PendingApproval)
            return CertificateEligibilityDecision.Reject(ConflictStale);

        // 5. Validate participation facts: positive IDs, matching registration, completed event and valid attendance
        if (participation == null ||
            participation.RegistrationId <= 0 ||
            participation.EventId <= 0 ||
            participation.RegistrationId != certificate.RegistrationId)
        {
            return CertificateEligibilityDecision.Reject(InconsistentData);
        }

        if (participation.EventStatus != EventCompleted)
            return CertificateEligibilityDecision.Reject(EventNotCompleted);

        if (!participation.HasRetainedAttendance || participation.IsAttendanceInvalidated)
            return CertificateEligibilityDecision.Reject(InvalidAttendance);

        // 6. Evaluate specific decision
        if (request.Decision == DecisionApprove)
        {
            // Approve -> Eligible. A nonblank decline reason must be rejected per plan.
            if (!string.IsNullOrWhiteSpace(request.DeclineReason))
                return CertificateEligibilityDecision.Reject(InvalidInput);

            return CertificateEligibilityDecision.Transition(Eligible);
        }
        else // DecisionDecline
        {
            // Decline requires a nonblank reason up to MaxReasonLength characters
            if (string.IsNullOrWhiteSpace(request.DeclineReason))
                return CertificateEligibilityDecision.Reject(InvalidReason);

            if (request.DeclineReason.Length > MaxReasonLength)
                return CertificateEligibilityDecision.Reject(InvalidReason);

            return CertificateEligibilityDecision.Transition(NotEligible, request.DeclineReason.Trim());
        }
    }

    private static string CheckReviewer(CertificateReviewerInput reviewer)
    {
        if (reviewer == null || reviewer.Id <= 0) return InvalidInput;
        if (!reviewer.IsActive) return Forbidden;
        if (!IsReviewerRole(reviewer.Role)) return Forbidden;
        return null;
    }
}
