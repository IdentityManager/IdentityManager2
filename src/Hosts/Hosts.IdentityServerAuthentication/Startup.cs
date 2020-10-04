using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Hosts.Shared.InMemory;
using IdentityManager2.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
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
                            HostChallengeType = "oidc",
                            AdditionalSignOutType = "oidc"
                        })
                .AddIdentityMangerService<InMemoryIdentityManagerService>();

            var rand = new Random();
            var identityManagerUsers = Users.Get(rand.Next(5000, 20000));
            services.AddSingleton(x => identityManagerUsers);
            services.AddSingleton(x => Roles.Get(rand.Next(15)));
            
            var client = new Client
            {
                ClientId = "identitymanager2",
                ClientName = "IdentityManager2",
                AllowedGrantTypes = GrantTypes.Implicit,
                RedirectUris = {"https://localhost:5000/idm/signin-oidc"},
                AllowedScopes = {"openid", "profile", "roles"},
                RequireConsent = false
            };

            var roles = new IdentityResource("roles", new List<string> {"role"});

            var identityServerUsers = identityManagerUsers.Select(x => new TestUser
            {
                SubjectId = x.Subject,
                Username = x.Username,
                Password = x.Password,
                Claims = x.Claims
            }).ToList();

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/login";
                    options.UserInteraction.LogoutUrl = "/logout";
                })
                .AddTestUsers(identityServerUsers)
                .AddInMemoryIdentityResources(new List<IdentityResource> {new IdentityResources.OpenId(), new IdentityResources.Profile(), roles})
                .AddInMemoryApiResources(new List<ApiResource>())
                .AddInMemoryClients(new List<Client> {client})
                .AddDeveloperSigningCredential(false);

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication()
                .AddCookie("cookie")
                .AddOpenIdConnect("oidc", opt =>
                {
                    opt.Authority = "https://localhost:5000/auth";
                    opt.ClientId = "identitymanager2";

                    // default: openid & profile
                    opt.Scope.Add("roles");

                    opt.RequireHttpsMetadata = false; // dev only
                    opt.SignInScheme = "cookie";
                    opt.CallbackPath = "/signin-oidc";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.Map("/auth", auth =>
            {
                auth.UseRouting();
                
                auth.UseIdentityServer();
                
                auth.UseEndpoints(x => x.MapDefaultControllerRoute());
            });
            
            app.Map("/idm", idm =>
            {
                idm.UseRouting();

                idm.UseAuthentication();
                idm.UseAuthorization();

                idm.UseIdentityManager();

                idm.UseEndpoints(x => x.MapDefaultControllerRoute());
            });

        }
    }
}