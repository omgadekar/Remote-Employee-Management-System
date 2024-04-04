using Microsoft.EntityFrameworkCore;
using MultiUserLoginTrial.Repository.IRepository;
using MultiUserLoginTrial.Repository;
using System.Net;
using MultiUserLoginTrial.Service;
using MultiUserLoginTrial.DataAccess.Data;
using Serilog;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

//

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<IUnitOfWorks, UnitOfWorks>();
//builder.Services.AddDbContext<ApplicationDBContext>(options =>  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),b => b.MigrationsAssembly("MultiUserLoginTrial")));

builder.Host.UseSerilog((context, services, configuration) => configuration
	.ReadFrom.Configuration(context.Configuration)
	.Enrich.FromLogContext()
	.WriteTo.Console()
	.WriteTo.File("Logs/myapp.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddScoped<IServices, Service>();





builder.Services.AddSession(options => {  options.Cookie.HttpOnly = true; options.IdleTimeout = TimeSpan.FromHours(24); options.Cookie.IsEssential = true; });
var app = builder.Build();
// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
