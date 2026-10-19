namespace ASI.Basecode.Services.AccountAdministration;

// Contract reference for the registration owner. No writes, seat changes or token operations.
public static class RegistrationCancellationRules
{
    public static AccountRuleDecision Evaluate(RegistrationRuleInput row, System.DateTimeOffset now)
    {
        if (row == null || row.Status is not ("pending" or "waitlisted" or "confirmed" or "cancelled")
            || row.EventStatus is not ("draft" or "published" or "completed" or "cancelled")
            || row.StartsAt == default || now == default)
            return AccountRuleDecision.Reject(AccountAdministrationCodes.InvalidInput);
        var cancel = row.StartsAt > now && row.EventStatus is ("draft" or "published")
            && row.Status != "cancelled" && !row.HasValidAttendance;
        return new(true, cancel);
    }
}
