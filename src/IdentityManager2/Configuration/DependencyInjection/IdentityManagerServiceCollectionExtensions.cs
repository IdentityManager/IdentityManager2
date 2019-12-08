using System;
using System.Threading.Tasks;
using IdentityManager2;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityManagerServiceCollectionExtensions
    {
        public static IIdentityManagerBuilder AddIdentityManager(this IServiceCollection services, Action<IdentityManagerOptions> optionsAction = null)
        {
            services.Configure(optionsAction ?? (options => { }));

            var identityManagerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<IdentityManagerOptions>>().Value;
            identityManagerOptions.Validate();

            services.AddControllersWithViews()
                .AddNewtonsoftJson();
            
            // IdentityManager API authentication scheme
            services.AddAuthentication()
                .AddCookie(IdentityManagerConstants.LocalApiScheme, options =>
                {
                    options.Cookie.Name = IdentityManagerConstants.LocalApiScheme;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                    // TODO: API Cookie: SlidingExpiration
                    // TODO: API Cookie: ExpireTimeSpan

                    options.LoginPath = "/api/login";

                    options.Events.OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    };
                });

            // IdentityManager API authorization scheme
            services.AddAuthorization(options =>
            {
                var policy = options.GetPolicy(IdentityManagerConstants.IdMgrAuthPolicy);
                if (policy != null) throw new InvalidOperationException($"Authorization policy with name {IdentityManagerConstants.IdMgrAuthPolicy} already exists");

                options.AddPolicy(IdentityManagerConstants.IdMgrAuthPolicy, config =>
                {
                    // IdentityManager role
                    config.RequireClaim(identityManagerOptions.SecurityConfiguration.RoleClaimType, identityManagerOptions.SecurityConfiguration.AdminRoleName);

                    // IdentityManager authentication scheme
                    config.AddAuthenticationSchemes(IdentityManagerConstants.LocalApiScheme);
                });
            });

            identityManagerOptions.SecurityConfiguration.Configure(services);

            return new IdentityManagerBuilder(services);
        }

        public static IIdentityManagerBuilder AddIdentityMangerService<T>(this IIdentityManagerBuilder builder)
            where T : class, IIdentityManagerService
        {
            builder.Services.AddTransient<IIdentityManagerService, T>();
            return builder;
        }

        public static IIdentityManagerBuilder AddIdentityManagerBuilder(this IServiceCollection services)
        {
            return new IdentityManagerBuilder(services);
        }
    }
}