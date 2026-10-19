namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record AccountListRequest(string Page = null)
{
    public const int PageSize = 25;
    public int NormalizePage(int totalCount)
    {
        if (totalCount < 0) throw new System.ArgumentOutOfRangeException(nameof(totalCount));
        if (!long.TryParse(Page, out var page) || page < 1) page = 1;
        var last = System.Math.Max(1, (totalCount + 24L) / PageSize);
        return (int)System.Math.Min(page, last);
    }
}

