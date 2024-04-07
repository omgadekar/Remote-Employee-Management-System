    
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.IO.Compression;
using MultiUserLoginTrial.Repository.IRepository;
using MultiUserLoginTrial.Repository;
using MultiUserLoginTrial.Service;
using MultiLogin.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using MultiUserLoginTrial.DataAccess.Data;
using MultiUserLoginTrial.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;

namespace MultiUserLoginTrial.Controllers
{

    public class DashboardController : Controller
    {
        //
        private static string totalHoursBasePath = @"C:\TotalHours\";
        private static string screenshotFolderPath = @"C:\ScreenshortFile\";
        private const string BaseFolderPath = @"C:\UrlFile\";

        //
        public long TimeElapsedInMilliseconds;
        private const string SessionUserTime = "UserTime";
        private const string SessionUserId = "UserId";

        //
        bool startflag = false;
        bool storedflag = false;
        //
        private readonly IServices Services;
        private readonly ApplicationDBContext DbContext;
        private readonly IUnitOfWorks _unitOfWorks;
        public DashboardController(ApplicationDBContext dBContext,IUnitOfWorks unitOfWorks, IServices Services)
        {
			DbContext = dBContext;
			_unitOfWorks = unitOfWorks;
            this.Services = Services;
          
        }
        //USER DASHBOARD 
       //[Route("DisplayAllUsers")] //Multiple URL Routing
        [Route("UserDisplay")]
        public IActionResult UserDashboard()
        {

            return View();
        }
        //-------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult DeleteClientEmail(int id) {
            try
            {
                var interset = new Interests { Id = id };
                DbContext.Interest.Remove(interset);
                DbContext.SaveChanges();
            }
            catch (Exception e) { 
            }
            return RedirectToAction("SuperAdminDashboard", "Dashboard");
        }


        //To Display AdminDashBoard with List of all the users
   
        public IActionResult AdminDashboard()
        {
            int adminID = HttpContext.Session.GetInt32("_AdminID") ?? 0;
            var users = _unitOfWorks.Users.GetUsersByAdminId(adminID).ToList();
            return View(users);
        }


        //Used to add new users into the record
        public IActionResult AddUser()
        {
            TempData.Keep();
            return View();
        }
        [HttpPost]
        public IActionResult AddUser(Users viewModel)
        {
            int adminID = HttpContext.Session.GetInt32("_AdminID") ?? 0;

            var user = new Users
            {
                UserEmail = viewModel.UserEmail,
                UserDesignation = viewModel.UserDesignation,
                UserCity = viewModel.UserCity,
                UserName = viewModel.UserName,
                UserPassword = viewModel.UserPassword,
                IsActive = viewModel.IsActive,
                AId = adminID
            };

            var addingRole = new Roles
            {
                Emails = viewModel.UserEmail,
                Role = "User"
            };

            if (user != null)
            {
                var passwordHasher = new PasswordHasher<Users>();
                user.UserPassword = passwordHasher.HashPassword(viewModel, user.UserPassword);

                _unitOfWorks.Users.Add(user);
                _unitOfWorks.Users.Add(user);
                _unitOfWorks.Save();

                _unitOfWorks.Roles.Add(addingRole);
                _unitOfWorks.Save();

                var message = Services.SendLoginCredentials(viewModel.UserEmail, viewModel.UserName, viewModel.UserPassword, HttpContext);
                if (message.Equals("The email was sent successfully!"))
                {
                    TempData["SuccessMessage"] = "The email was sent successfully!";

                }
                else {
                    TempData["ErrorMessage"] = "The email could not be sent.";

                }

                return RedirectToAction("AdminDashboard");
            }
            else
            {
                ViewBag.Error = "Incomplete Data";
                return View();
            }
        }




