namespace ASI.Basecode.Services.AccountAdministration;

public static class AccountAdministrationCodes
{
    public const string Admin = "admin", Officer = "officer", Member = "member";
    public const string InvalidInput = "invalid_input", Forbidden = "forbidden", InvalidRole = "invalid_role",
        InvalidReason = "invalid_reason", StaleState = "stale_account_state", LastAdmin = "last_active_administrator";
    public static bool IsRole(string role) => role is Admin or Officer or Member;
}
