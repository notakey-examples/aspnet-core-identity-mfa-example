using IdentitySample.Models;
using IdentitySample.Mvc.Models.SeedDataModels;
using IdentitySample.Services;
using IdentitySample.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentitySample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("default"));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                // We are not using standard providers 
                //.AddDefaultTokenProviders()
                .AddTokenProvider<NotakeyTokenProvider<ApplicationUser>>("Notakey");

            // Before custom sign in manager 
            services.Configure<NotakeyOptions>(options => Configuration.GetSection("Notakey").Bind(options));

            services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequireDigit = false;
				options.Password.RequiredLength = 4;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

				// User settings
				options.User.RequireUniqueEmail = false;
			});

            services.AddMvc();

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
			
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

			
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Demo users are created here
            SampleUserDataModel.Initialize(app.ApplicationServices.GetService<UserManager<ApplicationUser>>());
        }
    }

	
}