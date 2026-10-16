using ASI.Basecode.Services.CertificateEligibility;
using static ASI.Basecode.Services.CertificateEligibility.CertificateEligibilityCodes;
using Xunit;

namespace ASI.Basecode.CertificateEligibility.UnitTests;

public class AttendanceCorrectionCertificateRulesTests
{
    [Theory]
    [InlineData(PendingApproval)]
    [InlineData(Eligible)]
    public void EvaluateCorrectionConsequence_WhenPendingOrEligible_TransitionsToNotEligibleWithReason(string currentStatus)
    {
        var cert = new CertificateStateInput(CertificateId: 10, RegistrationId: 5, Status: currentStatus, ReviewNotes: "Initial notes");
        var correctionReason = "Mistaken check-in; attendee badge was scanned in error.";

        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(cert, correctionReason);

        Assert.True(decision.Allowed);
        Assert.True(decision.Changed);
        Assert.Equal(NotEligible, decision.ProposedStatus);
        Assert.Equal(correctionReason, decision.NormalizedReason);
        Assert.Null(decision.FailureCode);
    }

    [Fact]
    public void EvaluateCorrectionConsequence_WhenAlreadyNotEligible_PreservesExistingDecisionAndDoesNotMutate()
    {
        var originalDeclineNotes = "Declined during review due to disciplinary issue.";
        var cert = new CertificateStateInput(10, 5, NotEligible, ReviewNotes: originalDeclineNotes);

        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(cert, "Late attendance correction for mistaken check-in");

        Assert.True(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(NotEligible, decision.ProposedStatus);
        Assert.Equal(originalDeclineNotes, decision.NormalizedReason);
    }

    [Fact]
    public void EvaluateCorrectionConsequence_WhenIssued_BlocksCorrection()
    {
        var cert = new CertificateStateInput(10, 5, Issued, ReviewNotes: "Certificate already generated and delivered");

        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(cert, "Attempted correction after issue");

        Assert.False(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Equal(IssuedCannotMutate, decision.FailureCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EvaluateCorrectionConsequence_WhenBlankCorrectionReason_RejectsInvalidReason(string blankReason)
    {
        var cert = new CertificateStateInput(10, 5, PendingApproval, null);

        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(cert, blankReason);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidReason, decision.FailureCode);
    }

    [Fact]
    public void EvaluateCorrectionConsequence_WhenCertificateNullAndValidReason_AllowsCorrectionWithoutCertificateMutation()
    {
        // An absent certificate is normal before certificate preparation. Allow correction with no mutation.
        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(null, "Mistaken check-in corrected before certificate preparation");

        Assert.True(decision.Allowed);
        Assert.False(decision.Changed);
        Assert.Null(decision.ProposedStatus);
        Assert.Equal("Mistaken check-in corrected before certificate preparation", decision.NormalizedReason);
        Assert.Null(decision.FailureCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EvaluateCorrectionConsequence_WhenCertificateNullAndBlankReason_RejectsInvalidReason(string blankReason)
    {
        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(null, blankReason);

        Assert.False(decision.Allowed);
        Assert.Equal(InvalidReason, decision.FailureCode);
    }

    [Theory]
    // Absent certificate -> Allowed with NoChange
    [InlineData(null, true, false, null, null)]
    // Pending approval -> Allowed, Transitions to not_eligible with correction reason
    [InlineData(PendingApproval, true, true, NotEligible, null)]
    // Eligible -> Allowed, Transitions to not_eligible with correction reason
    [InlineData(Eligible, true, true, NotEligible, null)]
    // NotEligible -> Allowed, Preserves not_eligible without mutation
    [InlineData(NotEligible, true, false, NotEligible, null)]
    // Issued -> Blocked, IssuedCannotMutate
    [InlineData(Issued, false, false, null, IssuedCannotMutate)]
    // Corrupt / unknown status -> Fails closed with InconsistentData
    [InlineData("unknown_status", false, false, null, InconsistentData)]
    [InlineData("", false, false, null, InconsistentData)]
    public void EvaluateCorrectionConsequence_StateCombinationMatrix_FollowsBusinessRules(
        string currentStatus,
        bool expectedAllowed,
        bool expectedChanged,
        string expectedProposedStatus,
        string expectedFailureCode)
    {
        var cert = currentStatus == null
            ? null
            : new CertificateStateInput(10, 5, currentStatus, "Prior notes");

        var decision = AttendanceCorrectionCertificateRules.EvaluateCorrectionConsequence(cert, "Mistaken check-in correction");

        Assert.Equal(expectedAllowed, decision.Allowed);
        Assert.Equal(expectedChanged, decision.Changed);
        Assert.Equal(expectedProposedStatus, decision.ProposedStatus);
        Assert.Equal(expectedFailureCode, decision.FailureCode);
    }
}
