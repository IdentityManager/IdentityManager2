using System;
using System.Collections.Generic;
using System.Security.Claims;
using Host.InMemory;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Host
{
    public class StartupWithCustomAuth
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory IdentityManagerService (demo only)
            services.AddIdentityManager(opt => opt.SecurityConfiguration = new SecurityConfiguration { HostAuthenticationType = "test", AdminRoleName = "admin" })
                .AddIdentityMangerService<InMemoryIdentityManagerService>();

            services.AddAuthentication()
                .AddCookie("test", opt => opt.Cookie.Path = "/");

            var rand = new Random();
            services.AddSingleton(x => Users.Get(rand.Next(5000, 20000)));
            services.AddSingleton(x => Roles.Get(rand.Next(15)));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseIdentityManager();

            app.Map("/account/login",
                login => login.Use(async (context, func) =>
                {
                    await context.SignInAsync("test",
                        new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim("role", "admin") }, "test")));
                    context.Response.Redirect("/");
                }));
        }
    }
}
