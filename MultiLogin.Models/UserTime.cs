
using System.ComponentModel.DataAnnotations;

namespace MultiLogin.Models
{
    public class UserTime
    {
        [Key]
        public int TimeSheetId { get; set; }
        public int UserId { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }
        public DateTime Date { get; set; }
        public string Duration { get; set; }
        public int AdminId { get; set; }
    }
}
