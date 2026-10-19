using Xunit;
using ASI.Basecode.Services.AccountAdministration;
using ASI.Basecode.Services.ServiceModels.AccountAdministration;
using static ASI.Basecode.Services.AccountAdministration.AccountAdministrationCodes;
namespace ASI.Basecode.AdminRoles.UnitTests;
public class RegistrationCancellationRulesTests
{
    [Fact] public void FullStateAndBoundaryMatrix()
    {
        var now = new DateTimeOffset(2026,11,2,10,0,0,TimeSpan.Zero);
        foreach(var state in new[]{"pending","waitlisted","confirmed","cancelled"})
        foreach(var evt in new[]{"draft","published","completed","cancelled"})
        foreach(var offset in new[]{-1,0,1})
        foreach(var validAttendance in new[]{false,true})
        {
            var result = RegistrationCancellationRules.Evaluate(new(state,evt,now.AddTicks(offset),validAttendance),now);
            Assert.True(result.Allowed);
            Assert.Equal(offset>0 && (evt=="draft" || evt=="published") && state!="cancelled" && !validAttendance,result.Changed);
        }
    }
    [Fact] public void InvalidatedAttendanceDoesNotPreventCancellation()
    {
        var now = new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);
        Assert.True(RegistrationCancellationRules.Evaluate(new("confirmed","published",now.AddMinutes(1),false),now).Changed);
        Assert.False(RegistrationCancellationRules.Evaluate(new("confirmed","published",now.AddMinutes(1),true),now).Changed);
    }
    [Fact] public void UnknownStatesFailClosed()
    {
        var now=new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);
        Assert.Equal(InvalidInput,RegistrationCancellationRules.Evaluate(new("unknown","published",now.AddDays(1),false),now).FailureCode);
        Assert.False(RegistrationCancellationRules.Evaluate(null,now).Allowed);
    }
    [Fact]
    public void InvalidEventAndMissingTimeAreRejected()
    {
        var now = new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);
        Assert.False(RegistrationCancellationRules.Evaluate(new("confirmed", "unknown", now.AddDays(1), false), now).Allowed);
        Assert.False(RegistrationCancellationRules.Evaluate(new("confirmed", "published", default, false), now).Allowed);
        Assert.False(RegistrationCancellationRules.Evaluate(new("confirmed", "published", now, false), default).Allowed);
    }

    [Fact]
    public void StartBoundaryComparesInstantsAcrossTimezones()
    {
        var now = new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);
        var sameInstant = now.ToOffset(TimeSpan.FromHours(8));
        Assert.False(RegistrationCancellationRules.Evaluate(new("confirmed", "published", sameInstant, false), now).Changed);
        Assert.True(RegistrationCancellationRules.Evaluate(new("confirmed", "published", sameInstant.AddMilliseconds(1), false), now).Changed);
    }
}
