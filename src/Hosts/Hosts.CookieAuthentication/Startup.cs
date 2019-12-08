using System;
using Hosts.Shared.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Hosts.CookieAuthentication
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory IdentityManagerService (demo only)
            services.AddIdentityManager(options =>
                {
                    options.SecurityConfiguration.HostAuthenticationType = "cookie";
                    options.SecurityConfiguration.HostChallengeType = "cookie";
                })
                .AddIdentityMangerService<InMemoryIdentityManagerService>();

            var rand = new Random();
            services.AddSingleton(x => Users.Get(rand.Next(5000, 20000)));
            services.AddSingleton(x => Roles.Get(rand.Next(15)));

            services.AddAuthentication("cookie")
                .AddCookie("cookie", options =>
                {
                    options.LoginPath = "/login";
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseIdentityManager();

            app.UseEndpoints(x =>
            {
                x.MapDefaultControllerRoute();
            });
        }
    }
}