        //Editing Record of exisiting User
        [HttpGet] //Added Custom Url Routing
        public IActionResult EditUser(int id)
        {
            var user = _unitOfWorks.Users.GetById(id);
            return View(user);
        }
        [HttpPost]
        public IActionResult EditUser(Users viewModel)
        {
            var userData = _unitOfWorks.Users.GetById(viewModel.UId);
            var roleData = _unitOfWorks.Roles.FirstOrDefault(r => r.Emails == userData.UserEmail);

            if (userData != null)
            {
                bool isEmailChanged = !userData.UserEmail.Equals(viewModel.UserEmail);
                bool isPasswordChanged = !userData.UserPassword.Equals(viewModel.UserPassword);

                userData.UserEmail = viewModel.UserEmail;
                userData.UserPassword = viewModel.UserPassword;
                userData.UserName = viewModel.UserName;
                userData.UserDesignation = viewModel.UserDesignation;
                userData.UserCity = viewModel.UserCity;
                userData.IsActive = viewModel.IsActive;

                if (roleData != null)
                {
                    _unitOfWorks.Roles.Remove(roleData);
                    _unitOfWorks.Save();

                    var newRole = new Roles
                    {
                        Emails = viewModel.UserEmail,
                        Role = roleData.Role
                    };

                    _unitOfWorks.Roles.Add(newRole);
                }

                if (isEmailChanged || isPasswordChanged)
                {
                    var passwordHasher = new PasswordHasher<Users>();
                    userData.UserPassword = passwordHasher.HashPassword(viewModel, userData.UserPassword);
                   var message = Services.SendLoginCredentials(viewModel.UserEmail, viewModel.UserEmail, viewModel.UserPassword, HttpContext);
					if (message.Equals("The email was sent successfully!"))
					{
						TempData["SuccessMessage"] = "The email was sent successfully!";

					}
					else
					{
						TempData["ErrorMessage"] = "The email could not be sent.";

					}
				}

                _unitOfWorks.Save();
                return RedirectToAction("AdminDashboard");
            }
            return View();
        }



        //To Delete record of User
       
        [HttpGet] //Added Custom Url Routing
        public IActionResult DeleteUser(int id)
        {
            var user = _unitOfWorks.Users.GetById(id);
            return View(user);
        }
        [HttpPost]
        public IActionResult DeleteUser(Users viewModel)
        {
            var user = _unitOfWorks.Users.GetById(viewModel.UId);
            var role = _unitOfWorks.Roles.FirstOrDefault(r => r.Emails == viewModel.UserEmail);

            if (user != null && role != null)
            {
                _unitOfWorks.Users.Remove(user);
                _unitOfWorks.Roles.Remove(role);
                _unitOfWorks.Save();

                return RedirectToAction("AdminDashboard");
            }

            return View();
        }






        //OPERATIONS DONE BY THE SUPERADMIN


        //To Display SuperAdminDashBoard with List of all the Admins
        [Route("SuperAdminDashboard")] //Custom URL routing

        public IActionResult SuperAdminDashboard()
        {
            var admin = _unitOfWorks.Admin.GetAll().ToList();
            var users = _unitOfWorks.Users.GetAll().ToList();
            var interst = DbContext.Interest.ToList();
            var DashboardData = new DashboardViewModel
            {
                admins = admin,
                users = users,
                interests = interst,
            };
            return View(DashboardData);
        }


        //Used to add new Admins into the record
        public IActionResult AddAdmin()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddAdmin(Admin viewModel)
        {
            var admins = new Admin
            {
                AdminEmail = viewModel.AdminEmail,
                AdminPassword = viewModel.AdminPassword,
                IsActive = viewModel.IsActive,
                AdminName = viewModel.AdminName,
                AdminCity = viewModel.AdminCity,
                OrganizationName = viewModel.OrganizationName
            };

            var AddingRole = new Roles
            {
                Emails = viewModel.AdminEmail,
                Role = "Admin"
            };

            if (admins != null)
            {
                var passwordHasher = new PasswordHasher<Admin>();
                admins.AdminPassword = passwordHasher.HashPassword(viewModel, admins.AdminPassword);

                _unitOfWorks.Admin.Add(admins);
                //_unitOfWorks.Save();
                _unitOfWorks.Roles.Add(AddingRole);
                _unitOfWorks.Save();
				var message = Services.SendLoginCredentials(viewModel.AdminEmail, viewModel.AdminName, viewModel.AdminPassword, HttpContext);
				if (message.Equals("The email was sent successfully!"))
				{
					TempData["SuccessMessage"] = "The email was sent successfully!";

				}
				else
				{
					TempData["ErrorMessage"] = "The email could not be sent.";

				}

				return RedirectToAction("SuperAdminDashboard");
            }
            else
            {
                ViewBag.Error = "Incomplete Data";
                return View();
            }
        }


        //Editing Record of exisiting Admins
    
        [HttpGet]
        public IActionResult EditAdmin(int id)
        {
            var user = _unitOfWorks.Admin.GetById(id);
            return View(user);
        }

