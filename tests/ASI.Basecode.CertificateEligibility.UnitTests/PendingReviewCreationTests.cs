using ASI.Basecode.Services.CertificateEligibility;
using static ASI.Basecode.Services.CertificateEligibility.CertificateEligibilityCodes;
using Xunit;

namespace ASI.Basecode.CertificateEligibility.UnitTests;

public class PendingReviewCreationTests
{
    [Fact]
    public void EvaluateCreation_WhenValidCompletedAttendeeAndNoExistingCertificate_ReturnsTransitionToPending()
    {
        var participation = new CertificateParticipationInput(
            RegistrationId: 10,
            EventId: 1,
            EventStatus: EventCompleted,
            HasRetainedAttendance: true,
            IsAttendanceInvalidated: false
        );

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCertificate: null);

        Assert.True(decision.Allowed);
        Assert.True(decision.Changed);
        Assert.Equal(PendingApproval, decision.ProposedStatus);
        Assert.Null(decision.FailureCode);
    }

    [Theory]
    [InlineData(EventDraft)]
    [InlineData(EventPublished)]
    [InlineData(EventCancelled)]
    [InlineData("unknown")]
    public void EvaluateCreation_WhenEventNotCompleted_RejectsWithEventNotCompleted(string eventStatus)
    {
        var participation = new CertificateParticipationInput(10, 1, eventStatus, true, false);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, null);

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(EventNotCompleted, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCreation_WhenNoRetainedAttendance_RejectsWithInvalidAttendance()
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, HasRetainedAttendance: false, IsAttendanceInvalidated: false);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, null);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidAttendance, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCreation_WhenAttendanceIsInvalidated_RejectsWithInvalidAttendance()
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, HasRetainedAttendance: true, IsAttendanceInvalidated: true);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, null);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidAttendance, decision.FailureCode);
    }

    [Theory]
    [InlineData(PendingApproval)]
    [InlineData(Eligible)]
    [InlineData(NotEligible)]
    [InlineData(Issued)]
    public void EvaluateCreation_WhenCertificateAlreadyExists_ReturnsNoChangeWithoutResetting(string existingStatus)
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, true, false);
        var existingCert = new CertificateStateInput(CertificateId: 50, RegistrationId: 10, Status: existingStatus, ReviewNotes: "Existing notes");

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCert);

        Assert.True(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(existingStatus, decision.ProposedStatus);
    }

    [Fact]
    public void EvaluateCreation_WhenParticipationIsNull_RejectsWithInvalidInput()
    {
        var decision = CertificateEligibilityRules.EvaluateCreation(null, null);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void EvaluateCreation_WhenParticipationIdsNonPositive_RejectsWithInvalidInput(int regId, int eventId)
    {
        var participation = new CertificateParticipationInput(regId, eventId, EventCompleted, true, false);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, null);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidInput, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCreation_WhenExistingCertificateHasNonPositiveId_RejectsInconsistentData()
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, true, false);
        var existingCert = new CertificateStateInput(CertificateId: 0, RegistrationId: 10, Status: PendingApproval, ReviewNotes: null);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCert);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCreation_WhenExistingCertificateRegistrationMismatched_RejectsInconsistentData()
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, true, false);
        var existingCert = new CertificateStateInput(CertificateId: 50, RegistrationId: 99, Status: PendingApproval, ReviewNotes: null);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCert);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCreation_WhenExistingCertificateStatusUnknown_RejectsInconsistentData()
    {
        var participation = new CertificateParticipationInput(10, 1, EventCompleted, true, false);
        var existingCert = new CertificateStateInput(CertificateId: 50, RegistrationId: 10, Status: "invalid_status", ReviewNotes: null);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCert);

        Assert.False(decision.Allowed);
        Assert.Equal(InconsistentData, decision.FailureCode);
    }

    [Theory]
    // Event completed, valid attendance (retained=true, invalidated=false) -> Pending or Preserved
    [InlineData(EventCompleted, true, false, null, true, true, PendingApproval, null)]
    [InlineData(EventCompleted, true, false, PendingApproval, true, false, PendingApproval, null)]
    [InlineData(EventCompleted, true, false, Eligible, true, false, Eligible, null)]
    [InlineData(EventCompleted, true, false, NotEligible, true, false, NotEligible, null)]
    [InlineData(EventCompleted, true, false, Issued, true, false, Issued, null)]

    // Event completed, missing attendance (retained=false, invalidated=false) -> Rejected with InvalidAttendance
    [InlineData(EventCompleted, false, false, null, false, false, null, InvalidAttendance)]
    [InlineData(EventCompleted, false, false, PendingApproval, false, false, null, InvalidAttendance)]

    // Event completed, invalidated attendance (retained=true, invalidated=true) -> Rejected with InvalidAttendance
    [InlineData(EventCompleted, true, true, null, false, false, null, InvalidAttendance)]
    [InlineData(EventCompleted, true, true, PendingApproval, false, false, null, InvalidAttendance)]

    // Event NOT completed (draft, published, cancelled) -> Rejected with EventNotCompleted regardless of attendance
    [InlineData(EventDraft, true, false, null, false, false, null, EventNotCompleted)]
    [InlineData(EventPublished, true, false, null, false, false, null, EventNotCompleted)]
    [InlineData(EventCancelled, true, false, null, false, false, null, EventNotCompleted)]
    [InlineData("unknown_event_status", true, false, null, false, false, null, EventNotCompleted)]
    public void EvaluateCreation_StateCombinationMatrix_FollowsBusinessRules(
        string eventStatus,
        bool hasRetainedAttendance,
        bool isAttendanceInvalidated,
        string existingCertStatus,
        bool expectedAllowed,
        bool expectedChanged,
        string expectedProposedStatus,
        string expectedFailureCode)
    {
        var participation = new CertificateParticipationInput(
            RegistrationId: 10,
            EventId: 1,
            EventStatus: eventStatus,
            HasRetainedAttendance: hasRetainedAttendance,
            IsAttendanceInvalidated: isAttendanceInvalidated);

        var existingCert = existingCertStatus == null
            ? null
            : new CertificateStateInput(CertificateId: 50, RegistrationId: 10, Status: existingCertStatus, ReviewNotes: null);

        var decision = CertificateEligibilityRules.EvaluateCreation(participation, existingCert);

        Assert.Equal(expectedAllowed, decision.Allowed);
        Assert.Equal(expectedChanged, decision.Changed);
        Assert.Equal(expectedProposedStatus, decision.ProposedStatus);
        Assert.Equal(expectedFailureCode, decision.FailureCode);
    }
}
