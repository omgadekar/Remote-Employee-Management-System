using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.Web;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using MultiUserLoginTrial.Repository.IRepository;
using MultiUserLoginTrial.Service;
using MultiLogin.Models;
using MultiUserLoginTrial.DataAccess.Data;


namespace MultiUserLoginTrial.Controllers
{
    public class LoginController : Controller
	{
        StringBuilder windowClass = new StringBuilder(1024); // Increase the size
        const string SessionUserId = "_UserID";
		const string SessionUserTime = "_SessionUserTime";
        const string SessionAdminId = "_AdminID";
		const string SessionAdminTime = "_SessionAdminTime";
		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/

		public static ConcurrentDictionary<Guid, CancellationTokenSource> TokenSources = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        public static HttpListener Listener;
        public static CancellationTokenSource CancellationTokenSource;

		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		private static string screenshotFolderPath = @"C:\ScreenshortFile\";
        private const string BaseFolderPath = @"C:\UrlFile\";

        private ApplicationDBContext dbContext;
		 int UserID;
		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IServiceScopeFactory _scopeFactory;

		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


		private readonly IUnitOfWorks _unitOfWork;
		private readonly IServices Services;
		
		public LoginController( ApplicationDBContext dbContext, IServiceScopeFactory serviceScopeFactory, IServiceScopeFactory scopeFactory, IUnitOfWorks _unitOfWork, IServices Services)
		{
            
			this._unitOfWork = _unitOfWork;
            this.dbContext = dbContext;
            _serviceScopeFactory = serviceScopeFactory;
            _scopeFactory = scopeFactory;
			this.Services = Services;
			
			

        }
		/*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		public IActionResult Login()
		{
			return View();
		}
        public IActionResult SignUp() { return View(); }

        [HttpPost]
        public IActionResult SignUp(string emailaddress)
        {
			try {
				var interst = new Interests { Email = emailaddress};
				dbContext.Interest.Add(interst);
				dbContext.SaveChanges();
                TempData["UserRequestSuccessMessage"] = "Submitted successfully We'll Contact you Soon!";
                return View();	
            }
			catch (Exception ex)
			{
				TempData["UserRequestErrorMessage"] = "Error Occured";
				return View();
			}

            return View();
        }


        /*-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
		public IActionResult Login(LoginData viewModel)
		{
			DateTime time = DateTime.Now;
			string enteredEmail = viewModel.EnteredEmail;
			string enteredPassword = viewModel.EnteredPassword;

			var role = _unitOfWork.Roles.FirstOrDefault(r => r.Emails == enteredEmail);

			if (role != null)
			{
				switch (role.Role)
				{
					case "User":
						var user = _unitOfWork.Users.GetUserByEmail(enteredEmail);
						if (user != null)
						{
							var passwordHasher = new PasswordHasher<Users>();
							if (passwordHasher.VerifyHashedPassword(user, user.UserPassword, enteredPassword) == PasswordVerificationResult.Success && user.IsActive != 0)
							{
								
									UserID = user.UId;
									HttpContext.Session.SetInt32(SessionUserId, UserID);
									HttpContext.Session.SetString(SessionUserTime, time.ToString());
									HttpContext.Session.SetString("Role", role.Role);
									HttpContext.Session.SetString("Email", enteredEmail);
									HttpContext.Session.SetString("Name", user.UserName);
									DateTime logintime = DateTime.Now;
									HttpContext.Session.SetString("LoginTime", logintime.ToString());
									var cts = new CancellationTokenSource();
									var guid = Guid.NewGuid();
									TokenSources[guid] = cts;
									HttpContext.Session.SetString("TaskToken", guid.ToString());

									Listener = new HttpListener();
									Listener.Prefixes.Add("http://localhost:5000/");
									Task.Run(() => CaptureUrls(UserID, cts.Token, Listener));

									CancellationTokenSource = new CancellationTokenSource();
									Task.Run(() => CaptureScreenshotsPeriodically(UserID, CancellationTokenSource.Token));
									return RedirectToAction("UserDashboard", "Dashboard");
								
							}
							else
							{
								ViewBag.Error = "INVALID CREDENTIALS";

							}
						}
						break;
					case "Admin":
						var admin = _unitOfWork.Admin.GetAdminByEmail(enteredEmail);
						if (admin != null)
						{
							var passwordHasher = new PasswordHasher<Admin>();
							if (passwordHasher.VerifyHashedPassword(admin, admin.AdminPassword, enteredPassword) == PasswordVerificationResult.Success && admin.IsActive != 0)
							{
								HttpContext.Session.SetInt32(SessionAdminId, admin.AId);
								HttpContext.Session.SetString(SessionAdminTime, time.ToString());
								HttpContext.Session.SetString("Role", role.Role);
								HttpContext.Session.SetString("Email", enteredEmail);
								HttpContext.Session.SetString("Name", admin.AdminName);
								return RedirectToAction("AdminDashboard", "Dashboard");
							}
							else
							{
								ViewBag.Error = "INVALID CREDENTIALS";
							}
						}
						break;
					case "SuperAdmin":
						var superAdmin = _unitOfWork.SuperAdmin.GetSuperAdminByEmail(enteredEmail);
						if (superAdmin != null)
						{
							if (enteredPassword == superAdmin.SuperAdminPassword)
							{
								HttpContext.Session.SetString("Role", role.Role);
								HttpContext.Session.SetString("Email", enteredEmail);

								return RedirectToAction("SuperAdminDashboard", "Dashboard");
							}
							else
							{
								ViewBag.Error = "INVALID CREDENTIALS";
							}
						}
						break;
					default:
						return View();
				}
			}
			else
			{
				ViewBag.Error = "INVALID CREDENTIALS";
			}
			return View();
		}







		/* 1 -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		//CODE to Capture URL
		private void SaveWebHistory(int userId, string url)
		{
			using (var scope = _scopeFactory.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();


				// Get the user's admin and email
				var user = dbContext.Users.FirstOrDefault(u => u.UId == userId);
				var admin = dbContext.Admin.FirstOrDefault(a => a.AId == user.AId);
				var adminEmail = admin.AdminEmail;
				var userEmail = user.UserEmail;

				// Create directories if they don't exist
				if (!Directory.Exists(BaseFolderPath))
				{
					Directory.CreateDirectory(BaseFolderPath);
				}
				var adminDirectoryPath = Path.Combine(BaseFolderPath, adminEmail);
				if (!Directory.Exists(adminDirectoryPath))
				{
					Directory.CreateDirectory(adminDirectoryPath);
				}

				var userDirectoryPath = Path.Combine(adminDirectoryPath, userEmail);
				if (!Directory.Exists(userDirectoryPath))
				{
					Directory.CreateDirectory(userDirectoryPath);
				}

				// Create or append to the webHistory.txt file
				var filePath = Path.Combine(userDirectoryPath, "webHistory.txt");
				var entry = $"{DateTime.Now}: USER ID - {userId} : URL - {url}\n";
				System.IO.File.AppendAllText(filePath, entry);
			}
		}

		

		/*2 -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/
		private void CaptureUrls(int userId, CancellationToken token, HttpListener listener)
		{
			try
			{
				listener.Start();
				while (true)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}
					HttpListenerContext context = listener.GetContext();
					HttpListenerResponse response = context.Response;
					HttpListenerRequest request = context.Request;
					string time = request.QueryString["time"];
					string url = HttpUtility.UrlDecode(request.QueryString["url"]);
					string urlwithtime = "[URL: " + url + " | TIME OF ACCESS: " + DateTime.Now + "]";
					string urlJson = JsonConvert.SerializeObject(urlwithtime);

					using (var scope = _serviceScopeFactory.CreateScope())
					{
						var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

						var usertracking = dbContext.Users.FirstOrDefault(u => u.UId == userId);
						if (usertracking != null)
						{
							var urls = string.IsNullOrEmpty(usertracking.UrlsJson)
											? new List<string>()
											: JsonConvert.DeserializeObject<List<string>>(usertracking.UrlsJson);
							urls.Add(urlJson);
							usertracking.UrlsJson = JsonConvert.SerializeObject(urls);
							dbContext.SaveChanges();
						}
					}

					SaveWebHistory(userId, url);

					string responseString = "URL received";
					byte[] buffer = Encoding.UTF8.GetBytes(responseString);
					response.ContentLength64 = buffer.Length;
					Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					output.Close();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
			}
		}



		/* 3 -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------*/


		//SCREENSHOT OF USERS

		private void CaptureScreenshotsPeriodically(int userId, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var screenshot = Services.CaptureWebpageScreenshot();
					var fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

					using (var scope = _scopeFactory.CreateScope())
					{
						var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

						// Get the user's admin's email
						var user = dbContext.Users.FirstOrDefault(u => u.UId == userId);
						var admin = dbContext.Admin.FirstOrDefault(a => a.AId == user.AId);
						var adminEmail = admin.AdminEmail;
						var userEmail = user.UserEmail;

						// Create directories if they don't exist
						var adminDirectoryPath = Path.Combine(screenshotFolderPath, adminEmail);
						if (!Directory.Exists(adminDirectoryPath))
						{
							Directory.CreateDirectory(adminDirectoryPath);
						}

						var userDirectoryPath = Path.Combine(adminDirectoryPath, userEmail);
						if (!Directory.Exists(userDirectoryPath))
						{
							Directory.CreateDirectory(userDirectoryPath);
						}

						var filePath = Path.Combine(userDirectoryPath, fileName);
						screenshot.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

						Thread.Sleep(TimeSpan.FromSeconds(10));
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error capturing screenshot: {ex.Message}");
				}
			}
		}




	}
}
