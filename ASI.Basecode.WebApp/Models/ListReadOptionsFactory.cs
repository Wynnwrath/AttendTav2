using System;
using System.Globalization;
using ASI.Basecode.Data.Queries;
using Microsoft.AspNetCore.Http;

namespace ASI.Basecode.WebApp.Models;

public static class ListReadOptionsFactory
{
    public static ListReadOptions From(HttpRequest request)
    {
        if (request == null) return new();
        var query = request.Query;
        string Text(string key) => string.IsNullOrWhiteSpace(query[key]) ? null : query[key].ToString().Trim();
        // Date filters describe Manila calendar days; list timestamps are stored in UTC.
        DateTime? Date(string key) => DateTime.TryParseExact(Text(key), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) && date.Year is >= 1900 and < 9999
            ? DateTime.SpecifyKind(date.AddHours(-8), DateTimeKind.Utc) : null;
        return new ListReadOptions
        {
            Surface = request.RouteValues["controller"]?.ToString() ?? request.Path.Value?.Trim('/').Split('/')[0],
            PageSize = int.TryParse(Text("pageSize"), out var size) ? Math.Clamp(size, 1, 25) : 5,
            Search = Text("search"), Filter = Text("filter"), Course = Text("course"),
            Year = int.TryParse(Text("year"), out var year) && year is >= 1 and <= 10 ? year : null,
            Type = Text("type"), Role = Text("role"), From = Date("from"), To = Date("to")
        };
    }
}
