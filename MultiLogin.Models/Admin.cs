using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class Admin
    {
        [Key]
        public int AId { get; set; }
        [Required(ErrorMessage = "Email Required")]
        public string AdminEmail { get; set; }
        [Required(ErrorMessage = "Password Required")]
        [StringLength(450)]
        public string AdminPassword { get; set; }
        [Required(ErrorMessage = "Select Account Status")]
        [Range(0, 1)]
        public int IsActive { get; set; }
        [Required(ErrorMessage = "Name Required")]
        public string AdminName { get; set; }
        [Required(ErrorMessage = "City Required")]
        public string AdminCity { get; set; }
        [Required(ErrorMessage = "Organization Name Required")]
        public string OrganizationName { get; set; }

    }
}
