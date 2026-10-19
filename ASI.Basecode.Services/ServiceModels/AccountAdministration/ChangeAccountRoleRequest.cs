namespace ASI.Basecode.Services.ServiceModels.AccountAdministration;

public sealed record ChangeAccountRoleRequest(string Role, string ExpectedRole, string Reason);
