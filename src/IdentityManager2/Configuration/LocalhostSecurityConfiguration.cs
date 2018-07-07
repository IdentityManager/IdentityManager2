using IdentityManager2.Configuration.Hosting;
using IdentityManager2.Core;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityManager2.Configuration
{
    public class LocalhostSecurityConfiguration : SecurityConfiguration
    {
        public LocalhostSecurityConfiguration()
        {
            HostAuthenticationType = Constants.LocalAuthenticationType;
            HostChallengeType = Constants.LocalAuthenticationType;
        }

        public override void Configure(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<LocalhostAuthenticationOptions, LocalhostAuthenticationHandler>(
                    HostAuthenticationType, opt => { });

            base.Configure(services);
        }
    }
}