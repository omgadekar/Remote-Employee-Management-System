using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class LoginData
    {
        [Required]
        public string? EnteredEmail { get; set; }
        [Required]
        public string? EnteredPassword { get; set; }
    }
}
