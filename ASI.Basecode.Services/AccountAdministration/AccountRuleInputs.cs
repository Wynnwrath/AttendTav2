namespace ASI.Basecode.Services.AccountAdministration;

// Trusted current snapshots supplied by the future service, not HTTP inputs or EF entities.
public sealed record AccountRuleInput(int Id, string Role, bool IsActive);
public sealed record RegistrationRuleInput(string Status, string EventStatus, System.DateTimeOffset StartsAt, bool HasValidAttendance);
