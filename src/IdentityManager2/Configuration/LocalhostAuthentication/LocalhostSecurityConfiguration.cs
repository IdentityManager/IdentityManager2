using IdentityManager2.Configuration.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityManager2.Configuration
{
    public class LocalhostSecurityConfiguration : SecurityConfiguration
    {
        public LocalhostSecurityConfiguration()
        {
            HostAuthenticationType = IdentityManagerConstants.LocalAuthenticationType;
            HostChallengeType = IdentityManagerConstants.LocalAuthenticationType;
            ShowLoginButton = false;
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<LocalhostAuthenticationOptions, LocalhostAuthenticationHandler>(
                    IdentityManagerConstants.LocalAuthenticationType, opt => { });

            base.Configure(services);
        }
    }
}