using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models;

public sealed record ListFilterOption(string Value, string Label);
public sealed record ListFilterField(string Name, string Label, string Type = "text", IReadOnlyList<ListFilterOption> Options = null, bool AllowEmpty = true);

/// <summary>Each Razor surface declares its filters; the shared partial only renders controls.</summary>
public sealed record ListToolbarViewModel(string SearchLabel, IReadOnlyList<ListFilterField> Fields = null)
{
    public static ListFilterField Status(params ListFilterOption[] options) => new("filter", "Status", Options: options);
    public static ListFilterField Year => new("year", "Year", Options: new[] {
        new ListFilterOption("1", "Year 1"), new("2", "Year 2"), new("3", "Year 3"),
        new("4", "Year 4"), new("5", "Year 5"), new("6", "Year 6") });
}
