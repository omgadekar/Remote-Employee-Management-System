using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLogin.Models
{
    public class Interests
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
    }
}
