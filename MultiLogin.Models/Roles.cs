using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class Roles
    {
        [Key]
        [Required]
        public string? Emails { get; set; }
        [Required]
        public string? Role { get; set; }
    }
}
