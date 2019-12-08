using Microsoft.AspNetCore.Authentication;

namespace IdentityManager2.Configuration.Hosting
{
    internal class LocalhostAuthenticationOptions : AuthenticationSchemeOptions
    {
        public SecurityConfiguration Configuration { get; set; } 
            = new LocalhostSecurityConfiguration();
    }
}