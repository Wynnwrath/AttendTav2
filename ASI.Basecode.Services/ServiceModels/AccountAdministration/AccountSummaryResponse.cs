namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record AccountSummaryResponse(int Id, string FullName, string Email, string Role, bool IsActive);
