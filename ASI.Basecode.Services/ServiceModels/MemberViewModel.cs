using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class MemberViewModel
    {
        public int MemberId { get; set; }

        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        [Required(ErrorMessage = "Student number is required.")]
        [StringLength(50)]
        public string StudentNumber { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        [StringLength(100)]
        public string Course { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Year level must be greater than 0.")]
        public int YearLevel { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        [StringLength(50)]
        public string Section { get; set; }

        [StringLength(30)]
        public string MobileNumber { get; set; }

        [StringLength(100)]
        public string MembershipType { get; set; }

        [Required]
        [RegularExpression("^(active|inactive)$", ErrorMessage = "Status must be active or inactive.")]
        public string Status { get; set; }
    }
}
