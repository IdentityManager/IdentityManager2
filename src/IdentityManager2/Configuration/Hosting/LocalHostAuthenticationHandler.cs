using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using IdentityManager2.Resources;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityManager2.Configuration.Hosting
{
    internal class LocalhostAuthenticationHandler : AuthenticationHandler<LocalhostAuthenticationOptions>, IAuthenticationSignOutHandler
    {
        public LocalhostAuthenticationHandler(IOptionsMonitor<LocalhostAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ctx = Context;
            var localAddresses = new [] { "127.0.0.1", "::1", ctx.Connection.LocalIpAddress.ToString() };
            if (localAddresses.Contains(ctx.Connection.RemoteIpAddress.ToString()))
            {
                var id = new ClaimsIdentity(Options.Configuration.HostAuthenticationType, Options.Configuration.NameClaimType, Options.Configuration.RoleClaimType);
                id.AddClaim(new Claim(Options.Configuration.NameClaimType, Messages.LocalUsername));
                id.AddClaim(new Claim(Options.Configuration.RoleClaimType, Options.Configuration.AdminRoleName));

                var ticket = new AuthenticationTicket(new ClaimsPrincipal(id), Options.Configuration.HostAuthenticationType);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Request not coming from localhost"));
        }

        public Task SignOutAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }
    }
}