using System.ComponentModel.DataAnnotations.Schema;

namespace MultiLogin.Models
{
    public class BrowseScreenshotsViewModel
	{
		[NotMapped]
			public string CurrentPath { get; set; }
		[NotMapped]
			public string[] Directories { get; set; }
		[NotMapped]
			public string[] Screenshots { get; set; }
		

	}
}
