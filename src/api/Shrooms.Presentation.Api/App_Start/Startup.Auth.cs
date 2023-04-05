﻿using System;
using System.Configuration;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Presentation.Api.Providers;

namespace Shrooms.Presentation.Api
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthServerOptions { get; set; }

        public static string JsAppClientId { get; } = ConfigurationManager.AppSettings["AngularClientId"];

        public static string MobileAppClientId { get; } = ConfigurationManager.AppSettings["MobileAppClientId"];

        public void ConfigureAuthMiddleware(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new QueryStringBearerAuthProvider()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ExternalBearer,
                Provider = new QueryStringBearerAuthProvider()
            });
        }

        public void ConfigureAuthServer(IAppBuilder app, IContainer container)
        {
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            var tokenTimeSpanInHours = ConfigurationManager.AppSettings["AccessTokenLifeTimeInHours"];

            var appSettings = container.Resolve(typeof(IApplicationSettings)) as IApplicationSettings;

            OAuthServerOptions = new OAuthAuthorizationServerOptions
            {
                Provider = new ApplicationOAuthProvider(container, appSettings, JsAppClientId, MobileAppClientId),
                TokenEndpointPath = new PathString("/token"),
                AuthorizeEndpointPath = new PathString("/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(Convert.ToInt16(tokenTimeSpanInHours)),
                AllowInsecureHttp = true,
                RefreshTokenProvider = new RefreshTokenProvider(container)
            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);

            // Configure the application for OAuth based flow
            if (HasProviderSettings("GoogleAccountClientId", "GoogleAccountClientSecret"))
            {
                var googleOAuthOptions = new GoogleOAuth2AuthenticationOptions
                {
                    Provider = new CustomGoogleAuthProvider(container),
                    ClientId = ConfigurationManager.AppSettings["GoogleAccountClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["GoogleAccountClientSecret"]
                };
                app.UseGoogleAuthentication(googleOAuthOptions);
            }

            if (HasProviderSettings("FacebookAccountAppId", "FacebookAccountAppSecret"))
            {
                var facebookOAuthOptions = new FacebookAuthenticationOptions
                {
                    Provider = new CustomFacebookAuthProvider(container),
                    AppId = ConfigurationManager.AppSettings["FacebookAccountAppId"],
                    AppSecret = ConfigurationManager.AppSettings["FacebookAccountAppSecret"],
                    Scope = { "public_profile", "email" },
                    Fields = { "email", "name", "first_name", "last_name", "picture.width(800).height(800)" }
                };

                app.UseFacebookAuthentication(facebookOAuthOptions);
            }

            if (HasProviderSettings("MicrosoftAccountClientId", "MicrosoftAccountClientSecret"))
            {
                var microsoftOAuthOptions = new MicrosoftAccountAuthenticationOptions
                {
                    AuthorizationEndpoint = "https://login.microsoftonline.com/856ea157-bf1d-43df-af37-bbfbc0072578/oauth2/authorize",
                    TokenEndpoint = "https://login.microsoftonline.com/856ea157-bf1d-43df-af37-bbfbc0072578/oauth2/v2.0/token",
                    Provider = new CustomMicrosoftAccountAuthProvider(container),
                    ClientId = ConfigurationManager.AppSettings["MicrosoftAccountClientId"],
                    ClientSecret = ConfigurationManager.AppSettings["MicrosoftAccountClientSecret"]
                };

                app.UseMicrosoftAccountAuthentication(microsoftOAuthOptions);
            }
        }

        private static bool HasProviderSettings(string idKey, string secretKey)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[idKey]) &&
                !string.IsNullOrEmpty(ConfigurationManager.AppSettings[secretKey]);
        }

        public class QueryStringBearerAuthProvider : OAuthBearerAuthenticationProvider
        {
            public override Task RequestToken(OAuthRequestTokenContext context)
            {
                if (!context.Request.Path.Value.StartsWith("/signalr"))
                {
                    return Task.FromResult(context);
                }

                var token = context.Request.Query.Get("token");
                if (token != null)
                {
                    context.Token = token;
                }

                return Task.FromResult(context);
            }
        }
    }
}