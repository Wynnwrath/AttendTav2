namespace ASI.Basecode.Resources.Constants
{
    /// <summary>Shared program choices and profile field options.</summary>
    public static class ProfileOptions
    {
        /// <summary>The organization's current program list; add a program here when it changes.</summary>
        public static readonly IReadOnlyList<string> Courses = new[] { "BSCS", "BSIT" };

        // Recognize legacy long labels without rewriting historical registration snapshots.
        public static string? NormalizeCourse(string? course) => course?.Trim().ToUpperInvariant() switch
        {
            "BSIT" or "BS INFORMATION TECHNOLOGY" => "BSIT",
            "BSCS" or "BS COMPUTER SCIENCE" => "BSCS",
            _ => course?.Trim()
        };

        /// <summary>Positive year levels offered by the form.</summary>
        public static readonly IReadOnlyList<int> YearLevels = new[] { 1, 2, 3, 4, 5 };

        /// <summary>Sections offered by the form.</summary>
        public static readonly IReadOnlyList<string> Sections = new[] { "A", "B", "C", "D", "E", "F" };

        /// <summary>Membership types the organization uses.</summary>
        public static readonly IReadOnlyList<string> MembershipTypes = new[] { "Regular", "Officer" };
    }
}
