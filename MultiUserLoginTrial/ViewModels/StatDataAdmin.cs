namespace MultiUserLoginTrial.ViewModels
{
    public class StatDataAdmin
    {
        public List<string> Roles { get; set; }
        public List<(string, int)> RoleAndCount { get; set; }
    }
}
