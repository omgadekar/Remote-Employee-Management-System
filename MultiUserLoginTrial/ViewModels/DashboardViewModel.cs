using MultiLogin.Models;

namespace MultiUserLoginTrial.ViewModels
{
	public class DashboardViewModel
	{
		public List<Admin>	admins { get; set; }
		public List<Users> users { get; set; }
		public List<Interests> interests { get; set; }
	}
}
