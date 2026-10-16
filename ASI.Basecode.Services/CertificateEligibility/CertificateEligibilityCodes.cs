namespace ASI.Basecode.Services.CertificateEligibility;

public static class CertificateEligibilityCodes
{
    // Roles
    public const string Admin = "admin";
    public const string Officer = "officer";
    public const string Member = "member";

    // Event statuses
    public const string EventCompleted = "completed";
    public const string EventDraft = "draft";
    public const string EventPublished = "published";
    public const string EventCancelled = "cancelled";

    // Certificate statuses
    public const string PendingApproval = "pending_approval";
    public const string Eligible = "eligible";
    public const string NotEligible = "not_eligible";
    public const string Issued = "issued";

    // Review decisions
    public const string DecisionApprove = "approve";
    public const string DecisionDecline = "decline";

    // Stable failure / error codes
    public const string InvalidInput = "invalid_input";
    public const string Forbidden = "forbidden";
    public const string TargetMissing = "target_missing";
    public const string ConflictStale = "conflict_stale";
    public const string InvalidAttendance = "invalid_attendance";
    public const string EventNotCompleted = "event_not_completed";
    public const string InconsistentData = "inconsistent_data";
    public const string InvalidReason = "invalid_reason";
    public const string IssuedCannotMutate = "issued_cannot_mutate";

    public static bool IsReviewerRole(string role) =>
        role is Admin or Officer;

    public static bool IsValidDecision(string decision) =>
        decision is DecisionApprove or DecisionDecline;

    public static bool IsValidCertificateStatus(string status) =>
        status is PendingApproval or Eligible or NotEligible or Issued;
}
