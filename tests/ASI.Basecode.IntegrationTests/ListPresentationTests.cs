using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace ASI.Basecode.IntegrationTests;

public sealed class ListPresentationTests
{
    [Fact]
    public void QueryBindingTrimsFiltersBoundsSizeAndUsesManilaCalendarDays()
    {
        var context = new DefaultHttpContext();
        context.Request.RouteValues["controller"] = "Member";
        context.Request.QueryString = new("?search=%20Ana%20&pageSize=999&year=2&from=2026-11-15&to=2026-11-16");
        var result = ListReadOptionsFactory.From(context.Request);
        Assert.Equal("Ana", result.Search);
        Assert.Equal(25, result.PageSize);
        Assert.Equal(2, result.Year);
        Assert.Equal("Member", result.Surface);
        Assert.Equal(new DateTime(2026, 11, 14, 16, 0, 0, DateTimeKind.Utc), result.From);
    }

    [Fact]
    public void ChangingStatusPreservesOtherCriteriaAndRemovesStalePageAndFit()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/Member";
        context.Request.QueryString = new("?search=Ana&course=BSIT&year=2&page=3&fit=1&pageSize=2&status=active");
        var url = ListNavigation.Url(context.Request, new Dictionary<string, string> { ["status"] = "inactive" });
        Assert.Contains("search=Ana", url);
        Assert.Contains("course=BSIT", url);
        Assert.Contains("year=2", url);
        Assert.Contains("status=inactive", url);
        Assert.DoesNotContain("page=", url);
        Assert.DoesNotContain("fit=", url);
    }

    [Theory]
    [InlineData("/Member?search=Ana", true)]
    [InlineData("https://example.com", false)]
    [InlineData("//example.com", false)]
    [InlineData("/\\example.com", false)]
    [InlineData("/Member\r\nLocation:bad", false)]
    public async Task NativePostReturnsOnlyToALocalOrigin(string target, bool local)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.ContentType = "application/x-www-form-urlencoded";
        http.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { ["uiReturnUrl"] = target });
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var original = new RedirectToActionResult("Index", null, null);
        var executing = new ResultExecutingContext(action, filters, original, new object());
        await new ListReturnFilter().OnResultExecutionAsync(executing, () => Task.FromResult(new ResultExecutedContext(action, filters, executing.Result, new object())));
        if (local) Assert.Equal(target, Assert.IsType<LocalRedirectResult>(executing.Result).Url);
        else Assert.Same(original, executing.Result);
    }

    [Fact]
    public void LabelsDescribeRealSingularPluralAndFilteredRecords()
    {
        Assert.Equal("request", ListPagingLabels.Noun("AccountRequest", "Index", 1, "request"));
        Assert.Equal("requests", ListPagingLabels.Noun("AccountRequest", "Index", 6, "request"));
        Assert.Equal("entries", ListPagingLabels.Noun("ActivityLog", "Index", 0));
        var context = new DefaultHttpContext();
        context.Request.QueryString = new("?status=all&page=2");
        Assert.False(ListPagingLabels.IsFiltered(context.Request));
    }
}
