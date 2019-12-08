using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Hosts.Shared.InMemory;
using IdentityManager2.Configuration;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Hosts.IdentityServerAuthentication
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory IdentityManagerService (demo only)
            services.AddIdentityManager(opt =>
                    opt.SecurityConfiguration =
                        new SecurityConfiguration
                        {
                            HostAuthenticationType = "cookie",
                            HostChallengeType = "oidc"
                        })
                .AddIdentityMangerService<InMemoryIdentityManagerService>();

            var admin = new TestUser
            {
                SubjectId = "123",
                Username = "scott",
                Password = "scott",
                Claims = {new Claim("role", "IdentityManagerAdministrator")}
            };

            var client = new Client
            {
                ClientId = "identitymanager2",
                ClientName = "IdentityManager2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RedirectUris = {"http://localhost:5000/idm/signin-oidc"},
                AllowedScopes = {"openid", "profile", "roles"},
                RequireConsent = false
            };

            var roles = new IdentityResource("roles", new List<string> {"role"});

            services.AddIdentityServer()
                .AddTestUsers(new List<TestUser> {admin})
                .AddInMemoryIdentityResources(new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile(), roles})
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryClients(new List<Client> {client})
                .AddDeveloperSigningCredential(false);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication()
                .AddCookie("cookie")
                .AddOpenIdConnect("oidc", opt =>
                {
                    opt.Authority = "http://localhost:5000/auth";
                    opt.ClientId = "identitymanager2";

                    // default: openid & profile
                    opt.Scope.Add("roles");

                    opt.RequireHttpsMetadata = false; // dev only
                    opt.SignInScheme = "cookie";
                    opt.CallbackPath = "/signin-oidc";

                    opt.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = context => Task.CompletedTask
                    };
                });

            var rand = new Random();
            services.AddSingleton(x => Users.Get(rand.Next(5000, 20000)));
            services.AddSingleton(x => Roles.Get(rand.Next(15)));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.Map("/auth", auth =>
            {
                auth.UseIdentityServer();

                // Force authentication
                auth.Map("/account/login",
                    login => login.Use(async (context, func) =>
                    {
                        await context.SignInAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme,
                            new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> {new Claim("sub", "123")}, IdentityServerConstants.DefaultCookieAuthenticationScheme)));
                        context.Response.Redirect(context.Request.Query["returnUrl"]);
                    }));
            });
            
            app.Map("/idm", idm =>
            {
                idm.UseRouting();

                idm.UseAuthentication();
                idm.UseAuthorization();

                idm.UseIdentityManager();

                idm.UseEndpoints(x =>
                {
                    x.MapDefaultControllerRoute();
                });
            });

        }
    }
}