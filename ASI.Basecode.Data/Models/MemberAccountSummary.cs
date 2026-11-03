namespace ASI.Basecode.Data.Models
{
    /// <summary>
    /// Read-only projection of User fields needed to display linked member rows.
    /// The User/Auth entity remains owned by the Authentication module.
    /// </summary>
    public class MemberAccountSummary
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }
    }
}
