
using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class Broadcastmessage
    {
        [Key]
        public int MessageId { get; set; }
        public string messageheading { get; set; }
        public string Message { get; set; }
       public DateTime broadcastDate { get; set; }
        public int AId { get; set; }
    }
}
