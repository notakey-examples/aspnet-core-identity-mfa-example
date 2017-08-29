using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IdentitySample.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

		public async void EnsureSeedData(UserManager<ApplicationUser> userMgr)
		{
			if (!this.Users.Any(u => u.UserName == "admin@mydomain.com"))
			{
				

				// create admin user
				var adminUser = new ApplicationUser();
				adminUser.UserName = "admin@mydomain.com";
				adminUser.Email = "admin@mydomain.com";

				await userMgr.CreateAsync(adminUser, "MYP@55word");

				await userMgr.SetLockoutEnabledAsync(adminUser, false);
			}
		}
    }
}
