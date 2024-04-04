using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiLogin.Models
{
    public class Users
    {
        [Key]
        public int UId { get; set; }
        [Required(ErrorMessage = "EMAIL ID REQUIRED")]
        public string? UserEmail { get; set; }
        [Required(ErrorMessage = "PASSWORD REQUIRED")]
        [StringLength(450)]
        public string? UserPassword { get; set; }
        [Required(ErrorMessage = "ACCOUNT STATUS REQUIRED")]
        public int IsActive { get; set; }
        public int AId { get; set; }
        public string? UrlsJson { get; set; }

        [Required(ErrorMessage = "NAME REQUIRED")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "CITY REQUIRED")]
        public string UserCity { get; set; }
        [Required(ErrorMessage = "DESIGNATION REQUIRED")]
        public string UserDesignation { get; set; }
        //NOT MAPPED PART OF USER FOR TIME CALCULATION
        //Changes of 30-01-2024 Addded StarTime and ElapsedMilliSecond 
        [NotMapped]
        public DateTime? StartTime { get; set; }
        [NotMapped]
        public long ElapsedMilliseconds { get; set; }
        [NotMapped]
        //
        public DateTime? LoginTime { get; set; }
        [NotMapped]
        public DateTime? LogoutTime { get; set; }
        [NotMapped]
        public TimeSpan TotalHours { get; set; }
        [NotMapped]
        public DateTime? LastLogoutTime { get; set; }

        public Users()
        {
            // Initialize TotalHours to zero when a new user is created
            TotalHours = TimeSpan.Zero;
        }
        [NotMapped]
        public string FormattedTotalHours
        {
            get
            {
                return CalculateTotalHours.ToString(@"hh\:mm\:ss");
            }
        }
        [NotMapped]
        public TimeSpan CalculateTotalHours
        {
            get
            {
                if (LogoutTime.HasValue && LoginTime.HasValue)
                {
                    return LogoutTime.Value.Subtract(LoginTime.Value);
                }
                else
                {
                    return TimeSpan.Zero;
                }
            }

        }
		//public Admin Admin { get; set; }



	}
}
