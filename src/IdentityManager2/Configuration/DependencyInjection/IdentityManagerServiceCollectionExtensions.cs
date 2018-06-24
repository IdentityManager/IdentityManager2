using System;
using IdentityManager2;
using IdentityManager2.Configuration;
using IdentityManager2.Configuration.Hosting;
using IdentityManager2.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityManagerServiceCollectionExtensions
    {
        public static IIdentityManagerBuilder AddIdentityManagerBuilder(this IServiceCollection services)
        {
            return new IdentityManagerBuilder(services);
        }

        public static IIdentityManagerBuilder AddIdentityManager(this IServiceCollection services)
        {
            var builder = services.AddIdentityManagerBuilder();

            builder.Services.AddMvc();

            builder.Services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.TryAddSingleton<IUrlHelper>(x =>
            {
                var actionContext = x.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            builder.Services.AddTransient<ITokenProvider<AuthenticationTicket>, AuthenticationTicketTokenProvider>();

            builder.Services.AddOptions();

            var identityManagerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<IdentityManagerOptions>>().Value;
            identityManagerOptions.Validate();

            builder.Services.AddSingleton(identityManagerOptions);
            
            builder.Services.AddAuthorization(options =>
            {
                var policy = options.GetPolicy(Constants.IdMgrAuthPolicy);
                if (policy != null) throw new Exception($"Authorization policy with name {Constants.IdMgrAuthPolicy} already exists");

                options.AddPolicy(Constants.IdMgrAuthPolicy, config =>
                {
                    config.RequireClaim(identityManagerOptions.SecurityConfiguration.RoleClaimType, identityManagerOptions.SecurityConfiguration.AdminRoleName);
                    config.AddAuthenticationSchemes(identityManagerOptions.SecurityConfiguration.BearerAuthenticationType);
                });
            });

            services.AddAuthentication()
                .AddScheme<LocalBearerAuthenticationOptions, LocalBearerAuthenticationHandler>(Constants.BearerAuthenticationType, opt => { });
            identityManagerOptions.SecurityConfiguration.Configure(services);
            
            return builder;
        }

        public static IIdentityManagerBuilder AddIdentityManager(this IServiceCollection services, Action<IdentityManagerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityManager();
        }

        public static IIdentityManagerBuilder AddIdentityMangerService<T>(this IIdentityManagerBuilder builder)
            where T : class, IIdentityManagerService
        {
            builder.Services.AddTransient<IIdentityManagerService, T>();
            return builder;
        }
    }
}