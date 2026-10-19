using Xunit;
using ASI.Basecode.Services.AccountAdministration;
using ASI.Basecode.Services.ServiceModels.AccountAdministration;
using static ASI.Basecode.Services.AccountAdministration.AccountAdministrationCodes;
namespace ASI.Basecode.AdminRoles.UnitTests;
public class ResponseContractTests
{
    [Fact] public void SafeSummaryDoesNotExposeEntityCredentials()
    {
        var json=System.Text.Json.JsonSerializer.Serialize(new AccountSummaryResponse(1,"Name","mail@example.test",Member,true));
        Assert.DoesNotContain("Password",json); Assert.DoesNotContain("Token",json);
        Assert.Contains("mail@example.test",json);
    }
    [Theory] [InlineData(0,0)] [InlineData(1,1)] [InlineData(25,1)] [InlineData(26,2)] [InlineData(int.MaxValue,85899346)]
    public void TotalPagesUseFixedSizeWithoutOverflow(int count,int pages)
    {
        var response=new AccountPageResponse(Array.Empty<AccountSummaryResponse>(),1,count);
        Assert.Equal(25,response.PageSize); Assert.Equal(pages,response.TotalPages);
    }
}
