namespace MultiUserLoginTrial.ViewModels
{
	public class StatDataSuperAdmin
	{
		public List<string> OrganizationNames { get; set; }
		public List<(string, int)> OrganizationAndNumOfUsers { get; set; }
	}
}
