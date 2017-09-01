using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IdentitySample.Models;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Notakey.SDK;
using System.Reactive;
using System.Reactive.Linq;


namespace IdentitySample.Services
{


    public class NotakeySignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly UserManager<TUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _contextAccessor;
        private ManualResetEvent waitEvent;
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
            waitEvent = new ManualResetEvent(false);
        }

        public async Task<SignInResult> NotakeyPasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
			var user = await UserManager.FindByNameAsync(userName);
			if (user == null)
			{
				return SignInResult.Failed;
			}

			return await NotakeyPasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }

		public async Task<SignInResult> NotakeyPasswordSignInAsync(TUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            _logger.LogInformation(3, String.Format("NotakeySignInManager: PasswordSignInAsync for user: {0}", user));


            var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);

            if (result.Succeeded) { 
                loginUser = user;

                /**
                 * 1. The first task is to bind a SimpleApi instance to an API (endpoint + access_id combination)
                 * After the api instance is bound to the remote API, it can be used to perform other operations.
                 * 
                 * NOTE: before you are bound to an application, do not use any other functions!
                 */

                var api = new SimpleApi();

                api
                    .Bind(_ntkOpts.ServiceURL, _ntkOpts.ServiceID)
                    .SingleAsync()
                    .Subscribe(uri => OnBound(uri, api), OnBindError);

                waitEvent.WaitOne();
             }

            // result.RequiresTwoFactor = true;

            return result;
        }


        private  void OnBound(Uri address, SimpleApi boundApi)
        {
            _logger.LogInformation(3, String.Format("NotakeySignInManager: bound to {0}", address));
            _logger.LogInformation(3, String.Format("NotakeySignInManager: Requesting verification for user '{0}' ...", loginUser));
            /* 2. Now that we are bound to an application, 
             * we can invoke PerformFullVerification and other
             * methods.
             * 
             * PerformFullVerification will return once the result is known
             * (approved/denied) or the request expires (by default 5 minutes).
             */
            boundApi
                .PerformFullVerification(loginUser.ToString(), _ntkOpts.ActionTitle, "Example authentication request from .NET CORE 2.0", null, (int)_ntkOpts.MessageTtlSeconds)
                .SingleAsync()
                .Finally(() => waitEvent.Set())
                .Subscribe(OnVerificationResponse, OnVerificationError);
        }

        private  void OnVerificationError(Exception e)
        {

            Console.Error.WriteLine("NotakeySignInManager: ERROR: failed to perform verification: {0}", e.ToString());
        }

        private void OnVerificationResponse(ApprovalRequestResponse request)
        {
            /**
             * 3. ApprovalGranted is a convenience property, but it does not perform
             * signature validation.
             * 
             * In a real-world scenario, you could now verify the response payload
             * to be sure of the received data.
             */
            _logger.LogInformation(3, String.Format("SUCCESS: verification response: {0}", request.ApprovalGranted));
        }

        private  void OnBindError(Exception e)
        {
            waitEvent.Set();
            Console.Error.WriteLine("ERROR: failed to bind to API: {0}", e.ToString());
        }

        public override async Task SignOutAsync()
        {
            await base.SignOutAsync();

        }
    }
}