        [HttpPost]
        public IActionResult EditAdmin(Admin viewModel)
        {
            var adminData = _unitOfWorks.Admin.GetById(viewModel.AId);
            var roleData = _unitOfWorks.Roles.FirstOrDefault(r => r.Emails == adminData.AdminEmail);

            if (adminData != null)
            {
                bool isEmailChanged = !adminData.AdminEmail.Equals(viewModel.AdminEmail);
                bool isPasswordChanged = !adminData.AdminPassword.Equals(viewModel.AdminPassword);

                adminData.AdminEmail = viewModel.AdminEmail;
                adminData.AdminPassword = viewModel.AdminPassword;
                adminData.IsActive = viewModel.IsActive;
                adminData.AdminName = viewModel.AdminName;
                adminData.AdminCity = viewModel.AdminCity;
                adminData.OrganizationName = viewModel.OrganizationName;

                if (roleData != null)
                {
                    _unitOfWorks.Roles.Remove(roleData);
                    _unitOfWorks.Save();

                    var newRole = new Roles
                    {
                        Emails = viewModel.AdminEmail,
                        Role = roleData.Role
                    };

                    _unitOfWorks.Roles.Add(newRole);
                }

                if (isEmailChanged || isPasswordChanged)
                {
                    var passwordHasher = new PasswordHasher<Admin>();
                    adminData.AdminPassword = passwordHasher.HashPassword(viewModel, adminData.AdminPassword);
                  var message=  Services.SendLoginCredentials(viewModel.AdminEmail, viewModel.AdminEmail, viewModel.AdminPassword, HttpContext);
					if (message.Equals("The email was sent successfully!"))
					{
						TempData["SuccessMessage"] = "The email was sent successfully!";

					}
					else
					{
						TempData["ErrorMessage"] = "The email could not be sent.";

					}
				}

                _unitOfWorks.Save();
                return RedirectToAction("SuperAdminDashboard");
            }
            return View();
        }



        //To Delete record of Admins
        [HttpGet]
        public IActionResult DeleteAdmin(int id)
        {
            var user = _unitOfWorks.Admin.GetById(id);
            return View(user);
        }


        [HttpPost]
        public IActionResult DeleteAdmin(Admin viewModel)
        {
            var admin = _unitOfWorks.Admin.GetById(viewModel.AId);
            var role = _unitOfWorks.Roles.FirstOrDefault(r => r.Emails == viewModel.AdminEmail);
            var users = _unitOfWorks.Users.GetUsersByAdminId(viewModel.AId).ToList();
            var userRoles = _unitOfWorks.Roles.GetRolesByUserEmails(users.Select(u => u.UserEmail)).ToList();

            if (admin != null && role != null)
            {
                if (users.Any())
                {
                    _unitOfWorks.Users.RemoveRange(users);
                    _unitOfWorks.Roles.RemoveRange(userRoles);
                    _unitOfWorks.Save();
                }

                _unitOfWorks.Admin.Remove(admin);
                _unitOfWorks.Roles.Remove(role);
                _unitOfWorks.Save();
                return RedirectToAction("SuperAdminDashboard");
            }
            return View();
        }




        //TIME TRACKING OF USER

