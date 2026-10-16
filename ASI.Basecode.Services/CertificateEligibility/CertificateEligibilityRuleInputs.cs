namespace ASI.Basecode.Services.CertificateEligibility;

// Immutable domain snapshots for pure evaluation. Not EF entities or HTTP request models.
public sealed record CertificateReviewerInput(
    int Id,
    string Role,
    bool IsActive
);

public sealed record CertificateParticipationInput(
    int RegistrationId,
    int EventId,
    string EventStatus,
    bool HasRetainedAttendance,
    bool IsAttendanceInvalidated
);

public sealed record CertificateStateInput(
    int CertificateId,
    int RegistrationId,
    string Status,
    string ReviewNotes
);
