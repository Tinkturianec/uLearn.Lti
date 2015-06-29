﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LtiLibrary.Owin.Security.Lti.Provider;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using uLearn.Lti.DataContext;
using uLearn.Lti.Models;

namespace uLearn.Lti.LTI
{
	public static class SecurityHandler
	{
		/// <summary>
		/// Invoked after the LTI request has been authenticated so the application can sign in the application user.
		/// </summary>
		/// <param name="context">Contains information about the login session as well as the LTI request.</param>
		/// <param name="claims">Optional set of claims to add to the identity.</param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		public static async Task OnAuthenticated(LtiAuthenticatedContext context, IEnumerable<Claim> claims = null)
		{
			// Find existing pairing between LTI user and application user
			var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new LtiDb()));
			var loginProvider = string.Join(":", new[] { context.Options.AuthenticationType, context.LtiRequest.ConsumerKey });
			var providerKey = context.LtiRequest.UserId;
			var login = new UserLoginInfo(loginProvider, providerKey);
			var user = await userManager.FindAsync(login);
			if (user == null)
			{
				var usernameContext = new LtiGenerateUserNameContext(context.OwinContext, context.LtiRequest);
				await context.Options.Provider.GenerateUserName(usernameContext);
				if (string.IsNullOrEmpty(usernameContext.UserName))
				{
					return;
				}
				user = await userManager.FindByNameAsync(usernameContext.UserName);
				if (user == null)
				{
					user = new ApplicationUser { UserName = usernameContext.UserName };
					var result = await userManager.CreateAsync(user);
					if (!result.Succeeded)
					{
						return;
					}
				}
				// Save the pairing between LTI user and application user
				await userManager.AddLoginAsync(user.Id, login);
			}

			// Create the application identity, add the LTI request as a claim, and sign in
			var identity = await userManager.CreateIdentityAsync(user, context.Options.SignInAsAuthenticationType);
			identity.AddClaim(new Claim(context.Options.ClaimType, JsonConvert.SerializeObject(context.LtiRequest, Formatting.None,
					new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), ClaimValueTypes.String, context.Options.AuthenticationType));
			if (claims != null)
			{
				foreach (var claim in claims)
				{
					identity.AddClaim(claim);
				}
			}
			context.OwinContext.Authentication.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);

			// Redirect to original URL so the new identity takes affect
			context.RedirectUrl = context.LtiRequest.Url.ToString();
		}

		/// <summary>
		/// Generate a valid application username using information from an LTI request. The default
		/// ASP.NET application using Microsoft Identity uses an email address as the username. This
		/// code will generate an "anonymous" email address if one is not supplied in the LTI request.
		/// </summary>
		/// <param name="context">Contains information about the login session as well as the LTI request.</param>
		/// <returns>A <see cref="Task"/> representing the completed operation.</returns>
		public static Task OnGenerateUserName(LtiGenerateUserNameContext context)
		{
			if (string.IsNullOrEmpty(context.LtiRequest.LisPersonEmailPrimary))
			{
				var username = string.Concat("anon_", context.LtiRequest.UserId);
				Uri url;
				if (string.IsNullOrEmpty(context.LtiRequest.ToolConsumerInstanceUrl)
					|| !Uri.TryCreate(context.LtiRequest.ToolConsumerInstanceUrl, UriKind.Absolute, out url))
				{
					context.UserName = string.Concat(username, "@anon_", context.LtiRequest.ConsumerKey, ".lti");
				}
				else
				{
					context.UserName = string.Concat(username, "@", url.Host);
				}
			}
			else
			{
				context.UserName = context.LtiRequest.LisPersonEmailPrimary;
			}

			return Task.FromResult<object>(null);
		}
	}
}