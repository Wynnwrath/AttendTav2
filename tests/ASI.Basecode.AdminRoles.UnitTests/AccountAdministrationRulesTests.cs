using Xunit;
using ASI.Basecode.Services.AccountAdministration;
using ASI.Basecode.Services.ServiceModels.AccountAdministration;
using static ASI.Basecode.Services.AccountAdministration.AccountAdministrationCodes;
namespace ASI.Basecode.AdminRoles.UnitTests;
public class AccountAdministrationRulesTests
{
    private static readonly AccountRuleInput AdminUser = new(1, Admin, true);
    [Theory]
    [InlineData(false, false)] [InlineData(true, true)]
    public void LastAdminRemovalRequiresAnotherAdmin(bool other, bool allowed)
    {
        Assert.Equal(allowed, AccountAdministrationRules.ChangeStatus(AdminUser, AdminUser, other, new(false, true, "reason")).Allowed);
        Assert.Equal(allowed, AccountAdministrationRules.ChangeRole(AdminUser, AdminUser, other, new(Member, Admin, "reason")).Allowed);
    }
    [Theory]
    [InlineData("member", true)] [InlineData("officer", true)] [InlineData("admin", false)]
    public void UnauthorizedActorCannotEvenSubmitNoOp(string role, bool active)
    {
        var result = AccountAdministrationRules.ChangeRole(new(2, role, active), AdminUser, true, new(Admin, Admin, "reason"));
        Assert.Equal(Forbidden, result.FailureCode);
    }
    [Fact] public void NoOpAndStaleEditAreDistinct()
    {
        Assert.False(AccountAdministrationRules.ChangeRole(AdminUser, AdminUser, false, new(Admin, Admin, "reason")).Changed);
        Assert.Equal(StaleState, AccountAdministrationRules.ChangeRole(AdminUser, AdminUser, true, new(Admin, Member, "reason")).FailureCode);
        Assert.Equal(StaleState, AccountAdministrationRules.ChangeStatus(AdminUser, AdminUser, true, new(true, false, "reason")).FailureCode);
    }
    [Fact] public void InactiveAdminCanBeDemotedAndMemberReactivated()
    {
        Assert.True(AccountAdministrationRules.ChangeRole(AdminUser, new(2, Admin, false), false, new(Member, Admin, "reason")).Allowed);
        var result = AccountAdministrationRules.ChangeStatus(AdminUser, new(2, Member, false), false, new(true, false, "  reviewed  "));
        Assert.True(result.Changed); Assert.Equal("reviewed", result.NormalizedReason);
    }
}
