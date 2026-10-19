using ASI.Basecode.Services.ServiceModels.AccountAdministration;
using static ASI.Basecode.Services.AccountAdministration.AccountAdministrationCodes;
namespace ASI.Basecode.Services.AccountAdministration;

public static class AccountAdministrationRules
{
    public static AccountRuleDecision ChangeRole(AccountRuleInput actor, AccountRuleInput target,
        bool anotherActiveAdminExists, ChangeAccountRoleRequest request)
    {
        var error = CheckAccounts(actor, target);
        if (error != null) return AccountRuleDecision.Reject(error);
        if (request == null) return AccountRuleDecision.Reject(InvalidInput);
        if (!IsRole(request.Role) || !IsRole(request.ExpectedRole)) return AccountRuleDecision.Reject(InvalidRole);
        if (!ValidReason(request.Reason)) return AccountRuleDecision.Reject(InvalidReason);
        if (request.ExpectedRole != target.Role) return AccountRuleDecision.Reject(StaleState);
        return Decide(target, request.Role, target.IsActive, anotherActiveAdminExists, request.Reason.Trim());
    }
    public static AccountRuleDecision ChangeStatus(AccountRuleInput actor, AccountRuleInput target,
        bool anotherActiveAdminExists, ChangeAccountStatusRequest request)
    {
        var error = CheckAccounts(actor, target);
        if (error != null) return AccountRuleDecision.Reject(error);
        if (request?.IsActive == null || request.ExpectedIsActive == null) return AccountRuleDecision.Reject(InvalidInput);
        if (!ValidReason(request.Reason)) return AccountRuleDecision.Reject(InvalidReason);
        if (request.ExpectedIsActive != target.IsActive) return AccountRuleDecision.Reject(StaleState);
        return Decide(target, target.Role, request.IsActive.Value, anotherActiveAdminExists, request.Reason.Trim());
    }
    private static string CheckAccounts(AccountRuleInput actor, AccountRuleInput target)
    {
        if (actor == null || actor.Id <= 0) return InvalidInput;
        if (!actor.IsActive || actor.Role != Admin) return Forbidden;
        if (target == null || target.Id <= 0 || !IsRole(target.Role)) return InvalidInput;
        return null;
    }
    private static bool ValidReason(string reason) => !string.IsNullOrWhiteSpace(reason) && reason.Length <= 1000;
    private static AccountRuleDecision Decide(AccountRuleInput target, string role, bool active, bool otherAdmin, string reason)
    {
        if (target.Role == role && target.IsActive == active) return new(true, false, NormalizedReason: reason);
        if (target.Role == Admin && target.IsActive && (!active || role != Admin) && !otherAdmin)
            return AccountRuleDecision.Reject(LastAdmin);
        return new(true, true, NormalizedReason: reason);
    }
}
