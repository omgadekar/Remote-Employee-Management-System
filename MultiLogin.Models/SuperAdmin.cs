using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class SuperAdmin
    {
        [Key]
        public int SId { get; set; }
        [Required]
        public string? SuperAdminEmail { get; set; }
        [Required]
        [StringLength(450)]
        public string? SuperAdminPassword { get; set; }
        public int IsActive { get; set; }

    }
}
