using ASI.Basecode.Services.CertificateEligibility;
using ASI.Basecode.Services.ServiceModels.CertificateEligibility;
using static ASI.Basecode.Services.CertificateEligibility.CertificateEligibilityCodes;
using Xunit;

namespace ASI.Basecode.CertificateEligibility.UnitTests;

public class CertificateReviewRulesTests
{
    private readonly CertificateReviewerInput _adminReviewer = new(Id: 1, Role: Admin, IsActive: true);
    private readonly CertificateReviewerInput _officerReviewer = new(Id: 2, Role: Officer, IsActive: true);
    private readonly CertificateParticipationInput _validParticipation = new(RegistrationId: 10, EventId: 5, EventStatus: EventCompleted, HasRetainedAttendance: true, IsAttendanceInvalidated: false);
    private readonly CertificateStateInput _pendingCert = new(CertificateId: 100, RegistrationId: 10, Status: PendingApproval, ReviewNotes: null);

    [Theory]
    [InlineData(Admin)]
    [InlineData(Officer)]
    public void EvaluateReview_Approve_WhenActiveAuthorizedReviewer_TransitionsToEligible(string role)
    {
        var reviewer = new CertificateReviewerInput(Id: 3, Role: role, IsActive: true);
        var request = new ReviewCertificateRequest(CertificateId: 100, Decision: DecisionApprove, ExpectedStatus: PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(reviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.True(decision.Changed);
        Assert.Equal(Eligible, decision.ProposedStatus);
        Assert.Null(decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_Decline_WhenValidReason_TransitionsToNotEligibleWithReason()
    {
        var request = new ReviewCertificateRequest(CertificateId: 100, Decision: DecisionDecline, ExpectedStatus: PendingApproval, DeclineReason: "Did not fulfill assignment requirement.");

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.True(decision.Changed);
        Assert.Equal(NotEligible, decision.ProposedStatus);
        Assert.Equal("Did not fulfill assignment requirement.", decision.NormalizedReason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EvaluateReview_Decline_WhenBlankReason_RejectsWithInvalidReason(string reason)
    {
        var request = new ReviewCertificateRequest(100, DecisionDecline, PendingApproval, DeclineReason: reason);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidReason, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_Decline_WhenReasonExceeds1000Chars_RejectsWithInvalidReason()
    {
        var longReason = new string('x', 1001);
        var request = new ReviewCertificateRequest(100, DecisionDecline, PendingApproval, DeclineReason: longReason);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidReason, decision.FailureCode);
    }

    [Theory]
    [InlineData(Member)]
    [InlineData("guest")]
    [InlineData(null)]
    public void EvaluateReview_WhenReviewerRoleNotOfficerOrAdmin_RejectsForbidden(string role)
    {
        var reviewer = new CertificateReviewerInput(Id: 4, Role: role, IsActive: true);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(reviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(Forbidden, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenReviewerInactive_RejectsForbidden()
    {
        var reviewer = new CertificateReviewerInput(Id: 5, Role: Admin, IsActive: false);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(reviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(Forbidden, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_SelfReviewAllowed_WhenOfficerOrAdmin()
    {
        // An officer/admin may review their own record under identical rules
        var selfReviewer = new CertificateReviewerInput(Id: 10, Role: Officer, IsActive: true);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(selfReviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.Equal(Eligible, decision.ProposedStatus);
    }

    [Theory]
    [InlineData(Eligible)]
    [InlineData(NotEligible)]
    [InlineData(Issued)]
    public void EvaluateReview_WhenCurrentStatusNotPending_RejectsConflictStale(string currentStatus)
    {
        var nonPendingCert = new CertificateStateInput(100, 10, currentStatus, null);
        var request = new ReviewCertificateRequest(100, DecisionApprove, ExpectedStatus: currentStatus);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, nonPendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(ConflictStale, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenExpectedStatusDoesNotMatchCurrentStatus_RejectsConflictStale()
    {
        // Concurrency / stale edit detection: client expects 'eligible' but current is 'pending_approval'
        var request = new ReviewCertificateRequest(100, DecisionApprove, ExpectedStatus: Eligible);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(ConflictStale, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenAttendanceInvalidated_RejectsInvalidAttendance()
    {
        var invalidatedParticipation = new CertificateParticipationInput(10, 5, EventCompleted, HasRetainedAttendance: true, IsAttendanceInvalidated: true);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, invalidatedParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidAttendance, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenEventNotCompleted_RejectsEventNotCompleted()
    {
        var ongoingParticipation = new CertificateParticipationInput(10, 5, EventPublished, HasRetainedAttendance: true, IsAttendanceInvalidated: false);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, ongoingParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(EventNotCompleted, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenRegistrationIdMismatched_RejectsInconsistentData()
    {
        var mismatchedParticipation = new CertificateParticipationInput(RegistrationId: 999, EventId: 5, EventStatus: EventCompleted, HasRetainedAttendance: true, IsAttendanceInvalidated: false);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, mismatchedParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EvaluateReview_Approve_WhenDeclineReasonBlankOrOmitted_Succeeds(string reason)
    {
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval, DeclineReason: reason);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.Equal(Eligible, decision.ProposedStatus);
        Assert.Null(decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_Approve_WhenDeclineReasonNonBlank_RejectsInvalidInput()
    {
        // Approvals must not accept a decline reason
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval, DeclineReason: "Erroneous reason passed to approval");

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_Decline_WhenReasonExactly1000Chars_Succeeds()
    {
        var exact1000 = new string('a', 1000);
        var request = new ReviewCertificateRequest(100, DecisionDecline, PendingApproval, DeclineReason: exact1000);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.Equal(NotEligible, decision.ProposedStatus);
        Assert.Equal(1000, decision.NormalizedReason.Length);
    }

    [Fact]
    public void EvaluateReview_Decline_WhenReasonExactly1001Chars_RejectsInvalidReason()
    {
        var exact1001 = new string('a', 1001);
        var request = new ReviewCertificateRequest(100, DecisionDecline, PendingApproval, DeclineReason: exact1001);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidReason, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_Decline_TrimsDeclineReason()
    {
        var untrimmed = "   Needs further verification before eligibility.   ";
        var request = new ReviewCertificateRequest(100, DecisionDecline, PendingApproval, DeclineReason: untrimmed);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.True(decision.Allowed);
        Assert.Equal("Needs further verification before eligibility.", decision.NormalizedReason);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void EvaluateReview_WhenParticipationIdsNonPositive_RejectsInconsistentData(int regId, int eventId)
    {
        var invalidParticipation = new CertificateParticipationInput(regId, eventId, EventCompleted, true, false);
        var cert = new CertificateStateInput(100, regId, PendingApproval, null);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, invalidParticipation, cert, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenBothRegistrationIdsAreZero_RejectsInconsistentData()
    {
        // Guard against zero == zero false match
        var participationWithZeroReg = new CertificateParticipationInput(0, 5, EventCompleted, true, false);
        var certWithZeroReg = new CertificateStateInput(100, 0, PendingApproval, null);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, participationWithZeroReg, certWithZeroReg, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenCertificateRegistrationIdNonPositive_RejectsInconsistentData()
    {
        var certWithNegativeReg = new CertificateStateInput(100, -5, PendingApproval, null);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, certWithNegativeReg, request);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Theory]
    [InlineData(true, false, false, false)] // Null reviewer
    [InlineData(false, true, false, false)] // Null participation
    [InlineData(false, false, true, false)] // Null certificate
    [InlineData(false, false, false, true)] // Null request
    public void EvaluateReview_WhenAnyRequiredInputIsNull_RejectsWithConsistentContract(
        bool nullReviewer,
        bool nullParticipation,
        bool nullCert,
        bool nullRequest)
    {
        var reviewer = nullReviewer ? null : _adminReviewer;
        var participation = nullParticipation ? null : _validParticipation;
        var cert = nullCert ? null : _pendingCert;
        var request = nullRequest ? null : new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(reviewer, participation, cert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.NotNull(decision.FailureCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EvaluateReview_WhenReviewerIdNonPositive_RejectsInvalidInput(int reviewerId)
    {
        var reviewer = new CertificateReviewerInput(reviewerId, Admin, true);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(reviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EvaluateReview_WhenRequestCertificateIdNonPositive_RejectsInvalidInput(int certId)
    {
        var request = new ReviewCertificateRequest(certId, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EvaluateReview_WhenSnapshotCertificateIdNonPositive_RejectsTargetMissing(int certId)
    {
        var nonPositiveCert = new CertificateStateInput(certId, 10, PendingApproval, null);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, nonPositiveCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(TargetMissing, decision.FailureCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown_decision")]
    public void EvaluateReview_WhenDecisionUnknownOrNull_RejectsInvalidInput(string decisionCode)
    {
        var request = new ReviewCertificateRequest(100, decisionCode, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown_status")]
    public void EvaluateReview_WhenExpectedStatusUnknownOrNull_RejectsInvalidInput(string expectedStatus)
    {
        var request = new ReviewCertificateRequest(100, DecisionApprove, expectedStatus);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenRequestAndCertificateIdMismatch_RejectsInconsistentData()
    {
        var request = new ReviewCertificateRequest(CertificateId: 999, Decision: DecisionApprove, ExpectedStatus: PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, _validParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Fact]
    public void EvaluateReview_WhenAttendanceMissingDuringReview_RejectsInvalidAttendance()
    {
        // No retained attendance row (e.g. no-show)
        var noAttendanceParticipation = new CertificateParticipationInput(10, 5, EventCompleted, HasRetainedAttendance: false, IsAttendanceInvalidated: false);
        var request = new ReviewCertificateRequest(100, DecisionApprove, PendingApproval);

        var decision = CertificateEligibilityRules.EvaluateReview(_adminReviewer, noAttendanceParticipation, _pendingCert, request);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(InvalidAttendance, decision.FailureCode);
    }

    [Theory]
    // Valid participation: Completed event, retained attendance, not invalidated
    // Approve transitions pending_approval -> eligible
    [InlineData(PendingApproval, DecisionApprove, PendingApproval, true, true, false, true, true, Eligible, null)]
    // Decline transitions pending_approval -> not_eligible with valid reason
    [InlineData(PendingApproval, DecisionDecline, PendingApproval, true, true, false, true, true, NotEligible, null)]
    // Already eligible: cannot re-review
    [InlineData(Eligible, DecisionApprove, Eligible, true, true, false, false, false, null, ConflictStale)]
    [InlineData(Eligible, DecisionDecline, Eligible, true, true, false, false, false, null, ConflictStale)]
    // Already not_eligible: decline is irreversible
    [InlineData(NotEligible, DecisionApprove, NotEligible, true, true, false, false, false, null, ConflictStale)]
    [InlineData(NotEligible, DecisionDecline, NotEligible, true, true, false, false, false, null, ConflictStale)]
    // Issued: all review mutations blocked
    [InlineData(Issued, DecisionApprove, Issued, true, true, false, false, false, null, ConflictStale)]
    [InlineData(Issued, DecisionDecline, Issued, true, true, false, false, false, null, ConflictStale)]
    // ExpectedStatus mismatch -> ConflictStale
    [InlineData(PendingApproval, DecisionApprove, Eligible, true, true, false, false, false, null, ConflictStale)]
    // Invalid participation: Event not completed (draft) -> EventNotCompleted
    [InlineData(PendingApproval, DecisionApprove, PendingApproval, false, true, false, false, false, null, EventNotCompleted)]
    // Invalid participation: Retained=false -> InvalidAttendance
    [InlineData(PendingApproval, DecisionApprove, PendingApproval, true, false, false, false, false, null, InvalidAttendance)]
    // Invalid participation: Invalidated=true -> InvalidAttendance
    [InlineData(PendingApproval, DecisionApprove, PendingApproval, true, true, true, false, false, null, InvalidAttendance)]
    public void EvaluateReview_StateCombinationMatrix_FollowsBusinessRules(
        string currentCertStatus,
        string decision,
        string expectedStatus,
        bool isCompletedEvent,
        bool hasRetainedAttendance,
        bool isAttendanceInvalidated,
        bool expectedAllowed,
        bool expectedChanged,
        string expectedProposedStatus,
        string expectedFailureCode)
    {
        var participation = new CertificateParticipationInput(
            RegistrationId: 10,
            EventId: 5,
            EventStatus: isCompletedEvent ? EventCompleted : EventDraft,
            HasRetainedAttendance: hasRetainedAttendance,
            IsAttendanceInvalidated: isAttendanceInvalidated);

        var cert = new CertificateStateInput(100, 10, currentCertStatus, null);
        var declineReason = decision == DecisionDecline ? "Valid decline reason." : null;
        var request = new ReviewCertificateRequest(100, decision, expectedStatus, declineReason);

        var result = CertificateEligibilityRules.EvaluateReview(_adminReviewer, participation, cert, request);

        Assert.Equal(expectedAllowed, result.Allowed);
        Assert.Equal(expectedChanged, result.Changed);
        Assert.Equal(expectedProposedStatus, result.ProposedStatus);
        Assert.Equal(expectedFailureCode, result.FailureCode);
    }
}
