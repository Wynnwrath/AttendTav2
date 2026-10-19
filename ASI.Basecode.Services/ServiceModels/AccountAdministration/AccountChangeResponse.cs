namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record AccountChangeResponse(int AccountId, string Role, bool IsActive, bool Changed, CancellationCounts Cancelled);
