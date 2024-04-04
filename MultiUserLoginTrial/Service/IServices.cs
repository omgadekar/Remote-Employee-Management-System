using MultiLogin.Models;
using System.Drawing;

namespace MultiUserLoginTrial.Service
{
    public interface IServices
    {

        public string SendLoginCredentials(string email, string username, string password, HttpContext httpContext);
        public void WriteTotalHoursToFile(string? userEmail, TimeSpan value);

        public void CalculateAndSaveTotalHours(Users usertracking);

        public Bitmap CaptureWebpageScreenshot();


	}
}