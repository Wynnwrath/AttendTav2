namespace ASI.Basecode.Services.AccountAdministration;

public sealed record AccountRuleDecision(bool Allowed, bool Changed, string FailureCode = null, string NormalizedReason = null)
{
    public static AccountRuleDecision Reject(string code) => new(false, false, code);
}
