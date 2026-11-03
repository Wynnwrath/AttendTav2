using System;

namespace ASI.Basecode.Data.Models
{
    public partial class Member
    {
        public int MemberId { get; set; }

        public int UserId { get; set; }

        public string StudentNumber { get; set; }

        public string Course { get; set; }

        public int YearLevel { get; set; }

        public string Section { get; set; }

        public string MobileNumber { get; set; }

        public string MembershipType { get; set; }

        public string Status { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
