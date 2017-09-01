using Microsoft.AspNetCore.Identity;
using IdentitySample.Models;

using System.Threading.Tasks;

namespace IdentitySample.Mvc.Models.SeedDataModels
{

    public class SampleUserDataModel
    {
       
        public static void Initialize(UserManager<ApplicationUser> userManager)
        {
            DoAddDemoUser(userManager);
        }

		public static async void DoAddDemoUser(UserManager<ApplicationUser> _userManager)
		{
			var user = new ApplicationUser { UserName = "demo@demo.lv", Email = "demo@demo.lv" };
			var result = await _userManager.CreateAsync(user, "asdlkj");
            if (result.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
            }
		}
    }
}