        private TimeSpan ReadTotalHoursFromFile(string userName, TimeSpan totalHours)
        {
            string filePath = Path.Combine(@"D:\CODE\.NET C#\MultiUserLoginTrial\MultiUserLoginTrial\App_Data", "totalHours.txt");

            try
            {
                var lines = System.IO.File.ReadAllLines(filePath);

                var userLine = lines.FirstOrDefault(line => line.Contains(userName));

                if (userLine != null)
                {
                    var totalHoursString = userLine.Split('-').LastOrDefault()?.Trim();

                    if (TimeSpan.TryParse(totalHoursString, out var TH))
                    {
                        return TH;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading total hours from file: {ex.Message}");
            }

            return TimeSpan.Zero;
        }



        [HttpPost]
        public ActionResult UpdateTotalHours(TimeSpan? totalHours)
        {
            try
            {
                HttpContext.Session.SetString("StoredStartTime", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                var user = new Users();
                user.UId = HttpContext.Session.GetInt32("_UserID") ?? 0;
                string userlogintime = HttpContext.Session.GetString("_SessionUserTime");
                user.LoginTime = DateTime.Parse(userlogintime);
                if (user != null)
                {
                    if (totalHours.HasValue)
                    {
                        Services.WriteTotalHoursToFile(user.UserEmail, totalHours.Value);
                        // Return total hours as a string
                        //  return Json(user.CalculateTotalHours.ToString(@"hhz\:mm\:ss"));
                    }
                    else
                    {
                        // Handle the case where totalHours is null
                        return Json("Total hours is null");
                    }
                }

                return Json("Invalid user");
            }
            catch (Exception ex)
            {
                // Log the exception for further analysis
                Console.WriteLine($"Exception in UpdateTotalHours: {ex.Message}");
                return Json("An error occurred");
            }
        }

        public ActionResult GetTotalHours()
        {
            var user = new Users();
            user.UId = HttpContext.Session.GetInt32("_UserID") ?? 0;
            string userlogintime = HttpContext.Session.GetString("_SessionUserTime");
            user.LoginTime = DateTime.Parse(userlogintime);


            var result = "N/A";

            // Returning a JSON response
            return Json(result);
        }
        
        public ActionResult StartTracking() //USED TO REDIRECT TO TRACKED PAGE VIEW
        {
            var user = new Users();
            int storedUserId = HttpContext.Session.GetInt32("_UserID") ?? 0;
            string storedUserLoginTime = HttpContext.Session.GetString("_SessionUserTime");
            user.UId = storedUserId;
            user.LoginTime = DateTime.Parse(storedUserLoginTime);

            if (user != null)
            {
                // Check if there is a stored elapsed time for the day
                var storedElapsedTimeKey = $"{user.UId}_ElapsedMilliseconds";
                var storedElapsedTime = HttpContext.Session.GetInt32(storedElapsedTimeKey) ?? 0;

                // If there is stored elapsed time, update the user's elapsed time
                user.ElapsedMilliseconds = storedElapsedTime;

                return View("TrackedPage", user);
            }

            return RedirectToAction("Login", "Login");
        }



        private void setStoredStartTime(DateTime time)
        {
            time = DateTime.Now;

            // Set the value of the "StoredStartTime" cookie
            Response.Cookies.Append("StoredStartTime", time.ToString("yyyy-MM-ddTHH:mm:ss"));

            //  Response.Cookies["StoredStartTime"].Value = time.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public ActionResult UserInformation()
        {
            var userid = HttpContext.Session.GetInt32("_UserID");
            var timesheetdata = DbContext.UserTime
                .Where(u => u.UserId == userid)
                .OrderByDescending(u => u.LoginTime) //Sorted By date
                .ToList();
            return View(timesheetdata);
        }
        public ActionResult Logout(string displayTime)
        {
            var usertracking = new Users();
            int StoredUserID = HttpContext.Session.GetInt32("_UserID") ?? 0;
            usertracking.UId = StoredUserID;
            string role = HttpContext.Session.GetString("Role");
            try
            {
                if (role == "User")
                {
                    if (displayTime == "NaN")
                    {
                        displayTime = "0";
                    }
                    DateTime loginTime = Convert.ToDateTime(HttpContext.Session.GetString("LoginTime"));
                    DateTime logoutTime = DateTime.Now;
                    DateTime dateOnly = logoutTime.Date;
                    var userdata = DbContext.Users.Find(StoredUserID);
                    var timesheetdata = new UserTime
                    {
                        UserId = StoredUserID,
                        LoginTime = loginTime,
                        LogoutTime = logoutTime,
                        Duration = displayTime,
                        Date = dateOnly,
                        AdminId = userdata.AId
                    };
                    DbContext.UserTime.Add(timesheetdata);
                    DbContext.SaveChanges();
                }
                
                //	string StoredUserLoginTime = HttpContext.Session.GetString("_SessionUserTime");
                usertracking = _unitOfWorks.Users.GetById(StoredUserID);
                if (usertracking != null)
                {
                    usertracking.ElapsedMilliseconds = Convert.ToInt64(displayTime);
                    //var storedElapsedTimeKey = $"{usertracking.UId}_ElapsedMilliseconds";
                    //if (long.TryParse(HttpContext.Session.GetString(storedElapsedTimeKey), out long elapsedMilliseconds))
                    //{
                    //    usertracking.ElapsedMilliseconds = elapsedMilliseconds;
                    //}
                    //else
                    //{
                    //    Console.WriteLine($"Could not parse elapsed time: {HttpContext.Session.GetString(storedElapsedTimeKey)}");
                    //}
                    Services.CalculateAndSaveTotalHours(usertracking);


                    var guidString = HttpContext.Session.GetString("TaskToken");
                    Debug.WriteLine($"TaskToken: {guidString}");
                    if (guidString != null)
                    {
                        var guid = Guid.Parse(guidString);
                        if (LoginController.TokenSources.TryRemove(guid, out var cts))
                        {
                            cts.Cancel();
                            if (LoginController.Listener != null)
                            {
                                LoginController.Listener.Stop();
                                LoginController.Listener.Close();
                                LoginController.Listener = null;
                            }
                        }
                    }

                    if (LoginController.CancellationTokenSource != null)
                    {
                        LoginController.CancellationTokenSource.Cancel();
                        LoginController.CancellationTokenSource = null;
                    }
                    HttpContext.Session.Clear();
                    return Json(new { redirectUrl = Url.Action("Index", "Home") });

                    //  return RedirectToAction("UserInformation");
                }
                else
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction("Index", "Home");
                }
            }catch(Exception ex) { }
            return RedirectToAction("Index", "Home");

        }


        public ActionResult StopTracking(int elapsedMilliseconds)
        {
            var user = new Users();
            user.UId = HttpContext.Session.GetInt32("_UserID") ?? 0;
            string userlogintime = HttpContext.Session.GetString("_SessionUserTime");
            user.LoginTime = DateTime.Parse(userlogintime);

            if (user != null)
            {
                user.LogoutTime = DateTime.Now;

                user.ElapsedMilliseconds = elapsedMilliseconds;

                var storedElapsedTimeKey = $"{user.UId}_ElapsedMilliseconds";
                HttpContext.Session.SetString(storedElapsedTimeKey, elapsedMilliseconds.ToString());

                return RedirectToAction("UserDashboard", "Dashboard");
            }

            return RedirectToAction("UserDashboard");
        }



        [HttpPost]
        public ActionResult UpdateElapsedTime(int elapsedMilliseconds)
        {
            try
            {
                var storedUserId = HttpContext.Session.GetInt32(SessionUserId) ?? 0;

                if (storedUserId > 0)
                {
                    var storedElapsedTimeKey = $"{storedUserId}_ElapsedMilliseconds";
                    HttpContext.Session.SetInt32(storedElapsedTimeKey, elapsedMilliseconds);

                    return Json("Elapsed time updated successfully.");
                }

                return Json("Invalid user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateElapsedTime: {ex.Message}");
                return Json("An error occurred");
            }
        }

        [HttpPost]
        public ActionResult SetElapsedTimeToSession(long elapsedMilliseconds)
        {
            var storedElapsedTimeKey = $"{HttpContext.Session.GetInt32("_UserID")}_ElapsedMilliseconds";
            HttpContext.Session.SetString(storedElapsedTimeKey, elapsedMilliseconds.ToString());
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> DownloadData(string email, string dataType)
        {
            // Validate the data type
            if (dataType != "Urls" && dataType != "TotalHours" && dataType != "Screenshots")
            {
                return BadRequest("Invalid data type.");
            }

            // Determine the base path based on the data type
            string basePath;
            if (dataType == "Urls")
            {
                basePath = BaseFolderPath;
            }
            else if (dataType == "TotalHours")
            {
                basePath = totalHoursBasePath;
            }
            else // dataType == "Screenshots"
            {
                basePath = screenshotFolderPath;
            }

            // Retrieve the requested data
            string adminFolderPath = Path.Combine(basePath, email);

            if (!Directory.Exists(adminFolderPath))
            {
                return NotFound("Admin folder not found.");
            }

            // Create a ZIP file of the admin's folder
            var tempZipFilePath = Path.GetTempFileName() + ".zip";
            ZipFile.CreateFromDirectory(adminFolderPath, tempZipFilePath);
            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(tempZipFilePath);
            // Delete the temporary ZIP file
            System.IO.File.Delete(tempZipFilePath);
            string fileName = email + "_" + dataType + ".zip";

            // Return the data as a file download
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadUserData(int id, string userEmail, string dataType)
        {
			// Get the admin's email by id
			int specificAID = HttpContext.Session.GetInt32("_AdminID") ?? 0;
			var admin = DbContext.Admin.Find(specificAID);
            if (admin == null)
            {
                return NotFound();
            }

            var adminEmail = admin.AdminEmail;
            string basePath;
            if (dataType == "Urls")
            {
                basePath = BaseFolderPath;
            }
            else if (dataType == "TotalHours")
            {
                basePath = totalHoursBasePath;
            }
            else // dataType == "Screenshots"
            {
                basePath = screenshotFolderPath;
            }

            // Get the user's folder within the admin's directory
            var userFolder = Path.Combine(basePath, adminEmail, userEmail);

            // Check if the user's folder exists
            if (!Directory.Exists(userFolder))
            {
                return NotFound();
            }

            // Create a zip file of the data folder
            var zipPath = Path.Combine(Path.GetTempPath(), userEmail + "_" + dataType + ".zip");

            // Check if the file already exists, if so, delete it
            if (System.IO.File.Exists(zipPath))
            {
                System.IO.File.Delete(zipPath);
            }

            ZipFile.CreateFromDirectory(userFolder, zipPath);

            // Return the zip file for download
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            return File(bytes, "application/zip", userEmail + "_" + dataType + ".zip");
        }
        //-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------//
        public TimeSpan GetTodaysTotalHours(int userId)
        {
            var users = _unitOfWorks.Users.GetById(userId);
            if (users == null)
            {
                return TimeSpan.Zero;
            }
                string userEmail = users.UserEmail;
                string adminEmail = _unitOfWorks.Admin.GetById(users.AId).AdminEmail;
                string filePath = Path.Combine(totalHoursBasePath, adminEmail, userEmail, "totalHours.txt");
     
            if (!System.IO.File.Exists(filePath))
            {
                return TimeSpan.Zero;
            }

            var lines = System.IO.File.ReadLines(filePath).ToList();
            var todaysEntries = lines;
            TimeSpan todaysTotalHours = TimeSpan.Zero;


            foreach (var entry in todaysEntries)
            {
                var parts = entry.Split(" : ");
                if (parts.Length >= 3)
                {
                    var timePart = parts[2].Replace("Total Hours - ", "");
                    var splitdate = parts[0].Split(' ');
                    
                    DateOnly date = DateOnly.Parse(splitdate[0]);
            
                    if (date == DateOnly.FromDateTime(DateTime.Now))
                    {
                        if (TimeSpan.TryParse(timePart, out TimeSpan time))
                        {
                            todaysTotalHours += time;
                        }
                    }
             
                }
            }

            return todaysTotalHours;
        }

        public IActionResult BrowseScreenshots(string path)
        {
            try
            {
                // Get the user's role and email from the session
                var role = HttpContext.Session.GetString("Role");
                var email = HttpContext.Session.GetString("Email");

                // Start from the base folder if no path is provided
                if (string.IsNullOrEmpty(path))
                {
                    if (role == "SuperAdmin")
                    {
                        path = @"C:\ScreenshortFile\";
                    }
                    else if (role == "Admin" && email != null)
                    {
                        path = Path.Combine(@"C:\ScreenshortFile\", email);
                    }
                }

                // Get all directories in the current path
                var directories = Directory.GetDirectories(path);

                // Get all screenshots in the current path
                var screenshots = Directory.GetFiles(path, "*.png");

                // Create a model to pass to the view
                var model = new BrowseScreenshotsViewModel
                {
                    CurrentPath = role == "SuperAdmin" ? path : null,
                    Directories = directories,
                    Screenshots = screenshots
                };

                return View(model);
            }
            catch(Exception  ex)
            {
                TempData["NoScreenshots"] = "No Data Found";
                return RedirectToAction("AdminDashboard", "Dashboard");
            }
        }
        public IActionResult GetImage(string path)
        {
            var image = System.IO.File.OpenRead(path);
            return File(image, "image/png");
        }
        //---------------------------------------------------------------------------------------------------------------------
        [HttpPost]
        public IActionResult UpdateFlagValue(string start,string stored)
        {
            // Or store the flags in Session
            HttpContext.Session.SetString("startflag", start);
            HttpContext.Session.SetString("StoredFlag", stored);

            return Json(new { success = true });
        }
        //---------------------------------------------------------------------------------------------------------------------
        //EXCEL related method
        //-------------------------------------------------------------------------------------------------
        public  IActionResult ExportToExcelofAdmins() //Called by the superadmin to get admin data
        {
            //Get the Employee data from the database
           
            var admins = _unitOfWorks.Admin.GetAll().ToList();
            //Create an Instance of ExcelFileHandling
            ExcelFileHandling excelFileHandling = new ExcelFileHandling();
            //Call the CreateExcelFile method by passing the list of Employee
            var stream = excelFileHandling.CreateExcelFileOfAdmins(admins);
            //Give a Name to your Excel File
            string excelName = $"Admins-{Guid.NewGuid()}.xlsx";
            // 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' is the MIME type for Excel files
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
        //-----------------------------------------------------------------------------------------------------------------
        public IActionResult ExportToExcelofUsers() //Called by the superadmin to get admin data
        {
            //Get the Employee data from the database
            int adminID = HttpContext.Session.GetInt32("_AdminID") ?? 0;
            var users = _unitOfWorks.Users.GetUsersByAdminId(adminID).ToList();
            //Create an Instance of ExcelFileHandling
            ExcelFileHandling excelFileHandling = new ExcelFileHandling();
            //Call the CreateExcelFile method by passing the list of Employee
            var stream = excelFileHandling.CreateExcelFileOfUsers(users);
            //Give a Name to your Excel File
            string excelName = $"Users-{Guid.NewGuid()}.xlsx";
            // 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' is the MIME type for Excel files
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
        //--------------------------------------------------------------------------------------------------------------------
        public IActionResult ViewUrls() {
            var role = HttpContext.Session.GetString("Role");
            if (role == "SuperAdmin")
            {
                var users = DbContext.Users.ToList();
                foreach (var user in users)
                {
                    if (user.UrlsJson == null) { continue; }
                    var deserializedJson = JsonConvert.DeserializeObject(user.UrlsJson);
                    //    Console.WriteLine(deserializedJson);
                    user.UrlsJson = Convert.ToString(deserializedJson);
                }
                return View(users);
            }
            else if (role == "Admin") {
                int adminID = HttpContext.Session.GetInt32("_AdminID") ?? 0;

                var users = _unitOfWorks.Users.GetUsersByAdminId(adminID).ToList();
                foreach (var user in users)
                {
                    if (user.UrlsJson == null) { continue; }
                    var deserializedJson = JsonConvert.DeserializeObject(user.UrlsJson);
                    //    Console.WriteLine(deserializedJson);
                    user.UrlsJson = Convert.ToString(deserializedJson);
                }
                return View(users);

            }
            return View();
        }
        //----------------------------------------------------------------------------------------------------------------------
        public async Task< IActionResult> statisticsSuperAdmin() {
           var organizations=  DbContext.Admin.Select(o => o.OrganizationName).Distinct().ToList();
			var userCountsByOrganization = await DbContext.Users
		  .Join(DbContext.Admin, // The table to join with
			  u => u.AId, // The foreign key property on the Users table
			  a => a.AId, // The primary key property on the Admins table
			  (u, a) => new { User = u, Admin = a }) // The result selector
		  .GroupBy(ua => ua.Admin.OrganizationName) // Group by the organization name of the admin
		  .Select(g => new { OrganizationName = g.Key, UserCount = g.Count() }) // Select the organization name and the count of users
		  .ToListAsync();

            var statdata = new StatDataSuperAdmin { 
                OrganizationNames = organizations,
				OrganizationAndNumOfUsers = userCountsByOrganization.Select(x => (x.OrganizationName, x.UserCount)).ToList()
			};
			return View(statdata);
        }
        //--------------------------------------------------------------------------------------------
        public async Task<IActionResult> statisticsAdmin()
        {
          //  int specificAID = HttpContext.Session.GetInt32("_AdminID");
            int specificAID = HttpContext.Session.GetInt32("_AdminID") ?? 0;

            // Filter the users by the specific AID before selecting their designations
            var roles = await DbContext.Users
                .Where(u => u.AId == specificAID)
                .Select(u => u.UserDesignation)
                .Distinct()
                .ToListAsync(); 

            var roleAndCount = new List<(string, int)>();

            foreach (var role in roles)
            {
                // Count the users with the specific AID and role
                var count = await DbContext.Users
                    .Where(u => u.AId == specificAID && u.UserDesignation == role)
                    .CountAsync();

                roleAndCount.Add((role, count));
            }

            var model = new StatDataAdmin
            {
                Roles = roles,
                RoleAndCount = roleAndCount
            };

            return View(model);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------
        public IActionResult broadcastMessage() {
            var aid = HttpContext.Session.GetInt32("_AdminID") ?? 0;

            var broadcast_data = DbContext.Broadcastmessage.Where(Aid => Aid.AId == aid).ToList();
            var viewmodel = new BroadcastViewModel { 
            broadcastmessages = broadcast_data};
            return View(viewmodel);
        }
        [HttpPost]
        public IActionResult broadcastMessage(Broadcastmessage broadcastmessage) {
            var aid = HttpContext.Session.GetInt32("_AdminID") ?? 0;
            var data = new Broadcastmessage { 
                AId = aid,
                Message = broadcastmessage.Message,
                broadcastDate = DateTime.Now,
                messageheading = broadcastmessage.messageheading,
            };
            if(broadcastmessage.Message != null)
            {
                DbContext.Broadcastmessage.Add(data);
                DbContext.SaveChanges();
            }

            return RedirectToAction("broadcastMessage", "Dashboard");
        }
        //------------------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult deleteBroadmessage(int id) {
            var message = DbContext.Broadcastmessage.Find(id);
            DbContext.Broadcastmessage.Remove(message);
            DbContext.SaveChanges();
            return RedirectToAction("broadcastMessage","Dashboard");        
        }
		//-----------------------------------------------------------------------------------------------
		public IActionResult DisplaybroadcastMessage() {
			var userid = HttpContext.Session.GetInt32("_UserID");
            var userdetail = DbContext.Users.FirstOrDefault(u => u.UId == userid);
            var broadcast_data = DbContext.Broadcastmessage.Where(aid => aid.AId == userdetail.AId).ToList();
			return View(broadcast_data);

        }
        //---------------------------------------------------------------------------------------------------
        public IActionResult UserLeaveRequest()
        {
            var userid = HttpContext.Session.GetInt32("_UserID");
            var leave_data = DbContext.Leaves.Where(uid => uid.UserId == userid).OrderByDescending(l => l.DateTime).ToList();
            var leaveViewModel = new LeaveViewModel {
                LeavesList = leave_data,
            };
            return View(leaveViewModel);
        }
        [HttpPost]
        public IActionResult UserLeaveRequest(Leaves leaves) {
            if (leaves != null) {
                var userid = HttpContext.Session.GetInt32("_UserID")??0;
                var userdetais = DbContext.Users.Where(u => u.UId == userid).FirstOrDefault();
                leaves.UserId = userid;
                leaves.AdminId = userdetais.AId;
                leaves.leaveStatus = "0";
                leaves.DateTime = DateTime.Now;
                DbContext.Leaves.Add(leaves);
                DbContext.SaveChanges();
            }
            return RedirectToAction("UserLeaveRequest","Dashboard");
        }
		[HttpGet]
		public IActionResult Userdeleteleave(int id)
		{
			var leave = DbContext.Leaves.Find(id);
			DbContext.Leaves.Remove(leave);
			DbContext.SaveChanges();
			return RedirectToAction("UserLeaveRequest", "Dashboard");
		}

		//--------------------------------------------------------------------------------------------------------------------
		public IActionResult AdminViewLeaveRequest() {
            var Aid = HttpContext.Session.GetInt32("_AdminID") ?? 0;
            List<Leaves> leave_data_with_UserDetails = new List<Leaves>();
            var leaves = DbContext.Leaves.Where(aid => aid.AdminId == Aid).ToList();
            var users = DbContext.Users.Where(aid => aid.AId == Aid).ToList();
            for (int i = 0; i < leaves.Count; i++) {
                for (int j = 0; j < users.Count; j++) {
                    if (leaves[i].UserId == users[j].UId) {
                        leaves[i].username = users[j].UserName;
                        leaves[i].useremail = users[j].UserEmail;
                        leaves[i].userDesignation = users[j].UserDesignation;
                        leave_data_with_UserDetails.Add(leaves[i]);
                    }
                }
            }
            var adminviewmodel = new LeaveViewModel { 
                LeavesList = leave_data_with_UserDetails
            };
            return View(adminviewmodel);
        }
        [HttpPost]
        public IActionResult AdminViewLeaveRequest(int id, string status,string message)
        {
            var leavedata = DbContext.Leaves.Find(id);
            if (leavedata == null) { } else {
                leavedata.leaveStatusMessage = message;
                if(status is null) { status = "0"; }
                leavedata.leaveStatus = status;
              //  DbContext.Leaves.Add(leavedata);
                DbContext.SaveChanges();
            }
            return RedirectToAction("AdminViewLeaveRequest","Dashboard");
        }
        [HttpGet]
        public IActionResult Admindeleteleave(int id)
        {
            var leave = DbContext.Leaves.Find(id);
            DbContext.Leaves.Remove(leave);
            DbContext.SaveChanges();
            return RedirectToAction("AdminViewLeaveRequest", "Dashboard");
        }
        //------------------------------------------------------------------------------------------------------------------
        public IActionResult AdminTimeSheet()
        {
			var Aid = HttpContext.Session.GetInt32("_AdminID") ?? 0;
            var timedata = DbContext.UserTime.Where(u => u.AdminId == Aid).ToList();
            var userDetails = DbContext.Users.Where(u => u.AId == Aid).ToList();
            List<UserTimeAdminViewModel> data = new List<UserTimeAdminViewModel>();
            foreach (var user in userDetails)
            {
                for(int i=0;i<timedata.Count;i++)
                {
                    if (user.UId == timedata[i].UserId) { 
                        var temp = new UserTimeAdminViewModel();
                        temp.UserEmail = user.UserEmail;
                        temp.UserDesgination = user.UserDesignation;
                        temp.UserName = user.UserName;
                        temp.elapsedTime = Convert.ToInt64(timedata[i].Duration);
                        if (temp != null) {
                            data.Add(temp);
                        }
                    }
                }
            }
            Console.WriteLine(data);
			return View(data);
        }

	}
}
