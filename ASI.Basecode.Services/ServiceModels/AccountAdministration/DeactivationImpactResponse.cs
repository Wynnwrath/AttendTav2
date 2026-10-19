namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record RegistrationImpactDetail(int RegistrationId, int EventId, string EventTitle, System.DateTimeOffset StartsAt, string Status);
public sealed record CancellationCounts(int Pending, int Waitlisted, int Confirmed);
public sealed record DeactivationImpactResponse(int AccountId, bool IsActive, System.DateTimeOffset ComputedAt,
    CancellationCounts Counts, System.Collections.Generic.IReadOnlyList<RegistrationImpactDetail> Items, int Page, int TotalCount)
{
    public int PageSize => 25;
    public int TotalPages => (int)((TotalCount + 24L) / 25);
}

