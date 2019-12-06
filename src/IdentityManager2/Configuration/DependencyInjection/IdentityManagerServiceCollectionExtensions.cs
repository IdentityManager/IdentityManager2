using System;
using System.Linq;
using IdentityManager2;
using IdentityManager2.Api.Controllers;
using IdentityManager2.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityManagerServiceCollectionExtensions
    {
        public static IIdentityManagerBuilder AddIdentityManager(this IServiceCollection services, Action<IdentityManagerOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddIdentityManager();
        }

        public static IIdentityManagerBuilder AddIdentityManager(this IServiceCollection services)
        {
            var identityManagerOptions = services.BuildServiceProvider().GetRequiredService<IOptions<IdentityManagerOptions>>().Value;
            identityManagerOptions.Validate();

            var builder = services.AddIdentityManagerBuilder();

            if (string.IsNullOrEmpty(identityManagerOptions.SecurityConfiguration.PageRouteAttribute))
                builder.Services.AddMvc();
            else
            {
                builder.Services.AddMvc(opt =>
                {
                    opt.UseCentralRoutePrefix(new RouteAttribute(identityManagerOptions.SecurityConfiguration.PageRouteAttribute));
                });
            }
            builder.Services.AddOptions();
            builder.Services.AddSingleton(identityManagerOptions);

            builder.Services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.TryAddSingleton<IUrlHelper>(x =>
            {
                var actionContext = x.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });
            
            builder.Services.AddAuthorization(options =>
            {
                var policy = options.GetPolicy(IdentityManagerConstants.IdMgrAuthPolicy);
                if (policy != null) throw new InvalidOperationException($"Authorization policy with name {IdentityManagerConstants.IdMgrAuthPolicy} already exists");

                options.AddPolicy(IdentityManagerConstants.IdMgrAuthPolicy, config =>
                {
                    config.RequireClaim(identityManagerOptions.SecurityConfiguration.RoleClaimType, identityManagerOptions.SecurityConfiguration.AdminRoleName);

                    if (!string.IsNullOrEmpty(identityManagerOptions.SecurityConfiguration.AuthenticationScheme))
                        config.AddAuthenticationSchemes(identityManagerOptions.SecurityConfiguration.AuthenticationScheme);
                });
            });

            if (!string.IsNullOrEmpty(identityManagerOptions.SecurityConfiguration.AuthenticationScheme))
            {
                services.AddAuthentication()
                    .AddCookie(identityManagerOptions.SecurityConfiguration.AuthenticationScheme, options =>
                    {
                        options.Cookie.SameSite = SameSiteMode.Strict;
                        options.Cookie.HttpOnly = true;
                        options.Cookie.IsEssential = true;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                        options.LoginPath = "/api/login";
                    });
            }

            identityManagerOptions.SecurityConfiguration.Configure(services);
            
            return builder;
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

    class RouteConvention : IApplicationModelConvention
    {
        private readonly IRouteTemplateProvider _routeTemplateProvider;

        public RouteConvention(IRouteTemplateProvider routeTemplateProvider)
        {
            _routeTemplateProvider = routeTemplateProvider;
        }

        public void Apply(ApplicationModel application)
        {
            var matchedSelectors = application.Controllers.FirstOrDefault(c => c.ControllerType == typeof(PageController))?.Selectors;
            if (matchedSelectors != null && matchedSelectors.Any())
            {
                var centralPrefix = new AttributeRouteModel(_routeTemplateProvider);
                foreach (var selectorModel in matchedSelectors)
                {
                    selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(centralPrefix,
                        selectorModel.AttributeRouteModel);
                }
            }
        }
    }

    static class MvcOptionsExtensions
    {
        public static void UseCentralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Insert(0, new RouteConvention(routeAttribute));
        }
    }
}