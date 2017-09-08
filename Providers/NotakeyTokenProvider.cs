using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Notakey.SDK;
using System.Reactive;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using System.Reactive.Threading.Tasks;
using IdentitySample.Models;

namespace IdentitySample.Providers
{


	/// <summary>
	/// Used for Notakey authenticator request verification.
	/// </summary>
	public class NotakeyTokenProvider<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : class
	{

		private ILogger<SignInManager<TUser>> _logger;
		private readonly NotakeyOptions _ntkOpts;

		public NotakeyTokenProvider(ILogger<SignInManager<TUser>> logger,
									IOptions<NotakeyOptions> ntkOpts)
		{

			if (ntkOpts == null)
				throw new ArgumentNullException(nameof(ntkOpts));


			_ntkOpts = ntkOpts.Value;
			_logger = logger;
			
		}

		/// <summary>
		/// Checks if a two factor authentication token can be generated for the specified <paramref name="user"/>.
		/// </summary>
		/// <param name="manager">The <see cref="UserManager{TUser}"/> to retrieve the <paramref name="user"/> from.</param>
		/// <param name="user">The <typeparamref name="TUser"/> to check for the possibility of generating a two factor authentication token.</param>
		/// <returns>True if the user has an authenticator key set, otherwise false.</returns>
		public async virtual Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
		{
            // TODO: verify via API that user actually exists
            // On the other hand, this causes 2FA to be skipped altogather if we return false

            return await Task.FromResult(true) ;
		}

		/// <summary>
		/// Returns token that identifies autentication request sent to mobile device user
		/// </summary>
		/// <param name="purpose">Ignored.</param>
		/// <param name="manager">The <see cref="UserManager{TUser}"/> to retrieve the <paramref name="user"/> from.</param>
		/// <param name="user">The <typeparamref name="TUser"/>.</param>
		/// <returns>string.Empty.</returns>
		public virtual Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
		{
			/**
             * 1. The first task is to bind a SimpleApi instance to an API (endpoint + access_id combination)
             * After the api instance is bound to the remote API, it can be used to perform other operations.
             * 
             * NOTE: before you are bound to an application, do not use any other functions!
             */

			var api = new SimpleApi();

            var uuidSequence = Observable.Defer<string>(() => api.CreateAuthRequest(user.ToString(), _ntkOpts.ActionTitle, "Example authentication request from .NET CORE 2.0", (int)_ntkOpts.MessageTtlSeconds));

            var full = api
                .Bind(_ntkOpts.ServiceURL, _ntkOpts.ServiceID)
                .SingleAsync()
                .Select(_ => "")
                .Concat(uuidSequence);

            return full.ToTask();
		}

		/// <summary>
		/// Native method for 2FA token validation
		/// </summary>
		/// <param name="purpose"></param>
		/// <param name="token"></param>
		/// <param name="manager"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public virtual async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
		{
            var authState = await TwoFactorNotakeyAuthState(token);

            if(!authState.Expired && authState.ApprovalGranted){
                return true;
            }

			return false;
		}


		/// <summary>
		/// Checks if authentication request is valid and returns request state - Approved, Rejected, Pending
		/// When Pending == false, authentication flow can proceed. 
		/// ResponsePayloadBase64 field in response contains payload with signature from mobile device
        /// You can re-validate signature in XML payload with standard PKI toolset (validation is already done in NAS)
        /// Fingerprint element in XML can be used to track actual device, if user has multiple
		/// Model ApprovalRequestResponse comes from Notakey.SDK API 
		/// </summary>
		/// <param name="Uuid"></param>
		/// <returns></returns>
		public virtual Task<ApprovalRequestResponse> TwoFactorNotakeyAuthState(string Uuid)
        {
            if (string.IsNullOrWhiteSpace(Uuid)){
                throw new Exception("Invalid auth request UUID");
            }


			var api = new SimpleApi();

            var authSequence = Observable.Defer<ApprovalRequestResponse>(() => api.CheckResponse(Uuid));

            var full = api
                .Bind(_ntkOpts.ServiceURL, _ntkOpts.ServiceID)
                .SingleAsync()
                .Select(_ => new ApprovalRequestResponse())
                .Concat(authSequence)
                .LastAsync();


            return full.ToTask();
          
        }
		
  
	}
}