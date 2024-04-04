using Microsoft.EntityFrameworkCore;
using MultiLogin.Models;
using System.Data;

namespace MultiUserLoginTrial.DataAccess.Data
{
    public class ApplicationDBContext : DbContext
	{
		

		public ApplicationDBContext(DbContextOptions<ApplicationDBContext>options) : base(options) { }
		public DbSet<SuperAdmin> SuperAdmin { get; set; }
		public DbSet<Admin> Admin { get; set; }
		public DbSet<Users> Users { get; set; }
		public DbSet<Roles> Roles { get; set; }
        public DbSet<Interests> Interest { get; set; }
        public DbSet<UserTime> UserTime { get; set; }
        public DbSet<Broadcastmessage> Broadcastmessage { get; set; }
        public DbSet<Leaves> Leaves { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SuperAdmin>().HasData(
                new SuperAdmin { SId = 1, SuperAdminEmail = "Sumago@gmail.com", SuperAdminPassword = "Sumago@123" }
            );

            modelBuilder.Entity<Roles>().HasData(
                new Roles { Emails = "Sumago@gmail.com", Role = "SuperAdmin" }
            );


        }

    }
}
