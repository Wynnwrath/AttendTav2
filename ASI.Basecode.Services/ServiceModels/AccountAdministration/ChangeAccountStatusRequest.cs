namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record ChangeAccountStatusRequest(bool? IsActive, bool? ExpectedIsActive, string Reason);
