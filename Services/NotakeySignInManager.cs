using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using IdentitySample.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;

namespace IdentitySample.Services
{


    public class NotakeySignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly NotakeyOptions _ntkOpts;
        private ILogger<SignInManager<TUser>>  _logger; 
        private TUser loginUser; 

        public NotakeySignInManager(UserManager<TUser> userManager, 
                                    IHttpContextAccessor contextAccessor, 
                                    IUserClaimsPrincipalFactory<TUser> claimsFactory, 
                                    IOptions<IdentityOptions> optionsAccessor,
                                    ILogger<SignInManager<TUser>> logger, 
                                    ApplicationDbContext dbContext, 
                                    IAuthenticationSchemeProvider schemes,
                                    IOptions<NotakeyOptions> ntkOpts)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
        {
            if (userManager == null)
                throw new ArgumentNullException(nameof(userManager));

            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));

            if (contextAccessor == null)
                throw new ArgumentNullException(nameof(contextAccessor));

            if (ntkOpts == null)
                throw new ArgumentNullException(nameof(ntkOpts));

            _ntkOpts = ntkOpts.Value;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
            _db = dbContext;
            _logger = logger;
           
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
			var user = await UserManager.FindByNameAsync(userName);
			if (user == null)
			{
				return SignInResult.Failed;
			}

			return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

		public override async Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            _logger.LogInformation(3, String.Format("NotakeySignInManager: PasswordSignInAsync for user: {0}", user));


            var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);



            if (result.Succeeded) {
                //
            }

            return result;
        }

		public override async Task SignOutAsync()
		{
			await base.SignOutAsync();

		}
    }
}