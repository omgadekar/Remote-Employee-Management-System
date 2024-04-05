using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Drawing;
using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;
using System.Globalization;

namespace MultiUserLoginTrial.Service
{
    public class Service : IServices
    {

		//private readonly IServiceScopeFactory _scopeFactory;
		//private readonly string _screenshotFolderPath;
		private static string totalHoursBasePath = @"C:\TotalHours\";
        private readonly ApplicationDBContext dbContext;

        public Service(ApplicationDBContext dbContext, IServiceScopeFactory scopeFactory/*, string screenshotFolderPath*/)
        {
            this.dbContext = dbContext;
			//_scopeFactory = scopeFactory;
			//_screenshotFolderPath = screenshotFolderPath;
		}



        /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        [NonAction]
        public string SendLoginCredentials(string email, string username, string password, HttpContext httpContext)
        {
            var loginUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}/Login/Login";

            var fromEmail = new MailAddress("gadekaromus@gmail.com", "Login");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "wwtjmxjgbgjtspxu";
            string subject = "Your Account is Successfully Created!";

            string body = $"<br/>Dear {username},<br/><br/>We are excited to inform you that your account has been successfully created.<br/>" +
                $"Below are your login credentials:<br/><br/>Username: {email}<br/>Password: {password}<br/><br/>" +
                $"You can log in using the following link:<br/><a href='{loginUrl}'>{loginUrl}</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587, // Gmail SMTP port
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                try
                {
                    smtp.Send(message);
					return "The email was sent successfully!";

					
                }
                catch (Exception ex)
                {
					return "The email could not be sent.";
					
                }
            }
        }

		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		public void WriteTotalHoursToFile(string? ID, TimeSpan value)
        {
            string filePath = Path.Combine(@"D:\CODE\.NET C#\MultiUserLoginTrial\MultiUserLoginTrial\App_Data", "totalHours.txt");

            try
            {
                string formattedTotalHours = value.ToString(@"hh\:mm\:ss");

                string logEntry = $"{DateTime.Now}: USER ID - {ID} : Total Hours - {formattedTotalHours}{Environment.NewLine}";
                System.IO.File.AppendAllText(filePath, logEntry);
                Console.WriteLine($"Total hours written to file: {logEntry}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing total hours to file: {ex.Message}");
            }
        }

		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/



		public void CalculateAndSaveTotalHours(Users usertracking)
        {
            
                //Console.WriteLine($"Debug - LoginTime: {usertracking.LoginTime}, LogoutTime: {usertracking.LogoutTime}");

                //  usertracking.TotalHours = usertracking.LogoutTime.Value - usertracking.LoginTime.Value;
                usertracking.TotalHours = TimeSpan.FromMilliseconds(usertracking.ElapsedMilliseconds);


                // Save total hours to file
                //   WriteTotalHoursToFile(usertracking.UId.ToString(),usertracking.TotalHours);
                SaveTotalHours(usertracking.UId, usertracking.TotalHours.ToString(@"hh\:mm\:ss"));

            
                //Console.WriteLine($"Debug - LoginTime or LogoutTime is null");
            
        }


		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		public void SaveTotalHours(int userId, string totalHours)
        {
            // Get the user's admin and email       
            var user = dbContext.Users.FirstOrDefault(u => u.UId == userId);
            var admin = dbContext.Admin.FirstOrDefault(a => a.AId == user.AId);
            var adminEmail = admin.AdminEmail;
            var userEmail = user.UserEmail;

            // Create directories if they don't exist
            var adminDirectoryPath = Path.Combine(totalHoursBasePath, adminEmail);
            if (!Directory.Exists(adminDirectoryPath))
            {
                Directory.CreateDirectory(adminDirectoryPath);
            }

            var userDirectoryPath = Path.Combine(adminDirectoryPath, userEmail);
            if (!Directory.Exists(userDirectoryPath))
            {
                Directory.CreateDirectory(userDirectoryPath);
            }

			// Create or append to the totalHours.txt file
			var filePath = Path.Combine(userDirectoryPath, "totalHours.txt");
			if (System.IO.File.Exists(filePath))
			{
				var allLines = System.IO.File.ReadAllLines(filePath).ToList();
				var lastLine = allLines.Last();
                Console.WriteLine(lastLine.Split(' ')[0]);
				Console.WriteLine(lastLine.Split(' ')[1]);
				Console.WriteLine(lastLine.Split(' ')[2]);
				var lastDate = DateTime.ParseExact(lastLine.Split(' ')[0] + " " + lastLine.Split(' ')[1] + " " + lastLine.Split(' ')[2], "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
				if (lastDate.Date < DateTime.Now.Date)
				{
					// Calculate total hours for the previous day
					var previousDayLines = allLines.Where(line => DateTime.ParseExact(line.Split(' ')[0] + " " + line.Split(' ')[1] + " " + line.Split(' ')[2], "dd-MM-yyyy hh:mm:ss tt", CultureInfo.InvariantCulture).Date == lastDate.Date).ToList();
					TimeSpan totalHoursPreviousDay = TimeSpan.Zero;
					foreach (var line in previousDayLines)
					{
						var hours = TimeSpan.Parse(line.Split(new string[] { "Total Hours - " }, StringSplitOptions.None)[1].Trim());
						totalHoursPreviousDay += hours;
					}
					// Add total hours of the previous day to the file
					allLines.Add($"{lastDate:dd-MM-yyyy hh:mm:ss tt} : USER ID - {userId} : Total Hours of day = {totalHoursPreviousDay.ToString(@"hh\:mm\:ss")}");
					System.IO.File.WriteAllLines(filePath, allLines);
				}
			}
			var entry = $"{DateTime.Now} : USER ID - {userId} : Total Hours - {totalHours}\n";
			System.IO.File.AppendAllText(filePath, entry);

		}



		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

		public Bitmap CaptureWebpageScreenshot()
		{
			//To get Display Resolution dynamically adding adding addition methid in wwwroot/js /site.js file
			//then added that site.js script to _Layout.cshtml file
			//Added to new int variables to store the width and height of the screen

			var bmp = new Bitmap(1920, 1080);

			using (var g = Graphics.FromImage(bmp))
			{
				g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
			}

			return bmp;
		}
















	}
}
