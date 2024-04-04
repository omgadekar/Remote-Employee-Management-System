using Azure.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLogin.Models
{
    public class Leaves
    {
        [Key]
        public int leaveId { get; set; }
        public string leaveStatus { get; set; }
        public string? leaveStatusMessage { get; set; }
        public string leaveMessageHeading { get; set; }
        public string leaveMessageBody { get; set; }
        public int UserId { get; set; }
        public int AdminId { get; set; }
        public DateTime DateTime { get; set; }
        [NotMapped]
        public string username { get; set; }
        [NotMapped]
        public string userDesignation { get; set; }
        [NotMapped]
        public string useremail { get; set; }
    }
}
