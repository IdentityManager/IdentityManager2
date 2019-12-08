using System;
using Hosts.Shared.InMemory;
using IdentityManager2.Assets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

            app.UseFileServer(new FileServerOptions
            {
                RequestPath = new PathString("/assets"),
                FileProvider = new EmbeddedFileProvider(typeof(EmbeddedHtmlResult).Assembly, "IdentityManager2.Assets")
            });

            app.UseEndpoints(x =>
            {
                //x.MapIdentityManager("/idm");
                x.MapDefaultControllerRoute();
            });
        }
    }
}
