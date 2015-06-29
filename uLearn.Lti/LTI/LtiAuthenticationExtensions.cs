﻿using System;
using System.Data.Entity;
using LtiLibrary.Core.Common;
using LtiLibrary.Core.OAuth;
using LtiLibrary.Owin.Security.Lti;
using LtiLibrary.Owin.Security.Lti.Provider;
using Microsoft.AspNet.Identity;
using Owin;
using uLearn.Lti.DataContext;

namespace uLearn.Lti.LTI
{
	public static class LtiAuthenticationExtensions
	{
		public static IAppBuilder UseLtiAuthentication(this IAppBuilder app)
		{
			app.UseLtiAuthentication(new LtiAuthenticationOptions
			{
				Provider = new LtiAuthenticationProvider
				{
					// Look up the secret for the consumer
					OnAuthenticate = async context =>
					{
						// Make sure the request is not being replayed
						var timeout = TimeSpan.FromMinutes(5);
						var oauthTimestampAbsolute = OAuthConstants.Epoch.AddSeconds(context.LtiRequest.Timestamp);
						if (DateTime.UtcNow - oauthTimestampAbsolute > timeout)
						{
							throw new LtiException("Expired " + OAuthConstants.TimestampParameter);
						}

						var db = new LtiDb();
						var consumer = await db.Consumers.SingleOrDefaultAsync(c => c.Key == context.LtiRequest.ConsumerKey);
						if (consumer == null)
						{
							throw new LtiException("Invalid " + OAuthConstants.ConsumerKeyParameter);
						}

						var signature = context.LtiRequest.GenerateSignature(consumer.Secret);
						if (!signature.Equals(context.LtiRequest.Signature))
						{
							throw new LtiException("Invalid " + OAuthConstants.SignatureParameter);
						}

						// If we made it this far the request is valid
					},

					// Sign in using application authentication. This handler will create a new application
					// user if no matching application user is found.
					OnAuthenticated = context => SecurityHandler.OnAuthenticated(context),

					// Generate a username using the LisPersonEmailPrimary from the LTI request
					OnGenerateUserName = context => SecurityHandler.OnGenerateUserName(context)
				},
				SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
			});

			return app;
		}
	}
}
