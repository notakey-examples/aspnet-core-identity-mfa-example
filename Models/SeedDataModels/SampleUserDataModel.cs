using Microsoft.AspNetCore.Identity;
using IdentitySample.Models;

using System.Threading.Tasks;

namespace IdentitySample.Mvc.Models.SeedDataModels
{

    public class SampleUserDataModel
    {
       
        public static void Initialize(UserManager<ApplicationUser> userManager)
        {
            DoAddDemoUser(userManager, "demo@example.com", "asdlkj");
            DoAddDemoUser(userManager, "mode@example.com", "asdlkj");
        }

		public static async void DoAddDemoUser(UserManager<ApplicationUser> _userManager, string username, string pass)
		{
			var user = new ApplicationUser { UserName = username, Email = username };
			var result = await _userManager.CreateAsync(user, pass);
            if (result.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }

		}
    }
}