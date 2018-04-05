using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Host.InMemory;
using IdentityManager2.Configuration;

namespace Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory IdentityManagerService (demo only)
            services.AddIdentityManager()
                .AddIdentityMangerService<InMemoryIdentityManagerService>();

            var rand = new Random();
            services.AddSingleton(x => Users.Get(rand.Next(5000, 20000)));
            services.AddSingleton(x => Roles.Get(rand.Next(15)));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseIdentityManager();

            app.UseMvcWithDefaultRoute();
        }
    }
}
