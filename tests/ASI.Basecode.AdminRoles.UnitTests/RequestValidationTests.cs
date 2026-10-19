using Xunit;
using ASI.Basecode.Services.AccountAdministration;
using ASI.Basecode.Services.ServiceModels.AccountAdministration;
using static ASI.Basecode.Services.AccountAdministration.AccountAdministrationCodes;
namespace ASI.Basecode.AdminRoles.UnitTests;
public class RequestValidationTests
{
    private static readonly AccountRuleInput Actor = new(1, Admin, true);
    [Theory] [InlineData(null)] [InlineData("")] [InlineData("   ")]
    public void MissingReasonRejected(string reason) => Assert.Equal(InvalidReason,
        AccountAdministrationRules.ChangeRole(Actor, Actor, true, new(Member, Admin, reason)).FailureCode);
    [Theory] [InlineData("Admin")] [InlineData("owner")] [InlineData(null)]
    public void OnlyFixedRolesAccepted(string role) => Assert.Equal(InvalidRole,
        AccountAdministrationRules.ChangeRole(Actor, Actor, true, new(role, Admin, "reason")).FailureCode);
    [Fact] public void ReasonBoundaryAndRequiredBooleans()
    {
        Assert.True(AccountAdministrationRules.ChangeRole(Actor, Actor, true, new(Member, Admin, new string('a',1000))).Allowed);
        Assert.Equal(InvalidReason, AccountAdministrationRules.ChangeRole(Actor, Actor, true, new(Member, Admin, new string('a',1001))).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(Actor, Actor, true, new(null, true, "reason")).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(Actor, Actor, true, new(false, null, "reason")).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeRole(Actor, new(0, Member, true), true, new(Admin, Member, "reason")).FailureCode);
    }
    [Theory]
    [InlineData(null,26,1)] [InlineData("abc",26,1)] [InlineData("-1",26,1)]
    [InlineData("0",26,1)] [InlineData("99",26,2)] [InlineData("2",25,1)]
    [InlineData("2",26,2)] [InlineData("99",0,1)] [InlineData("9223372036854775807",26,2)]
    public void PageNormalized(string page,int count,int expected) => Assert.Equal(expected,new AccountListRequest(page).NormalizePage(count));
    [Fact]
    public void NullRequestsAndInvalidActorsFailWithoutThrowing()
    {
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeRole(Actor, Actor, true, null).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(Actor, Actor, true, null).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(null, Actor, true, new(false, true, "reason")).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(new(0, Admin, true), Actor, true, new(false, true, "reason")).FailureCode);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeRole(Actor, null, true, new(Member, Admin, "reason")).FailureCode);
    }

    [Fact]
    public void StatusChangesRequireAReasonAndAuthorizeBeforeNoOp()
    {
        Assert.Equal(InvalidReason, AccountAdministrationRules.ChangeStatus(Actor, Actor, true, new(false, true, " ")).FailureCode);
        Assert.Equal(Forbidden, AccountAdministrationRules.ChangeStatus(new(2, Officer, true), Actor, true, new(true, true, "reason")).FailureCode);
        var unchanged = AccountAdministrationRules.ChangeStatus(Actor, Actor, false, new(true, true, "reason"));
        Assert.True(unchanged.Allowed);
        Assert.False(unchanged.Changed);
    }

    [Fact]
    public void JsonDistinguishesMissingBooleanFromFalse()
    {
        var missing = System.Text.Json.JsonSerializer.Deserialize<ChangeAccountStatusRequest>("{}");
        var provided = System.Text.Json.JsonSerializer.Deserialize<ChangeAccountStatusRequest>(
            "{\"IsActive\":false,\"ExpectedIsActive\":true,\"Reason\":\"reviewed\"}");
        Assert.Null(missing.IsActive);
        Assert.Equal(InvalidInput, AccountAdministrationRules.ChangeStatus(Actor, Actor, true, missing).FailureCode);
        Assert.True(AccountAdministrationRules.ChangeStatus(Actor, Actor, true, provided).Changed);
    }

    [Fact]
    public void PagingProtectsOffsetArithmetic()
    {
        Assert.Equal(1, new AccountListRequest("99999999999999999999999999999").NormalizePage(26));
        var last = new AccountListRequest(long.MaxValue.ToString()).NormalizePage(int.MaxValue);
        Assert.Equal(2147483625, checked((last - 1) * AccountListRequest.PageSize));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AccountListRequest().NormalizePage(-1));
    }
}
