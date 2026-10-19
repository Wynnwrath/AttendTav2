namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record AccountPageResponse(System.Collections.Generic.IReadOnlyList<AccountSummaryResponse> Items, int Page, int TotalCount)
{
    public int PageSize => 25;
    public int TotalPages => (int)((TotalCount + 24L) / 25);
}

