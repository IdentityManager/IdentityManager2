using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityManager2.Configuration.Hosting
{
    internal class LocalBearerAuthenticationHandler : AuthenticationHandler<LocalBearerAuthenticationOptions>
    {
        private readonly ITokenProvider<AuthenticationTicket> tokenProvider;

        public LocalBearerAuthenticationHandler(IOptionsMonitor<LocalBearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, ITokenProvider<AuthenticationTicket> tokenProvider) 
            : base(options, logger, encoder, clock)
        {
            this.tokenProvider = tokenProvider;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = GetTokenFromHeader(Request);
            
            if (string.IsNullOrWhiteSpace(token)) return Task.FromResult(AuthenticateResult.Fail("No token present in Authorization header"));

            var authenticationTicket = tokenProvider.Validate(token);
            
            if (authenticationTicket == null)
                return Task.FromResult(AuthenticateResult.Fail("Cannot validate bearer token"));

            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }

        private static string GetTokenFromHeader(HttpRequest request)
        {
            var authorization = request.Headers["Authorization"].FirstOrDefault();

            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authorization.Substring("Bearer".Length + 1).Trim();
            }

            return null;
        }
    }

    public class LocalBearerAuthenticationOptions : AuthenticationSchemeOptions
    {
    }
}